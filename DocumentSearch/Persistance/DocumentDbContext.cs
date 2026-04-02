using DocumentSearch.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentSearch.Persistance
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base (options)
        {
            
        }

        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
