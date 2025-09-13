using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceSeeker.Data;
using ServiceProvider = ServiceSeeker.Data.ServiceProvider;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // --- DbSets for all your custom entities ---
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<EndUserProfile> EndUserProfiles { get; set; }
    public DbSet<ServiceProvider> ServiceProviders { get; set; }
    public DbSet<CompanyMembership> CompanyMemberships { get; set; }
    public DbSet<ServiceCategory> ServiceCategories { get; set; }
    public DbSet<ProviderServiceArea> ProviderServiceAreas { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<StateProvince> StateProvinces { get; set; }
    public DbSet<UserAuthenticationHistory> UserAuthenticationHistories { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- User & Profile Configurations ---

        // One-to-One: ApplicationUser -> EndUserProfile
        builder.Entity<ApplicationUser>()
            .HasOne(au => au.EndUserProfile)
            .WithOne(eup => eup.User)
            .HasForeignKey<EndUserProfile>(eup => eup.UserID);

        // One-to-Many: ApplicationUser -> UserAuthenticationHistory
        builder.Entity<UserAuthenticationHistory>()
            .HasOne(h => h.User)
            .WithMany() // No inverse navigation property on ApplicationUser
            .HasForeignKey(h => h.UserID);


        // --- Company & Membership (Many-to-Many Join) ---

        // Define primary key for the join table
        builder.Entity<CompanyMembership>()
            .HasKey(cm => cm.ID);

        // Many-to-One: CompanyMembership -> ApplicationUser
        builder.Entity<CompanyMembership>()
            .HasOne(cm => cm.ApplicationUser)
            .WithMany(au => au.CompanyMemberships)
            .HasForeignKey(cm => cm.ApplicationUserId);

        // Many-to-One: CompanyMembership -> ServiceProvider (Company)
        builder.Entity<CompanyMembership>()
            .HasOne(cm => cm.ServiceProvider)
            .WithMany(sp => sp.Members)
            .HasForeignKey(cm => cm.ServiceProviderID);


        // --- Service Provider & Service Area (Many-to-Many Join) ---

        // Define primary key for the join table
        builder.Entity<ProviderServiceArea>()
            .HasKey(psa => psa.ID);

        // Many-to-One: ProviderServiceArea -> ServiceProvider
        // Note: Your ProviderServiceArea model currently doesn't link back to ServiceProvider.
        // If you add 'public Guid ServiceProviderId { get; set; }' and a navigation property,
        // you would configure that relationship here. For now, we only configure the existing part.

        // Many-to-One: ProviderServiceArea -> ServiceCategory
        builder.Entity<ProviderServiceArea>()
            .HasOne(psa => psa.ServiceCategory)
            .WithMany() // ServiceCategory doesn't have a collection of ProviderServiceArea
            .HasForeignKey(psa => psa.ServiceCategoryID);


        // --- Location & Address Configurations (Table-per-Hierarchy Inheritance) ---

        // Define the base table for the inheritance hierarchy
        builder.Entity<Location>()
            .ToTable("Locations");

        // Define the derived type
        builder.Entity<Address>()
            .HasBaseType<Location>();

        // --- Address Ownership Configurations ---

        // One-to-Many: EndUserProfile -> Address
        builder.Entity<EndUserProfile>()
            .HasMany(eup => eup.Address)
            .WithOne() // Address has no navigation property back to EndUserProfile
            .HasForeignKey(a => a.EndUserProfileID)
            .OnDelete(DeleteBehavior.Cascade); // If a user profile is deleted, their addresses are deleted too.

        // One-to-Many: ServiceProvider -> Address
        builder.Entity<ServiceProvider>()
            .HasMany(sp => sp.Addresses)
            .WithOne() // Address has no navigation property back to ServiceProvider
            .HasForeignKey(a => a.ServiceProviderID) // Using the FK you defined
            .OnDelete(DeleteBehavior.Cascade); // If a service provider is deleted, their addresses are also deleted.


        // --- Lookup Table Configurations ---

        // Many-to-One: StateProvince -> Country
        builder.Entity<StateProvince>()
            .HasOne(sp => sp.Country)
            .WithMany() // Country does not have a navigation property for StateProvinces
            .HasForeignKey(sp => sp.CountryID);

        // Many-to-One: Location -> StateProvince
        builder.Entity<Location>()
            .HasOne(l => l.StateProvince)
            .WithMany() // StateProvince does not have a navigation property for Locations
            .HasForeignKey(l => l.StateProvinceID);
    }
}