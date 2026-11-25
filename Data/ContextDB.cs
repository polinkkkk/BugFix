using BugFix.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugFix.Api.Data;

public class ContextDB : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public ContextDB(DbContextOptions<ContextDB> options) : base(options) {}

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasIndex(user => user.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique();

        builder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");

            entity.HasKey(project => project.Id);

            entity.Property(project => project.Name).IsRequired();

            entity.Property(project => project.RepositoryUrl).IsRequired();

            entity.HasIndex(project => project.Name).IsUnique();
        });

        builder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("ProjectMembers");

            entity.HasKey(member => member.Id);

            entity.HasIndex(member => new
            {
                member.ProjectId,
                member.UserId
            })
                .IsUnique();

            entity.HasOne(member => member.Project)
                .WithMany(project => project.Members)
                .HasForeignKey(member => member.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(member => member.User)
                .WithMany()
                .HasForeignKey(member => member.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasDiscriminator<string>("Role")
                .HasValue<Tester>("Tester")
                .HasValue<Developer>("Developer")
                .HasValue<TeamLead>("TeamLead")
                .HasValue<Teacher>("Teacher");
        });
    }
}