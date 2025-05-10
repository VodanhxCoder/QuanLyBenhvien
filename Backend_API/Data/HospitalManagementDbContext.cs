using Backend_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_API.Data
{
    public class HospitalManagementDbContext : DbContext
    {
        public HospitalManagementDbContext(DbContextOptions<HospitalManagementDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; } // Thêm Code verification 
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình mối quan hệ 1-1 giữa User và Doctor (tùy chọn)
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserID)
                .IsRequired(false); // Làm cho mối quan hệ là tùy chọn

            // Cấu hình mối quan hệ 1-1 giữa User và Patient (tùy chọn)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserID)
                .IsRequired(false); // Làm cho mối quan hệ là tùy chọn

            // Đảm bảo UserID là duy nhất trong Doctor và Patient
            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.UserID)
                .IsUnique();

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.UserID)
                .IsUnique();
        }
    }
}