using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MusicMVC.Areas.Identity.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole { Id = "84b99fe4-a339-4d06-8a9f-b95b16a1bf57", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "351a426e-e9d8-4c3e-9378-b8514bf6e0f8", Name = "Teacher", NormalizedName = "TEACHER" },
                new IdentityRole { Id = "afefdf8d-fd15-446a-b318-b5d719d4a46d", Name = "Student", NormalizedName = "STUDENT" }
            );
        }
    }
}
