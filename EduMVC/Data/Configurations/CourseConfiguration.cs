using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduMVC.Data.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Entities.Course>
    {
        public void Configure(EntityTypeBuilder<Entities.Course> builder)
        {
            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.Description)
                .HasMaxLength(200);

            builder.Property(c => c.OwnerId)
                    .IsRequired()
                    .HasMaxLength(128);
        }
    }
}
