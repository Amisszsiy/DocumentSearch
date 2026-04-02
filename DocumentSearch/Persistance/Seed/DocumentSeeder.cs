using DocumentSearch.Models;
using DocumentSearch.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentSearch.Persistance.Seed
{
    public static class DocumentSeeder
    {
        public static async Task SeedAsync(DocumentDbContext context, IThaiTokenizerService tokenizer, ILogger logger)
        {
            if (context.Documents.Any())
                return;

            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Persistance", "Seed", "Mock.json");
            var json = await File.ReadAllTextAsync(jsonPath);

            var records = JsonSerializer.Deserialize<List<MockRecord>>(json);
            if (records == null || records.Count == 0)
                return;

            var documents = new List<Document>();
            foreach (var record in records)
            {
                documents.Add(new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = record.FileName,
                    Content = await tokenizer.TokenizeAsync(record.FileContent)
                });
            }

            context.Documents.AddRange(documents);
            await context.SaveChangesAsync();

            logger.LogInformation("Seeded {Count} documents.", documents.Count);
        }

        private record MockRecord(
            [property: JsonPropertyName("file_name")] string FileName,
            [property: JsonPropertyName("file_content")] string FileContent
        );
    }
}
