using NpgsqlTypes;

namespace DocumentSearch.Models
{
    public class Document
    {
        public required Guid Id { get; set; }
        public required string FileName { get; set; }
        public required string Content { get; set; }

        public NpgsqlTsVector SearchVector { get; set; }
    }
}
