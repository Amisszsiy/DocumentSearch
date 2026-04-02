namespace DocumentSearch.Models
{
    public class DocumentIndexingRequest
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
    }
}
