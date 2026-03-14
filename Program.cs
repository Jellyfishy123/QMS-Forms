using Microsoft.EntityFrameworkCore;
using QMSForms.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// Add services to the container
// ==========================
builder.Services.AddControllersWithViews();

// Configure PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ Add session services
builder.Services.AddDistributedMemoryCache(); // in-memory cache for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1); // session timeout
    options.Cookie.HttpOnly = true;              // prevent JS access
    options.Cookie.IsEssential = true;           // required for GDPR
});

builder.Services.AddTransient<EmailService>();

var app = builder.Build();

// ==========================
// Configure the HTTP request pipeline
// ==========================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // show detailed errors in dev
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Enable session middleware BEFORE Authorization
app.UseSession();

app.UseAuthorization();

// Default route: login page first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
