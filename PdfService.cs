
using System;
using System.IO;
using ClosedXML.Excel;
using LeatexApp.Models;

namespace LeatexApp.Services
{
    public static class ExcelService
    {
        public static string ExportStanjeToExcel(StanjeSkladista stanje)
        {
            var path = Path.Combine(StorageService.DataDir, $"stanje_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
            using(var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("Stanje");
                ws.Cell(1,1).Value = "Šifra"; ws.Cell(1,2).Value = "Naziv"; ws.Cell(1,3).Value = "Količina";
                int r = 2;
                foreach (var it in stanje.Stanja)
                {
                    ws.Cell(r,1).Value = it.Sifra;
                    ws.Cell(r,2).Value = it.Naziv;
                    ws.Cell(r,3).Value = it.Kolicina;
                    r++;
                }
                ws.Columns().AdjustToContents();
                wb.SaveAs(path);
            }
            return path;
        }
    }
}
