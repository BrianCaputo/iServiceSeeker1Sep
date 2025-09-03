using iServiceSeeker1Sep.Components;
using iServiceSeeker1Sep.Components.Account;
using iServiceSeeker1Sep.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using iServiceSeeker1Sep.Services;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OAuth.LinkedIn;

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
}).AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

//builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailSender<ApplicationUser>>(provider =>
    new EmailSenderAdapter<ApplicationUser>(provider.GetRequiredService<IEmailSender>()));

builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated(); // This creates the database and tables
}

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

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
