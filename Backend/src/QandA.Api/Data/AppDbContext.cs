using Microsoft.EntityFrameworkCore;
using QandA.Api.Domain;

namespace QandA.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyOption> Options => Set<SurveyOption>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<UserAvatar> UserAvatars => Set<UserAvatar>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(u => u.Email).HasMaxLength(Limits.EmailMax);
            e.Property(u => u.NormalizedEmail).HasMaxLength(Limits.EmailMax);
            e.HasIndex(u => u.NormalizedEmail).IsUnique();
            e.Property(u => u.DisplayName).HasMaxLength(Limits.DisplayNameMax);
            e.Property(u => u.SecurityStamp).HasMaxLength(32);
            e.Property(u => u.Locale).HasMaxLength(5);
        });

        b.Entity<UserAvatar>(e =>
        {
            e.HasKey(a => a.UserId);
            e.Property(a => a.ContentType).HasMaxLength(32);
            e.HasOne(a => a.User).WithOne().HasForeignKey<UserAvatar>(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<UserToken>(e =>
        {
            e.Property(t => t.TokenHash).HasMaxLength(64);
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasIndex(t => new { t.UserId, t.Purpose });
            e.Property(t => t.Purpose).HasConversion<string>().HasMaxLength(32);
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Survey>(e =>
        {
            e.Property(s => s.Title).HasMaxLength(Limits.TitleMax);
            e.Property(s => s.Description).HasMaxLength(Limits.DescriptionMax);
            e.HasOne(s => s.Author).WithMany().HasForeignKey(s => s.AuthorId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => s.AuthorId);
            e.HasIndex(s => s.PublishedAt);
        });

        b.Entity<SurveyOption>(e =>
        {
            e.Property(o => o.Text).HasMaxLength(Limits.OptionTextMax);
            e.HasOne(o => o.Survey).WithMany(s => s.Options).HasForeignKey(o => o.SurveyId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(o => o.AddedBy).WithMany().HasForeignKey(o => o.AddedById).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(o => new { o.SurveyId, o.Position });
        });

        b.Entity<Vote>(e =>
        {
            e.HasKey(v => new { v.OptionId, v.UserId });
            e.HasOne(v => v.Option).WithMany(o => o.Votes).HasForeignKey(v => v.OptionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.User).WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Survey).WithMany(s => s.Votes).HasForeignKey(v => v.SurveyId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(v => new { v.SurveyId, v.UserId });
        });
    }
}
