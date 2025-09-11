using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace iServiceSeeker1Sep.Data
{
    // Custom enum to define the user's primary role in the application
    public enum UserType
    {
        NotSet,
        EndUser,
        ServiceProvider
    }
    public enum AuthenticationMethod
    {
        Local,
        Google,
        LinkedIn,
        Microsoft
    }
    public class ApplicationUser : IdentityUser
    {
        // --- Universal Profile Data ---
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        // --- Application State ---
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginDate { get; set; }

        // --- ADDED: Properties for Profile Completion ---
        public bool IsProfileComplete { get; set; } = false;
        public UserType UserType { get; set; } = UserType.NotSet;

        // --- Authentication Tracking ---
        public AuthenticationMethod PrimaryAuthMethod { get; set; } = AuthenticationMethod.Local;
        public DateTime PrimaryAuthSetAt { get; set; } = DateTime.UtcNow;

        // --- Password Management ---
        public bool HasLocalPassword { get; set; } = false;
        public DateTime? LocalPasswordAddedAt { get; set; }

        // --- Email Confirmation Logic ---
        public bool InitialEmailConfirmed { get; set; } = false; // Tracks if INITIAL registration was confirmed

        public string FullName => $"{FirstName} {LastName}";
        // Helper methods
        public bool RequiresEmailConfirmation => PrimaryAuthMethod == AuthenticationMethod.Local && !InitialEmailConfirmed;
        public bool IsExternalPrimary => PrimaryAuthMethod != AuthenticationMethod.Local;
        public bool HasMultipleAuthMethods => HasLocalPassword && UserLogins.Any();

        // Navigation properties
        public ICollection<CompanyMembership> CompanyMemberships { get; set; } = new List<CompanyMembership>();
        public EndUserProfile? EndUserProfile { get; set; }

        // This tracks external logins - built into Identity
        public virtual ICollection<IdentityUserLogin<string>> UserLogins { get; set; } = new List<IdentityUserLogin<string>>();
    }

    public class UserAuthenticationHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public AuthenticationMethod Method { get; set; }
        public string? ExternalProvider { get; set; } // "Google", "LinkedIn"
        public DateTime AddedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class EndUserProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } = null!; // Navigation property
        public ICollection<Address> Address { get; set; } = new List<Address>(); // Allows multiple addresses
    }

    public class Country
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        public string Iso2Code { get; set; } = string.Empty; // e.g., "US", "CA"
    }
    public class StateProvince
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Abbreviation { get; set; } = string.Empty; // e.g., "PA", "ON"

        // --- Relationship to Country ---
        [Required]
        public int CountryID { get; set; }
        [ForeignKey("CountryID")]
        public Country Country { get; set; }
    }
    public class Location
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        [StringLength(200)]
        public string StreetLine1 { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [DataType(DataType.PostalCode)]
        public string PostalCode { get; set; } = string.Empty;

        // --- Geographic Coordinates ---
        [Column(TypeName = "decimal(9, 6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9, 6)")]
        public decimal Longitude { get; set; }

        // --- Relationships to Lookup Tables ---
        [Required]
        public int StateProvinceID { get; set; }
        [ForeignKey("StateProvinceID")]
        public StateProvince StateProvince { get; set; }

        //[Required]
        //public int CountryID { get; set; }
        //[ForeignKey("CountryID")]
        //public Country Country { get; set; }
    }
    public class Address : Location
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Main Office", "Home"

        [StringLength(100)]
        public string? StreetLine2 { get; set; } // For suite, apt, etc.

        [StringLength(100)]
        public string? StreetLine3 { get; set; }

        [Required]
        public AddressPurpose Purpose { get; set; } // An enum for context
    }
    public enum AddressPurpose
    {
        Mailing,
        Billing,
        Service,
        Residiential,
        Headquarters
    }

    public class CompanyMembership
    {
        public Guid Id { get; set; } // Primary Key

        // Foreign Key to the User
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        // Foreign Key to the Company
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        // Role of the user within THIS specific company
        public CompanyRole Role { get; set; } // e.g., Owner, Admin, Employee
    }
    public enum CompanyRole { Owner, Admin, Employee }

    public class Company
    {
        public Guid Id { get; set; } // Primary Key
        public string CompanyName { get; set; } = string.Empty;
        public string? LicenseNumber { get; set; }
        public string? InsuranceProvider { get; set; }
        public bool IsVerified { get; set; }

        // Address Information
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        // --- Relationships ---
        public ICollection<CompanyMembership> Members { get; set; } = new List<CompanyMembership>();
        public ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
        [StringLength(8)]
        public string? InviteCode { get; set; }

    }
    public class ServiceProviderProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;
        [StringLength(50)]
        public string? LicenseNumber { get; set; }
        public Address? Address { get; set; }

        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
        [Precision(8,2)]
        public decimal ServiceRadius { get; set; } = 50; // miles

        [StringLength(1000)]
        public string? Description { get; set; }
        public string? Website { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public List<ServiceProviderServiceArea> ServiceAreas { get; set; } = new();
    }
    public class ServiceProviderServiceArea
    {
        public int Id { get; set; }
        public int ServiceProviderProfileId { get; set; }
        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;
        public int ServiceCategoryId { get; set; }
        public ServiceCategory ServiceCategory { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
    public class ServiceCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;

        public List<ServiceProviderServiceArea> ServiceProviderServiceAreas { get; set; } = new();
    }
}
