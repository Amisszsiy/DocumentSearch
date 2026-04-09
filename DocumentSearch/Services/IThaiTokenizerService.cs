namespace DocumentSearch.Services
{
    public interface IThaiTokenizerService
    {
        Task<string> TokenizeAsync(string text);
        Task<string[]> TokenizeBatchAsync(string[] texts);
    }
}
