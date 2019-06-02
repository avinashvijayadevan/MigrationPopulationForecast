using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Pensive
{
    public partial class TravelContext : DbContext
    {
        public TravelContext()
        {
        }

        public TravelContext(DbContextOptions<TravelContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DeltaPopulation> DeltaPopulations { get; set; }
        public virtual DbSet<DestinationAggregation> DestinationAggregations { get; set; }
        public virtual DbSet<OrignAggregation> OrignAggregations { get; set; }
        public virtual DbSet<PassengerInfoNew> PassengerInfoNews { get; set; }
        public virtual DbSet<TravelRawData> TravelRawData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=pensive.database.windows.net;Initial Catalog=Travel;Persist Security Info=True;User ID=vijayav;Password=P@55w0rd");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.0-rtm-35687");

            modelBuilder.Entity<DeltaPopulation>(entity =>
            {
                entity.Property(e => e.Place).IsUnicode(false);
            });

            modelBuilder.Entity<DestinationAggregation>(entity =>
            {
                entity.Property(e => e.Destination).IsUnicode(false);
            });

            modelBuilder.Entity<OrignAggregation>(entity =>
            {
                entity.HasKey(e => e.OriginAggregation)
                    .HasName("pk_OriginAggregation");

                entity.Property(e => e.Origin).IsUnicode(false);
            });

            modelBuilder.Entity<PassengerInfoNew>(entity =>
            {
                entity.HasKey(e => e.PassengerInfoId)
                    .HasName("pk_PassengerInfoId");

                entity.Property(e => e.Destination).IsUnicode(false);

                entity.Property(e => e.Mode).IsUnicode(false);

                entity.Property(e => e.Orign).IsUnicode(false);
            });

            modelBuilder.Entity<TravelRawData>(entity =>
            {
                entity.HasKey(e => e.TravelId)
                    .HasName("PK_TravelId");

                entity.Property(e => e.DateOfBirth).IsUnicode(false);

                entity.Property(e => e.Destination).IsUnicode(false);

                entity.Property(e => e.Origin).IsUnicode(false);

                entity.Property(e => e.TravelDate).IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}