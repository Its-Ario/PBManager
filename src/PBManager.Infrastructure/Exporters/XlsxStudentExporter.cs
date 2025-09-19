using ClosedXML.Excel;
using PBManager.Core.Entities;
using PBManager.Core.Interfaces;

namespace PBManager.Infrastructure.Exporters
{
    public class XlsxStudentExporter : IDataExporter<Student>
    {
        public Task ExportAsync(IEnumerable<Student> students, Stream stream)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Students");

            worksheet.Cell(1, 1).Value = "NationalCode";
            worksheet.Cell(1, 2).Value = "FirstName";
            worksheet.Cell(1, 3).Value = "LastName";
            worksheet.Cell(1, 4).Value = "ClassName";
            worksheet.Row(1).Style.Font.Bold = true;

            int row = 2;
            foreach (var s in students)
            {
                worksheet.Cell(row, 1).Value = s.NationalCode;
                worksheet.Cell(row, 2).Value = s.FirstName;
                worksheet.Cell(row, 3).Value = s.LastName;
                worksheet.Cell(row, 4).Value = s.Class?.Name;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(stream);

            return Task.CompletedTask;
        }
    }
}
