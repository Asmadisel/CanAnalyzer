using Microsoft.EntityFrameworkCore;
using CanAnalyzer.Models;

namespace CanAnalyzer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options) { }

        public DbSet<Sdo> Sdo { get; set; } = null!;
        public DbSet<SdoType> SdoType { get; set; } = null!;
        public DbSet<Status> Status { get; set; } = null!;
        public DbSet<StatusEvent> StatusEvent { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sdo>(entity =>
            {
                entity.ToTable("sdo", "public");
                entity.HasKey(e => e.SdoId).HasName("sdo_pkey");
                entity.Property(e => e.SdoId).HasColumnName("sdo_id");
                entity.Property(e => e.Number).HasColumnName("number");
                entity.Property(e => e.Manufacturer).HasColumnName("manufacturer");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.SdoType).HasColumnName("sdo_type");

                // ⚠️ ЯВНО УКАЗЫВАЕМ СВЯЗЬ
                entity.HasOne(d => d.SdoTypeNavigation)
                    .WithMany(p => p.Sdos)
                    .HasForeignKey(d => d.SdoType)
                    .HasPrincipalKey(p => p.SdoTypeId)
                    .HasConstraintName("fk_sdo_sdo_types");
            });

            modelBuilder.Entity<StatusEvent>(entity =>
            {
                entity.ToTable("status_events", "public");
                entity.HasKey(e => e.Id).HasName("status_events_pkey");
                entity.Property(e => e.Id).HasColumnName("id")
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");
                entity.Property(e => e.Time).HasColumnName("time");
                entity.Property(e => e.StatusCode).HasColumnName("status_code");
                entity.Property(e => e.Sdo).HasColumnName("sdo");
                entity.Property(e => e.RecordedAt).HasColumnName("recorded_at");

                entity.HasOne(d => d.StatusNavigation)
                    .WithMany(p => p.StatusEvents)
                    .HasForeignKey(d => d.StatusCode)
                    .HasPrincipalKey(p => p.StatusCodeId) 
                    .HasConstraintName("fk_status_events_statuses");

                entity.HasOne(d => d.SdoNavigation)
                    .WithMany(p => p.StatusEvents)
                    .HasForeignKey(d => d.Sdo)
                    .HasPrincipalKey(p => p.SdoId) 
                    .HasConstraintName("fk_status_events_sdo");
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.ToTable("statuses", "public");
                entity.HasKey(e => e.StatusCodeId).HasName("statuses_pkey");
                entity.Property(e => e.StatusCodeId).HasColumnName("status_code_id");
                entity.Property(e => e.StatusName).HasColumnName("status_name");
            });

            modelBuilder.Entity<SdoType>(entity =>
            {
                entity.ToTable("sdo_types", "public");
                entity.HasKey(e => e.SdoTypeId);
                entity.Property(e => e.SdoTypeId).HasColumnName("sdo_type_id");
                entity.Property(e => e.SdoTypeName).HasColumnName("sdo_type_name");
            });
        }
    }
}
