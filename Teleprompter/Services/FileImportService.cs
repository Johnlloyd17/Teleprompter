namespace Teleprompter.Services
{
    public class ImportedScript
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public interface IFileImportService
    {
        Task<ImportedScript?> PickAndImportAsync();
    }

    public class FileImportService : IFileImportService
    {
        public async Task<ImportedScript?> PickAndImportAsync()
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a script file",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".txt", ".docx", ".pdf" } },
                    { DevicePlatform.Android, new[] { "text/plain", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/pdf" } },
                    { DevicePlatform.iOS, new[] { "public.plain-text", "org.openxmlformats.wordprocessingml.document", "com.adobe.pdf" } },
                    { DevicePlatform.MacCatalyst, new[] { "public.plain-text", "org.openxmlformats.wordprocessingml.document", "com.adobe.pdf" } },
                }),
            });

            if (result is null)
                return null;

            try
            {
                var content = await ReadContentAsync(result);

                return new ImportedScript
                {
                    Title = Path.GetFileNameWithoutExtension(result.FileName),
                    Content = content,
                };
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Import failed", ex.Message, "OK");
                return null;
            }
        }

        private static async Task<string> ReadContentAsync(FileResult result)
        {
            await using var stream = await result.OpenReadAsync();

            var extension = Path.GetExtension(result.FileName).ToLowerInvariant();
            return extension switch
            {
                ".txt" => await ReadTxtAsync(stream),
                ".docx" => await ReadDocxAsync(stream),
                ".pdf" => await ReadPdfAsync(stream),
                _ => throw new NotSupportedException($"Unsupported file type: {extension}"),
            };
        }

        private static async Task<string> ReadTxtAsync(Stream stream)
        {
            using var reader = new StreamReader(stream);
            return (await reader.ReadToEndAsync()).Trim();
        }

        private static async Task<string> ReadDocxAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var document = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(memoryStream, false);
            var body = document.MainDocumentPart?.Document?.Body;
            if (body is null)
                return string.Empty;

            var builder = new System.Text.StringBuilder();
            foreach (var paragraph in body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                builder.AppendLine(paragraph.InnerText);

            return builder.ToString().Trim();
        }

        private static async Task<string> ReadPdfAsync(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            using var pdf = UglyToad.PdfPig.PdfDocument.Open(bytes);
            var builder = new System.Text.StringBuilder();
            foreach (var page in pdf.GetPages())
                builder.AppendLine(page.Text);

            return builder.ToString().Trim();
        }
    }
}
