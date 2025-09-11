using AspNet.Security.OAuth.LinkedIn;
using iServiceSeeker.Data;
using iServiceSeeker1Sep.Components;
using iServiceSeeker1Sep.Components.Account;
using iServiceSeeker1Sep.Data;
using iServiceSeeker1Sep.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.SaveTokens = true;

    // Important: Set the callback path
    options.CallbackPath = "/signin-google";

    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapJsonKey("picture", "picture");
})
/*.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
    options.SaveTokens = true;

    // Optional: Add additional scopes
    options.Scope.Add("https://graph.microsoft.com/user.read");

    // Optional: Map additional claims
    options.ClaimActions.MapJsonKey("picture", "picture");
    options.ClaimActions.MapJsonKey("locale", "locale");
})*/
.AddLinkedIn(LinkedInAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:LinkedIn:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:LinkedIn:ClientSecret"];
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.SaveTokens = true;

    // Map LinkedIn claims
    options.ClaimActions.MapJsonKey("picture", "profilePicture");
    options.ClaimActions.MapJsonKey("locale", "locale");
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Email confirmation required for local registration only
    options.SignIn.RequireConfirmedAccount = true;

    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User requirements
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register identity linking services
builder.Services.AddScoped<IUserConfirmation<ApplicationUser>, IdentityLinkingUserConfirmation>();
builder.Services.AddScoped<AuthMethodLinkingService>();
builder.Services.AddScoped<IUserTrackingService, UserTrackingService>();
builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailSender<ApplicationUser>>(provider =>
    new EmailSenderAdapter<ApplicationUser>(provider.GetRequiredService<IEmailSender>()));

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);
// **** Register the new database initializer service ****
builder.Services.AddScoped<ApplicationDbInitializer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
//TODO - need to fix the EF issue that allows user POCO to update and migrate properly

// --- START: Database Reset and Seeding Logic (For Development Only) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // WARNING: This is for development only. It deletes the database on every startup.
        //context.Database.EnsureDeleted();
        //context.Database.EnsureCreated();

        // Seed the "Admin" role into the database if it doesn't exist.
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // Create a default admin user if one doesn't exist.
        var adminUser = await userManager.FindByEmailAsync("admin@serviceseeker.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin@serviceseeker.com",
                Email = "admin@serviceseeker.com",
                FirstName = "Admin",
                LastName = "User",
                UserType = UserType.NotSet, // Or another default
                EmailConfirmed = true // Confirm email immediately for the admin
            };
            // IMPORTANT: Use a strong, secure password from your secrets file in a real app!
            await userManager.CreateAsync(adminUser, "AdminPassword1!");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating and seeding the DB.");
    }
}
// --- END: Database Reset and Seeding Logic ---
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();
// **** Run the database seeder ****
await SeedDatabaseAsync(app);

app.Run();


async Task SeedDatabaseAsync(IHost app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var initializer = services.GetRequiredService<ApplicationDbInitializer>();
        await initializer.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}