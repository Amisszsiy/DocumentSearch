using System.Net.Http.Json;

namespace DocumentSearch.Services
{
    public class ThaiTokenizerService : IThaiTokenizerService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ThaiTokenizerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> TokenizeAsync(string text)
        {
            var client = _httpClientFactory.CreateClient("ThaiTokenizer");
            var response = await client.PostAsJsonAsync("/tokenize", new { text });
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TokenizeResponse>();
            return result!.Result;
        }

        public async Task<string[]> TokenizeBatchAsync(string[] texts)
        {
            var client = _httpClientFactory.CreateClient("ThaiTokenizer");
            var response = await client.PostAsJsonAsync("/tokenize/batch", new { texts });
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TokenizeBatchResponse>();
            return result!.Results;
        }

        private record TokenizeResponse(string Result);
        private record TokenizeBatchResponse(string[] Results);
    }
}
