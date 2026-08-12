using Microsoft.AspNetCore.Localization; // these were added to support localization of the application
using RegistrationSystem;                // these were added to support localization of the application
using System.Globalization;              // these were added to support localization of the application
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegistrationSystem.Data;
using RegistrationSystem.Services; // this is needed to use the PersonService and BookService class in the Program.cs file
using Microsoft.AspNetCore.Identity.UI.Services; // this is needed to use the EmailSender class in the Program.cs file

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Tells the localization system that translation files
// are stored inside the Resources folder.
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// Adds localization support to Razor views and directs
// every model's validation messages to SharedResource.
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider =
            (type, factory) =>
                factory.Create(typeof(SharedResource)); // normally, ASP.NET Core searches for a different resource file for every model. This tells it to use our shared resource files for all validation messages instead
    });

// The application currently supports English and Arabic.
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // English is used when no language was selected.
    options.DefaultRequestCulture =
        new RequestCulture("en");

    // Controls culture-sensitive formatting,
    // including dates, times, and numbers.
    options.SupportedCultures =
        supportedCultures;

    // Controls which resource file supplies
    // the translated interface text.
    options.SupportedUICultures =
        supportedCultures;
});

builder.Services.AddHttpContextAccessor(); // lets ApplicationDbContext access the currently logged-in user's email

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IdentityUser is Microsoft's built-in class for storing user information such as username, email, and password. It is stored in the AspNetUsers table in the database. AddDefaultIdentity adds the secure account, password, token, and sign-in services used by the custom MVC account pages.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // users can log in immediately after registering; email confirmation is not needed for this school project

    options.Password.RequiredLength = 6; // password must contain at least 6 characters
    options.Password.RequireDigit = false; // a number is not required
    options.Password.RequireLowercase = false; // a lowercase letter is not required
    options.Password.RequireUppercase = false; // an uppercase letter is not required
    options.Password.RequireNonAlphanumeric = false; // a special character such as ! or @ is not required
    options.Password.RequiredUniqueChars = 1; // only one different character is required
})
.AddRoles<IdentityRole>() // adds role support so accounts can belong to roles such as Admin and User
.AddEntityFrameworkStores<ApplicationDbContext>(); // tells Identity to save users and passwords in our ApplicationDbContext database

// Makes the custom MVC Login page the destination
// used when a logged-out visitor requests a protected page.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthorization();

// Loads Gmail SMTP settings from User Secrets.
builder.Services.Configure<GmailOptions>(
    builder.Configuration.GetSection("Gmail"));

// Makes Gmail the email sender used by the custom
// Forgot Password and Reset Password workflow.
builder.Services.AddTransient<
    IEmailSender,
    EmailSender>();

builder.Services.AddScoped<PersonService>(); // AddScoped means create one PersonService per web request, and reuse that same instance through that request
                                             //
                                             //
                                             //
builder.Services.AddScoped<BookService>();   //
                                             //
                                             //
                                             //
builder.Services.AddScoped<BorrowService>(); // these are what let the controllers use the services, and the services use the ApplicationDbContext
                                             //
                                             //
                                             //
builder.Services.AddScoped<AuthorService>(); //
                                             //
                                             //
                                             //
builder.Services.AddScoped<CategoryService>(); // basically one instance of PersonService and ApplicationDbContext are created per web request
                                             //   
                                             //   
                                             //   
builder.Services.AddScoped<UserService>();   // lets UsersController use the user-management service
builder.Services.AddScoped<DashboardService>(); // lets DashboardController use the dashboard reporting service
var app = builder.Build();

// Determines the culture at the beginning of each request
// before pages and views retrieve translated text.
app.UseRequestLocalization();

// creates the Admin and User roles and assigns the configured existing account to Admin.
// this runs safely on every startup because IdentitySeedData checks what already exists.
await IdentitySeedData.InitializeAsync(app.Services, app.Configuration);

// configure the HTTP request pipeline (the code below configures how incoming requests are handled
if (!app.Environment.IsDevelopment()) // checks if the application is running outside Development mode - normally Production
{
    app.UseExceptionHandler("/Home/Error"); // redirects to the Error page if an unhandled exception occurs
    // HSTS tells the browser : "for the next 30 days , only use HTTPS for this website
    app.UseHsts(); // useful in production, but now needed while developing locally
}

app.UseHttpsRedirection(); // if someone tries to access the site using HTTP, redirect them to HTTPS
app.UseRouting(); // reads the url and works out which controller and action to call, and any parameters to pass to that action

app.UseAuthentication(); // checks whether the current visitor is logged in and reads their login cookie

app.UseAuthorization(); // checks whether the logged-in user is allowed to access the requested page

app.MapStaticAssets(); // makes static files such as css, js, and images available to the browser

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Book}/{action=Index}/{id?}") // the main page is now View Books instead of Person Registration
    .WithStaticAssets();

app.Run();
