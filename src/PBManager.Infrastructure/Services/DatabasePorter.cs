using Microsoft.EntityFrameworkCore;
using PBManager.Core.Interfaces;
using PBManager.Infrastructure.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PBManager.Infrastructure.Services;

public class DatabasePorter(DatabaseContext dbContext) : IDatabasePorter
{
    private const string APP_SECRET_KEY = "PBManager_fhkV8).iL.f=#\"~wHVB3FQ6+";
    private const string APP_SALT = "PBManager_Salt_v1";
    private const int KEY_DERIVATION_ITERATIONS = 100000;

    private readonly DatabaseContext _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task ExportDatabaseAsync(string destinationPath)
    {
        var exportData = new DatabaseExportData
        {
            Students = await _db.Students.AsNoTracking().ToListAsync(),
            Exams = await _db.Exams.AsNoTracking().ToListAsync(),
            GradeRecords = await _db.GradeRecords.AsNoTracking().ToListAsync(),
            StudyRecords = await _db.StudyRecords.AsNoTracking().ToListAsync(),
            AuditLogs = await _db.AuditLogs.AsNoTracking().ToListAsync(),
            Classes = await _db.Classes.AsNoTracking().ToListAsync(),
            Subjects = await _db.Subjects.AsNoTracking().ToListAsync(),
        };

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(exportData);

        var compressedBytes = await CompressAsync(jsonBytes);
        var encryptedBytes = Encrypt(compressedBytes);
        await File.WriteAllBytesAsync(destinationPath, encryptedBytes);
    }

    public async Task<bool> ImportDatabaseAsync(string sourcePath)
    {
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(sourcePath);
            var compressedBytes = Decrypt(fileBytes);
            var jsonBytes = await DecompressAsync(compressedBytes);

            var importData = JsonSerializer.Deserialize<DatabaseExportData>(jsonBytes);
            if (importData == null) return false;

            using var transaction = await _db.Database.BeginTransactionAsync();

            _db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                _db.Students.RemoveRange(_db.Students);
                await _db.Students.AddRangeAsync(importData.Students);

                _db.Exams.RemoveRange(_db.Exams);
                await _db.Exams.AddRangeAsync(importData.Exams);

                _db.GradeRecords.RemoveRange(_db.GradeRecords);
                await _db.GradeRecords.AddRangeAsync(importData.GradeRecords);

                _db.Classes.RemoveRange(_db.Classes);
                await _db.Classes.AddRangeAsync(importData.Classes);

                _db.StudyRecords.RemoveRange(_db.StudyRecords);
                await _db.StudyRecords.AddRangeAsync(importData.StudyRecords);

                _db.Subjects.RemoveRange(_db.Subjects);
                await _db.Subjects.AddRangeAsync(importData.Subjects);

                _db.AuditLogs.RemoveRange(_db.AuditLogs);
                await _db.AuditLogs.AddRangeAsync(importData.AuditLogs);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            finally
            {
                _db.ChangeTracker.AutoDetectChangesEnabled = true;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Import failed: {ex.Message}");
            return false;
        }
    }

    private static byte[] Encrypt(byte[] dataToEncrypt)
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

    private static byte[] Decrypt(byte[] dataToDecrypt)
    {
        if (dataToDecrypt == null || dataToDecrypt.Length < 16)
        {
            throw new CryptographicException("Invalid encrypted data");
        }
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

    private static async Task<byte[]> CompressAsync(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionMode.Compress, true))
        {
            await gzip.WriteAsync(data);
        }
        return output.ToArray();
    }

    private static async Task<byte[]> DecompressAsync(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        await gzip.CopyToAsync(output);
        return output.ToArray();
    }
}