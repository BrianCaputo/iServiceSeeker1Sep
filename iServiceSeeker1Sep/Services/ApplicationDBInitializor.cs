using ServiceSeeker.Data;
using ServiceSeeker.Services;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ServiceSeeker.Data
{
    /// <summary>
    /// Service responsible for seeding the database with essential data at application startup.
    /// </summary>
    public class ApplicationDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationDbInitializer> _logger;

        public ApplicationDbInitializer(ApplicationDbContext context, ILogger<ApplicationDbInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            // Use a transaction to ensure the entire seeding process succeeds or fails as a single unit.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if any countries already exist. If they do, we assume the DB is seeded.
                if (await _context.Countries.AnyAsync())
                {
                    _logger.LogInformation("Database already seeded with location data. Skipping.");
                    await transaction.CommitAsync(); // Commit the transaction even if we do nothing.
                    return;
                }

                _logger.LogInformation("Database is empty. Seeding location data...");

                // Get the data from our static seeder class
                var countries = LocationDataSeeder.GetCountries();
                var stateProvinces = LocationDataSeeder.GetStateProvinces();

                // Temporarily enable identity insert for Countries
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Countries ON");
                await _context.Countries.AddRangeAsync(countries);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Countries OFF");
                _logger.LogInformation("Seeded Countries successfully.");


                // Temporarily enable identity insert for StateProvinces
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.StateProvinces ON");
                await _context.StateProvinces.AddRangeAsync(stateProvinces);
                await _context.SaveChangesAsync();
                await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.StateProvinces OFF");
                _logger.LogInformation("Seeded StateProvinces successfully.");

                // If all operations were successful, commit the transaction
                await transaction.CommitAsync();

                _logger.LogInformation("Finished seeding the database successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                // If any error occurred, roll back the entire transaction.
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}