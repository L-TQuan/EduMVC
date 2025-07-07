using EduMVC.Data.Configurations;
using EduMVC.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduMVC.Data
{
    public class EduDbContext : DbContext
    {
        public EduDbContext(DbContextOptions<EduDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new CourseConfiguration());

            //base.OnModelCreating(modelBuilder);
        }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<PreviewMedium> PreviewMedia { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Medium> Media { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Rating> Ratings { get; set; }
    }
}
