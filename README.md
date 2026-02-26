
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LeatexApp.Models;

namespace LeatexApp.Services
{
    public static class PdfService
    {
        static float Mm(double mm) => (float)(mm * 72.0 / 25.4);

        static void Header(IContainer container, AppSettings s)
        {
            container.Column(col=>{
                col.Item().Text(s.KompanijaNaziv).Bold().FontSize(14);
                col.Item().Text($"{s.Telefon1} | {s.Telefon2} | {s.Email}").FontSize(9);
                col.Item().LineHorizontal(0.8f);
            });
        }

        static byte[] MakeQrPng(string content)
        {
            using var gen = new QRCodeGenerator();
            var data = gen.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            return png.GetGraphic(4);
        }

        public static string GenerateDeklaracije(string naziv, string sifra, List<string> serijskiList, AppSettings s)
        {
            var file = Path.Combine(StorageService.DataDir, $"deklaracije_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            var pageWidth = Mm(297);
            var pageHeight = Mm(210);

            var labelW = Mm(61);
            var labelH = Mm(192);
            var marginL = Mm(27); var marginR = Mm(27); var marginT = Mm(8); var marginB = Mm(9);

            var usableW = pageWidth - marginL - marginR;
            var totalLabelsW = 4 * labelW;
            var scaleX = totalLabelsW > usableW ? usableW / totalLabelsW : 1f;
            var scaledLabelW = labelW * scaleX;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(pageWidth, pageHeight));
                    page.Margin(20);
                    page.Header().Element(x=> Header(x,s));

                    page.Content().Element(c =>
                    {
                        c.PaddingLeft(marginL).PaddingRight(marginR).PaddingTop(marginT).PaddingBottom(marginB);
                        c.Column(col=>{
                            int idx = 0;
                            while (idx < serijskiList.Count)
                            {
                                col.Item().Row(row =>
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        if (idx < serijskiList.Count)
                                        {
                                            var ser = serijskiList[idx++];
                                            var qrNaziv = MakeQrPng(naziv);
                                            var qrSer = MakeQrPng(ser);
                                            row.RelativeColumn().Element(box =>
                                            {
                                                box.MinWidth(scaledLabelW).Height(labelH);
                                                box.Border(Colors.Grey.Lighten3);
                                                box.Padding(10);
                                                box.Column(inner =>
                                                {
                                                    inner.Item().Text("DEKLARACIJA").Bold().FontSize(12);
                                                    inner.Item().Text($"Artikal: {naziv}").FontSize(10);
                                                    inner.Item().Text($"Šifra: {sifra}").FontSize(10);
                                                    inner.Item().Text($"Serijski: {ser}").FontSize(10);
                                                    inner.Item().Text($"Datum: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                                                    inner.Item().Row(r=>{
                                                        r.ConstantItem(60).Image(qrNaziv);
                                                        r.ConstantItem(60).Image(qrSer);
                                                    });
                                                });
                                            });
                                        }
                                        else
                                        {
                                            row.RelativeColumn();
                                        }
                                    }
                                });
                            }
                        });
                    });
                });
            }).GeneratePdf(file);

            return file;
        }

        public static string GenerateOtpremnica(Otpremnica o, AppSettings s, Dictionary<string,int> sazetak)
        {
            var file = Path.Combine(StorageService.DataDir, $"otpremnica_{o.DatumKreiranja:yyyyMMdd_HHmmss}.pdf");
            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Element(x=> Header(x,s));

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"OTPREMNICA {o.BrojOtpremnice}").Bold().FontSize(13);
                        col.Item().Text("Stavke i serijski brojevi:").Bold();
                        foreach (var st in o.Stavke)
                            col.Item().Text($"- {st.Naziv} ({st.Sifra}): {string.Join(", ", st.Serijski)}").FontSize(10);

                        col.Item().PaddingTop(10).Text("Sažetak količina po šifri:").Bold();
                        foreach (var kv in sazetak)
                            col.Item().Text($"{kv.Key}: {kv.Value} kom");
                        var ukupno = 0; foreach (var v in sazetak.Values) ukupno += v;
                        col.Item().PaddingTop(5).Text($"UKUPNO: {ukupno} kom").Bold();
                    });

                    page.Footer().Row(row =>
                    {
                        row.Spacing(20);
                        row.RelativeItem().Text($"Izdao: {o.Izdao}");
                        row.RelativeItem().Text($"Primio: {o.Primio}");
                    });
                });
            }).GeneratePdf(file);

            return file;
        }
    }
}
