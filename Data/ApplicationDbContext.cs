using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZE.Models;

namespace ZE.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Technology> Technologies => Set<Technology>();
    public DbSet<ProjectTechnology> ProjectTechnologies => Set<ProjectTechnology>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Founder> Founders => Set<Founder>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Title).IsRequired();
            entity.Property(p => p.Slug).IsRequired();
        });

        builder.Entity<Technology>(entity =>
        {
            entity.HasIndex(t => t.Name).IsUnique();
        });

        builder.Entity<ProjectTechnology>(entity =>
        {
            entity.HasKey(pt => new { pt.ProjectId, pt.TechnologyId });

            entity.HasOne(pt => pt.Project)
                .WithMany(p => p.Technologies)
                .HasForeignKey(pt => pt.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Technology)
                .WithMany(t => t.Projects)
                .HasForeignKey(pt => pt.TechnologyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Skill>(entity =>
        {
            entity.HasIndex(s => s.Name).IsUnique();
        });

        builder.Entity<Founder>(entity =>
        {
            entity.Property(f => f.AccentColor).HasDefaultValue("#F5B942");
        });

        builder.Entity<ContactMessage>(entity =>
        {
            entity.HasIndex(m => m.CreatedAt);
            entity.HasIndex(m => m.IsRead);
        });
    }
}
