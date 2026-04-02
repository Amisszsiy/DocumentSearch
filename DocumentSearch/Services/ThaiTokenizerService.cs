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

        private record TokenizeResponse(string Result);
    }
}
