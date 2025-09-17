using Microsoft.EntityFrameworkCore;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace PBManager.Infrastructure.Services;

public class DatabasePorter : IDatabasePorter
{
    private const string APP_SECRET_KEY = "PBManager_fhkV8).iL.f=#\"~wHVB3FQ6+";
    private const string APP_SALT = "PBManager_Salt_v1";
    private const int KEY_DERIVATION_ITERATIONS = 50000;
    private const string TEMP_DB_SUFFIX = ".pending_import";
    private const string STARTUP_FLAG_FILE = "db_import_pending.flag";

    private readonly DatabaseContext _db;

    public DatabasePorter(DatabaseContext dbContext)
    {
        _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public static async Task HandlePendingImportOnStartupAsync(string dbPath)
    {
        var flagFile = Path.Combine(Path.GetDirectoryName(dbPath) ?? "", STARTUP_FLAG_FILE);
        var tempDbPath = dbPath + TEMP_DB_SUFFIX;

        if (File.Exists(flagFile) && File.Exists(tempDbPath))
        {
            try
            {
                var backupPath = dbPath + ".backup_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                if (File.Exists(dbPath))
                {
                    File.Copy(dbPath, backupPath, true);
                }

                File.Copy(tempDbPath, dbPath, true);

                File.Delete(tempDbPath);
                File.Delete(flagFile);

                Debug.WriteLine($"Database import completed on startup. Backup saved as: {Path.GetFileName(backupPath)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to complete database import on startup: {ex.Message}");
                try { File.Delete(flagFile); } catch { }
            }
        }
    }

    public async Task ExportDatabaseAsync(string destinationPath)
    {
        var dbBytes = await GetDatabaseBytesAsync();
        var compressedBytes = await CompressAsync(dbBytes);
        var encryptedBytes = Encrypt(compressedBytes);
        await File.WriteAllBytesAsync(destinationPath, encryptedBytes);
    }

    public async Task<bool> ImportDatabaseAsync(string sourcePath)
    {
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(sourcePath);
            var compressedBytes = Decrypt(fileBytes);
            var dbBytes = await DecompressAsync(compressedBytes);

            await StoreForImportOnRestart(dbBytes);
            return true;
        }
        catch (CryptographicException ex)
        {
            Debug.WriteLine($"Decryption failed: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Import failed: {ex.Message}");
            return false;
        }
    }

    public bool IsPendingImportOnRestart()
    {
        var dbPath = _db.Database.GetDbConnection().DataSource;
        var flagFile = Path.Combine(Path.GetDirectoryName(dbPath) ?? "", STARTUP_FLAG_FILE);
        return File.Exists(flagFile);
    }

    private async Task StoreForImportOnRestart(byte[] databaseBytes)
    {
        var dbPath = _db.Database.GetDbConnection().DataSource;
        var tempDbPath = dbPath + TEMP_DB_SUFFIX;
        var flagFile = Path.Combine(Path.GetDirectoryName(dbPath) ?? "", STARTUP_FLAG_FILE);

        await File.WriteAllBytesAsync(tempDbPath, databaseBytes);

        await File.WriteAllTextAsync(flagFile, $"Import scheduled at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    private byte[] Encrypt(byte[] dataToEncrypt)
    {
        using var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;

        var salt = Encoding.UTF8.GetBytes(APP_SALT);
        using var pbkdf2 = new Rfc2898DeriveBytes(APP_SECRET_KEY, salt, KEY_DERIVATION_ITERATIONS, HashAlgorithmName.SHA256);
        aes.Key = pbkdf2.GetBytes(32);
        aes.GenerateIV();

        using var memoryStream = new MemoryStream();
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
            cryptoStream.FlushFinalBlock();
        }

        return memoryStream.ToArray();
    }

    private byte[] Decrypt(byte[] dataToDecrypt)
    {
        using var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;

        byte[] iv = new byte[16];
        Array.Copy(dataToDecrypt, 0, iv, 0, iv.Length);

        var salt = Encoding.UTF8.GetBytes(APP_SALT);
        using var pbkdf2 = new Rfc2898DeriveBytes(APP_SECRET_KEY, salt, KEY_DERIVATION_ITERATIONS, HashAlgorithmName.SHA256);
        aes.Key = pbkdf2.GetBytes(32);
        aes.IV = iv;

        using var encryptedStream = new MemoryStream(dataToDecrypt, 16, dataToDecrypt.Length - 16);
        using var decryptedStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(encryptedStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
        {
            cryptoStream.CopyTo(decryptedStream);
        }

        return decryptedStream.ToArray();
    }

    private async Task<byte[]> GetDatabaseBytesAsync()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await _db.Database.ExecuteSqlRawAsync($"VACUUM INTO '{tempFile}'");
            return await File.ReadAllBytesAsync(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    private async Task<byte[]> CompressAsync(byte[] data)
    {
        using var output = new MemoryStream();
        await using (var gzip = new GZipStream(output, CompressionMode.Compress, true))
        {
            await gzip.WriteAsync(data, 0, data.Length);
        }
        return output.ToArray();
    }

    private async Task<byte[]> DecompressAsync(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        await using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        await gzip.CopyToAsync(output);
        return output.ToArray();
    }
}