using BookingSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Data;

public class BookingContext : DbContext
{
    public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.PatientId);
            entity.Property(p => p.PatientId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.MiddleName).HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Email).IsRequired().HasMaxLength(255);
            entity.Property(p => p.Phone).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Password).IsRequired();
            entity.Property(p => p.Address).HasMaxLength(500);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(p => p.Email).IsUnique();
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(s => s.SpecializationId);
            entity.Property(s => s.SpecializationId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(s => s.SpecializationName).IsRequired().HasMaxLength(150);
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(d => d.DoctorId);
            entity.Property(d => d.DoctorId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.MiddleName).HasMaxLength(100);
            entity.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Email).IsRequired().HasMaxLength(255);
            entity.Property(d => d.Phone).IsRequired().HasMaxLength(20);
            entity.Property(d => d.LicenseNo).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Password).IsRequired();
            entity.Property(d => d.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(d => d.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(d => d.Email).IsUnique();
            entity.HasIndex(d => d.LicenseNo).IsUnique();

            entity.HasOne(d => d.Specialization)
                .WithMany(s => s.Doctors)
                .HasForeignKey(d => d.SpecializationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(a => a.AdminId);
            entity.Property(a => a.AdminId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(a => a.Username).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Password).IsRequired();
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(a => a.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(a => a.Username).IsUnique();
        });

        modelBuilder.Entity<Receptionist>(entity =>
        {
            entity.HasKey(r => r.ReceptionistId);
            entity.Property(r => r.ReceptionistId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(r => r.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.MiddleName).HasMaxLength(100);
            entity.Property(r => r.LastName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Email).IsRequired().HasMaxLength(255);
            entity.Property(r => r.Phone).IsRequired().HasMaxLength(20);
            entity.Property(r => r.EmployeeNo).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Password).IsRequired();
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(r => r.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(r => r.Email).IsUnique();
            entity.HasIndex(r => r.EmployeeNo).IsUnique();
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(s => s.ScheduleId);
            entity.Property(s => s.ScheduleId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(s => s.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(s => s.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(a => a.AppointmentId);
            entity.Property(a => a.AppointmentId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(a => a.Status).HasConversion<string>().IsRequired().HasMaxLength(50);
            entity.Property(a => a.Reason).HasMaxLength(500);
            entity.Property(a => a.Notes).HasMaxLength(1000);
            entity.Property(a => a.Diagnosis).HasMaxLength(1000);
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(a => a.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(a => a.Schedule)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(a => a.CancellationNote).HasMaxLength(500);
            entity.Property(a => a.CancelledBy).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(a => a.Receptionist)
                .WithMany()
                .HasForeignKey(a => a.ReceptionistId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.RefreshTokenId);
            entity.Property(r => r.RefreshTokenId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(r => r.TokenHash).IsRequired().HasMaxLength(256);
            entity.Property(r => r.UserRole).HasConversion<string>().IsRequired().HasMaxLength(20);
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(r => r.TokenHash).IsUnique();
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.NotificationId);
            entity.Property(n => n.NotificationId).HasDefaultValueSql("NEWSEQUENTIALID()");
            entity.Property(n => n.UserId).IsRequired();
            entity.Property(n => n.UserType).HasConversion<string>().IsRequired().HasMaxLength(20);
            entity.Property(n => n.Subject).IsRequired().HasMaxLength(255);
            entity.Property(n => n.Content).IsRequired();
            entity.Property(n => n.Type).HasConversion<string>().IsRequired().HasMaxLength(50);
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(n => n.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
