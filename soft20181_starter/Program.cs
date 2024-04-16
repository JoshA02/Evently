using System.Security.Claims;
using System.Security.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using soft20181_starter.Models;
using soft20181_starter.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Default") ?? throw new InvalidOperationException("Connection string 'Default' not found.");
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(configuration);
builder.Services.AddDbContext<EventAppDbContext>(options =>
{
    options.UseSqlite(connectionString);
    options.EnableSensitiveDataLogging(true);
});

// builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<EventAppDbContext>();

builder.Services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
     .AddDefaultTokenProviders()
     .AddDefaultUI() // TODO: Remove this once I've added my own pages
     .AddEntityFrameworkStores<EventAppDbContext>();
// builder.Services.AddAuthentication().AddCookie(options => options.LoginPath = "/Account/Login");
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditEvent", policy =>
    {
        // Require that the logged in user matches the id in the route (bla/blabla/{id}):
        policy.RequireAssertion(CanEditEvent);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();



Boolean CanEditEvent(AuthorizationHandlerContext context)
{
    if (context.User.IsInRole("Admin")) return true; // Admins can do anything; they're the best
    if (!context.User.Identity?.IsAuthenticated ?? false) return false; // If the user isn't logged in, they can't do anything. This shouldn't be possible, but just in case
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    // Get http context:
    if (context.Resource is not DefaultHttpContext httpContext) return false;

    // Get route data:
    var routeData = httpContext.Request.RouteValues;
    foreach (var key in routeData.Keys)
    {
        Console.Out.WriteLine(key + ": " + routeData[key]);
    }

    if (!routeData.ContainsKey("id")) return false;
    var routeId = routeData["id"]?.ToString();
            
    // Get the db context:
    var db = httpContext.RequestServices.GetService(typeof(EventAppDbContext)) as EventAppDbContext;
    var temp = db?.Events.FirstOrDefault(e => e.HostId == userId && e.Id.ToString() == routeId);

    return temp != null;
}