using Microsoft.EntityFrameworkCore;

namespace Karent.DataModel
{
    public partial class KarentDBContext : DbContext
    {
        public KarentDBContext()
        {
        }

        public KarentDBContext(DbContextOptions<KarentDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Car> Cars { get; set; } = null!;
        public virtual DbSet<Rental> Rentals { get; set; } = null!;
        public virtual DbSet<RentalReturn> RentalReturns { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>(entity =>
            {
                entity.ToTable("cars");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Brand)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("brand");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LateRatePerDay)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("late_rate_per_day");

                entity.Property(e => e.Model)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("model");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_on");

                entity.Property(e => e.PlateNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("plate_number");

                entity.Property(e => e.RentalRatePerDay)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("rental_rate_per_day");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("status")
                    .HasDefaultValueSql("('available')");

                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<Rental>(entity =>
            {
                entity.ToTable("rentals");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CarId).HasColumnName("car_id");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_on");

                entity.Property(e => e.EndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("end_date");

                entity.Property(e => e.StartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("start_date");

                entity.Property(e => e.TotalFee)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total_fee");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Car)
                    .WithMany(p => p.Rentals)
                    .HasForeignKey(d => d.CarId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__rentals__car_id__4316F928");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Rentals)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__rentals__user_id__4222D4EF");
            });

            modelBuilder.Entity<RentalReturn>(entity =>
            {
                entity.ToTable("rental_returns");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.LateFee)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("late_fee");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_on");

                entity.Property(e => e.RentalId).HasColumnName("rental_id");

                entity.Property(e => e.ReturnDate)
                    .HasColumnType("datetime")
                    .HasColumnName("return_date");

                entity.Property(e => e.TotalFee)
                    .HasColumnType("decimal(10, 2)")
                    .HasColumnName("total_fee");

                entity.HasOne(d => d.Rental)
                    .WithMany(p => p.RentalReturns)
                    .HasForeignKey(d => d.RentalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__rental_re__renta__46E78A0C");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Address)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("address");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("created_on")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DrivingLicenseNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("driving_license_number");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_on");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("phone_number");

                entity.Property(e => e.UserType)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("user_type")
                    .HasDefaultValueSql("('customer')");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
