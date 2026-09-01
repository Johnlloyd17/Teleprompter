using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace Teleprompter.Services
{
    public interface IFileExportService
    {
        Task ExportTxtAsync(string title, string content);
        Task ExportPdfAsync(string title, string content);
    }

    public class FileExportService : IFileExportService
    {
        private const string FontName = "OpenSans";

        static FileExportService()
        {
            try
            {
                GlobalFontSettings.FontResolver = new OpenSansFontResolver();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Teleprompter: failed to load export font: {ex}");
            }
        }

        public async Task ExportTxtAsync(string title, string content)
        {
            try
            {
                var safeTitle = SanitizeTitle(title);
                var path = Path.Combine(FileSystem.CacheDirectory, $"{safeTitle}.txt");
                await File.WriteAllTextAsync(path, content);
                await ShareFileAsync(path);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Export failed", ex.Message, "OK");
            }
        }

        public async Task ExportPdfAsync(string title, string content)
        {
            try
            {
                var safeTitle = SanitizeTitle(title);
                var path = Path.Combine(FileSystem.CacheDirectory, $"{safeTitle}.pdf");
                await Task.Run(() => GeneratePdf(path, title, content));
                await ShareFileAsync(path);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Export failed", ex.Message, "OK");
            }
        }

        private static void GeneratePdf(string path, string title, string content)
        {
            const double margin = 48;

            var document = new PdfDocument();
            document.Info.Title = title;

            var titleFont = new XFont(FontName, 18, XFontStyle.Bold);
            var bodyFont = new XFont(FontName, 12);
            var titleLineHeight = titleFont.GetHeight();
            var bodyLineHeight = bodyFont.GetHeight();

            var page = document.AddPage();
            page.Size = PageSize.A4;

            double pageWidth = page.Width.Point;
            double pageHeight = page.Height.Point;
            double usableWidth = pageWidth - (margin * 2);

            var gfx = XGraphics.FromPdfPage(page);
            var measure = XGraphics.CreateMeasureContext(new XSize(pageWidth, pageHeight), XGraphicsUnit.Point, XPageDirection.Downwards);

            gfx.DrawString(title, titleFont, XBrushes.Black,
                new XRect(margin, margin, usableWidth, titleLineHeight), XStringFormats.TopCenter);

            double y = margin + titleLineHeight + 16;

            foreach (var paragraph in content.Replace("\r\n", "\n").Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                {
                    y += bodyLineHeight * 0.6;
                    continue;
                }

                foreach (var line in WrapParagraph(paragraph, bodyFont, usableWidth, measure))
                {
                    if (y + bodyLineHeight > pageHeight - margin)
                    {
                        page = document.AddPage();
                        page.Size = PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                    }

                    gfx.DrawString(line, bodyFont, XBrushes.Black, new XPoint(margin, y), XStringFormats.TopLeft);
                    y += bodyLineHeight;
                }

                y += bodyLineHeight * 0.6;
            }

            document.Save(path);
            document.Dispose();
        }

        private static List<string> WrapParagraph(string paragraph, XFont font, double usableWidth, XGraphics measure)
        {
            var lines = new List<string>();
            var current = string.Empty;

            foreach (var word in paragraph.Split(' '))
            {
                var candidate = current.Length == 0 ? word : current + " " + word;

                if (current.Length == 0 || measure.MeasureString(candidate, font).Width <= usableWidth)
                    current = candidate;
                else
                {
                    lines.Add(current);
                    current = word;
                }
            }

            if (current.Length > 0)
                lines.Add(current);

            return lines;
        }

        private static async Task ShareFileAsync(string path)
        {
            var request = new ShareFileRequest
            {
                Title = "Share script",
                File = new ShareFile(path),
            };

            await Share.Default.RequestAsync(request);
        }

        private static string SanitizeTitle(string title)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var safe = new string(title.Where(c => !invalid.Contains(c)).ToArray());
            return string.IsNullOrWhiteSpace(safe) ? "script" : safe;
        }

        private sealed class OpenSansFontResolver : IFontResolver
        {
            private static readonly byte[] FontBytes = LoadFontBytes();

            public string DefaultFontName => FontName;

            public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
                => new FontResolverInfo(FontName, isBold, isItalic);

            public byte[] GetFont(string faceName) => FontBytes;

            private static byte[] LoadFontBytes()
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("ExportFonts/OpenSans-Regular.ttf").GetAwaiter().GetResult();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
