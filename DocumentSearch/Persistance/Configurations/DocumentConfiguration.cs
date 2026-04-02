using DocumentSearch.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentSearch.Persistance.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .ValueGeneratedNever();

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Content)
                .IsRequired();

            builder.HasGeneratedTsVectorColumn(d => d.SearchVector, "simple", d => new { d.FileName, d.Content })
                .HasIndex(d => d.SearchVector)
                .HasMethod("GIN");
        }
    }
}
