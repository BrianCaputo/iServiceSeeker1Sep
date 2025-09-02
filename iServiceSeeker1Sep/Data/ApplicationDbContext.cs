using iServiceSeeker1Sep.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets for all your custom entities
    public DbSet<EndUserProfile> EndUserProfiles { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyMembership> CompanyMemberships { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<ServiceProviderProfile> ServiceProviderProfiles { get; set; }
    public DbSet<ServiceProviderServiceArea> ServiceProviderServiceAreas { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<StateProvince> StateProvinces { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- Configure the ApplicationUser to EndUserProfile relationship (One-to-One) ---
        builder.Entity<ApplicationUser>()
            .HasOne(au => au.EndUserProfile)
            .WithOne(eup => eup.User)
            .HasForeignKey<EndUserProfile>(eup => eup.UserId);

        // --- Configure the CompanyMembership (Many-to-Many between ApplicationUser and Company) ---
        builder.Entity<CompanyMembership>()
            .HasKey(cm => cm.Id); // Use the Guid Id as the primary key

        builder.Entity<CompanyMembership>()
            .HasOne(cm => cm.ApplicationUser)
            .WithMany(au => au.CompanyMemberships) // Correctly links to the collection on ApplicationUser
            .HasForeignKey(cm => cm.ApplicationUserId);

        builder.Entity<CompanyMembership>()
            .HasOne(cm => cm.Company)
            .WithMany(c => c.Members) // Correctly links to the collection on Company
            .HasForeignKey(cm => cm.CompanyId);

        // --- Configure the ServiceProviderServiceArea (Many-to-Many between ServiceProviderProfile and ServiceCategory) ---
        builder.Entity<ServiceProviderServiceArea>()
           .HasKey(spsa => spsa.Id);

        builder.Entity<ServiceProviderServiceArea>()
            .HasOne(spsa => spsa.ServiceProviderProfile)
            .WithMany(sp => sp.ServiceAreas)
            .HasForeignKey(spsa => spsa.ServiceProviderProfileId);

        builder.Entity<ServiceProviderServiceArea>()
            .HasOne(spsa => spsa.ServiceCategory)
            .WithMany(sc => sc.ServiceProviderServiceAreas)
            .HasForeignKey(spsa => spsa.ServiceCategoryId);


        // --- Configure the Location and Address (Table-per-Hierarchy Inheritance) ---
        builder.Entity<Location>()
            .ToTable("Locations"); // Explicitly name the table for clarity

        builder.Entity<Address>()
            .HasBaseType<Location>();


        // --- Configure StateProvince to Country relationship (Many-to-One) ---
        builder.Entity<StateProvince>()
            .HasOne(sp => sp.Country)
            .WithMany() // A country can have many states/provinces. No navigation property back needed.
            .HasForeignKey(sp => sp.CountryID);
    }
}