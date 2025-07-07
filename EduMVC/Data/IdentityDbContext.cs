using EduMVC.Areas.Identity.Data;
using EduMVC.Areas.Identity.Data.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MusicMVC.Areas.Identity.Data.Configurations;

namespace EduMVC.Data;

public class IdentityDbContext : IdentityDbContext<EduUser>
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .ApplyConfiguration(new RoleConfiguration())
            .ApplyConfiguration(new ProfileDocumentConfiguration());

        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

    public DbSet<ProfileDocument> ProfileDocuments { get; set; }
}
