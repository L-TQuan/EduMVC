using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduMVC.Areas.Identity.Data.Configurations
{
    public class ProfileDocumentConfiguration : IEntityTypeConfiguration<ProfileDocument>
    {
        public void Configure(EntityTypeBuilder<ProfileDocument> builder)
        {
            builder.Property(pd => pd.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(pd => pd.FileName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(pd => pd.Extension)
                .HasMaxLength(10);

            builder.HasOne(pd => pd.User)
                .WithOne(u => u.ProfileDocument)
                .HasForeignKey<ProfileDocument>(pd => pd.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
