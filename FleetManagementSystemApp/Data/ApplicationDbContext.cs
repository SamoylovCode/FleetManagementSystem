using FleetManagementSystemApp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace FleetManagementSystemApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Passport> Passports { get; set; } = null!;
    public DbSet<Insurance> Insurances { get; set; } = null!;
    public DbSet<RegistrationCertificate> RegistrationCertificates { get; set; } = null!;
    public DbSet<CertificateTechInspection> CertificateTechInspections { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = ApplicationRole.Admin,
                NormalizedName = ApplicationRole.Admin.ToUpper()
            },
            new IdentityRole
            {
                Id = "2",
                Name = ApplicationRole.Manager,
                NormalizedName = ApplicationRole.Manager.ToUpper()
            },
            new IdentityRole
            {
                Id = "3",
                Name = ApplicationRole.Dispatcher,
                NormalizedName = ApplicationRole.Dispatcher.ToUpper()
            },
            new IdentityRole
            {
                Id = "4",
                Name = ApplicationRole.Inspector,
                NormalizedName = ApplicationRole.Inspector.ToUpper()
            }
        );

        builder.Entity<Company>()
            .HasMany(c => c.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasOne(c => c.Address)
            .WithOne(a => a.Company)
            .HasForeignKey<Address>(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Company>()
            .HasMany(c => c.Vehicles)
            .WithOne(v => v.Company)
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vehicle>()
            .HasOne(v => v.Passport)
            .WithOne(p => p.Vehicle)
            .HasForeignKey<Passport>(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vehicle>()
            .HasOne(v => v.Insurance)
            .WithOne(i => i.Vehicle)
            .HasForeignKey<Insurance>(i => i.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vehicle>()
            .HasOne(v => v.RegCertificate)
            .WithOne(r => r.Vehicle)
            .HasForeignKey<RegistrationCertificate>(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vehicle>()
            .HasOne(v => v.CertificateTechInspection)
            .WithOne(r => r.Vehicle)
            .HasForeignKey<CertificateTechInspection>(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        /* Concurrency properies */

        builder.Entity<Vehicle>()
            .Property(v => v.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Entity<Insurance>()
            .Property(i => i.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Entity<Passport>()
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Entity<RegistrationCertificate>()
            .Property(r => r.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.Entity<CertificateTechInspection>()
            .Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}