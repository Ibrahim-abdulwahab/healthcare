using Healthcare.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Healthcare.Api.Data.Configurations;

public class HealthcareFileConfiguration
    : IEntityTypeConfiguration<HealthcareFile>
{
    public void Configure(EntityTypeBuilder<HealthcareFile> builder)
    {
        // C# class is HealthcareFile, but PostgreSQL table is "files"
        builder.ToTable("files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.TableName)
            .HasColumnName("table_name")
            .IsRequired();

        builder.Property(f => f.RecordId)
            .HasColumnName("record_id")
            .IsRequired();

        builder.Property(f => f.FileName)
            .HasColumnName("file_name")
            .IsRequired();

        builder.Property(f => f.FileUrl)
            .HasColumnName("file_url")
            .IsRequired();

        builder.Property(f => f.UploadedBy)
            .HasColumnName("uploaded_by")
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // User → Uploaded Files
        builder.HasOne(f => f.Uploader)
            .WithMany(u => u.UploadedFiles)
            .HasForeignKey(f => f.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}