using Microsoft.EntityFrameworkCore;

namespace picture_backend.Models
{
    public class AppDbContext : DbContext
    {
        // 数据表为 Users
        public DbSet<User> Users { get; set; }
        public DbSet<Script> Scripts { get; set; }
        public DbSet<ScriptHistory> ScriptHistories { get; set; }
        public DbSet<VisualElement> VisualElements { get; set; }
        public DbSet<ScriptAnalysis> ScriptAnalyses { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            });

            modelBuilder.Entity<Script>(entity =>
            {
                entity.ToTable("script");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(100);
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
                entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(s => s.User)
                      .WithMany(u => u.Scripts)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            modelBuilder.Entity<ScriptHistory>(entity =>
            {
                entity.ToTable("script_history");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScriptId).HasColumnName("script_id");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.Response).HasColumnName("response");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(h => h.Script)
                      .WithMany(s => s.ScriptHistories)
                      .HasForeignKey(h => h.ScriptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VisualElement>(entity =>
            {
                entity.ToTable("visual_element");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ImageUrl).HasColumnName("image_url");
                entity.Property(e => e.ImageGeneratedAt).HasColumnName("image_generated_at");
                entity.Property(e => e.ScriptId).HasColumnName("script_id");

                entity.HasOne(v => v.Script)
                      .WithMany(s => s.VisualElements)
                      .HasForeignKey(v => v.ScriptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ScriptAnalysis>(entity =>
            {
                entity.ToTable("script_analysis");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.AnalysisResult).HasColumnName("analysis_result");
                entity.Property(e => e.AnalyzedAt).HasColumnName("analyzed_at");
                entity.Property(e => e.ScriptId).HasColumnName("script_id");

                entity.HasOne(sa => sa.Script)
                      .WithOne(s => s.ScriptAnalysis)
                      .HasForeignKey<ScriptAnalysis>(sa => sa.ScriptId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
