using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RegistrationSystem.Data;
using RegistrationSystem.Services; // this is needed to use the PersonService and BookService class in the Program.cs file
using Microsoft.AspNetCore.Identity.UI.Services; // this is needed to use the EmailSender class in the Program.cs file

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages(options =>
{
    // Only administrators can open the Change Password page
    // inside the Identity area.    
    options.Conventions.AuthorizeAreaPage(
        "Identity",
        "/Account/Manage/ChangePassword",
        "AdminOnly");
}); // Identity Login and Register pages are Razor Pages, so this allows the application to use them

builder.Services.AddHttpContextAccessor(); // lets ApplicationDbContext access the currently logged-in user's email

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// IdentityUser is Microsoft's built-in class for storing user information such as username, email, and password. It is stored in the AspNetUsers table in the database. The AddDefaultIdentity method adds all the services needed for Identity to work, including the login and register pages
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
// Defines the authorization rule used by the built-in Change Password page.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireRole("Admin");
    });
}); // this protects the actual page even if a regualr User manually types the URL

// Loads Gmail SMTP settings from User Secrets.
builder.Services.Configure<GmailOptions>(
    builder.Configuration.GetSection("Gmail"));

// Makes Gmail the email sender used by Identity's
// Register, Forgot Password, and Reset Password pages.
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

app.MapRazorPages(); // makes all razor pages in the project accessible through web URLs

app.Run();
