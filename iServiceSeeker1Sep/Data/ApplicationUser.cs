using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ServiceSeeker.Data
{
    #region User Related Models
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
        public int ID { get; set; }
        public string UserID { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public AuthenticationMethod Method { get; set; }
        public string? ExternalProvider { get; set; } // "Google", "LinkedIn"
        public DateTime AddedAt { get; set; }
        public DateTime? RemovedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
    public class EndUserProfile
    {
        public Guid ID { get; set; }
        public string UserID { get; set; } = string.Empty; // Foreign key to ApplicationUser
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

        [Required]
        [StringLength(3)]
        public string Iso3Code { get; set; } = string.Empty; // e.g., "USA", "CAN"
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
    }
    public class Address : Location
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Main Office", "Home"

        [StringLength(100)]
        public string? StreetLine2 { get; set; } // For suite, apt, etc.

        [StringLength(100)]
        public string? StreetLine3 { get; set; }

        [Required]
        public AddressPurpose Purpose { get; set; } // An enum for context
                                                    // Foreign key for the one-to-many relationship with EndUserProfile
        public Guid? EndUserProfileID { get; set; }

        // Foreign key for the one-to-one relationship with ServiceProviderProfile
        public Guid? ServiceProviderID { get; set; }
    }
    public enum AddressPurpose
    {
        Mailing,
        Billing,
        Service,
        Residiential,
        Headquarters
    }
    #endregion
    #region Company
    public class CompanyMembership
    {
        public Guid ID { get; set; } // Primary Key

        // Foreign Key to the User
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        // Foreign Key to the Company
        public Guid ServiceProviderID { get; set; }
        public ServiceProvider ServiceProvider { get; set; }

        // Role of the user within THIS specific company
        public UserRole Role { get; set; } // e.g., Owner, Admin, Employee
    }
    public enum UserRole { Owner, Admin, Employee }
    public class ServiceProvider
    {
        public Guid ID { get; set; } // Primary Key
        public string CompanyName { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; }
        [StringLength(9)]
        public string DUNSNumber { get; set; } = string.Empty; // D-U-N-S Number for business identification
        [StringLength(1000)]
        public string? Description { get; set; }
        // --- Relationships ---
        public ICollection<CompanyMembership> Members { get; set; } = new List<CompanyMembership>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<ProviderServiceArea> ProviderServiceArea { get; set; } = new List<ProviderServiceArea>();
    }
    public class ProviderServiceArea
    {
        public int ID { get; set; }
        //public int ServiceProviderID { get; set; }
        //public ServiceProvider ServiceProvider { get; set; } = null!;
        public int ServiceCategoryID { get; set; }
        public ServiceCategory ServiceCategory { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public ServiceAreaType ServiceAreaType { get; set; }
    }
    public class ServiceCategory
    {
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public string? UNSPSCCode { get; set; } // Optional UNSPSC code for standardization
    }
    /// <summary>
    /// Types of service areas
    /// </summary>
    public enum ServiceAreaType
    {
        [Display(Name = "City")]
        City = 1,

        [Display(Name = "County")]
        County = 2,

        [Display(Name = "State")]
        State = 3,

        [Display(Name = "ZIP Code")]
        ZipCode = 4,

        [Display(Name = "Radius (Miles)")]
        Radius = 5,

        [Display(Name = "Metropolitan Area")]
        Metropolitan = 6,

        [Display(Name = "Regional")]
        Regional = 7,

        [Display(Name = "National")]
        National = 8
    }
    #endregion 
}