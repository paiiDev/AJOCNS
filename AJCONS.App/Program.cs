using ajocns.database.interfaces;
using AJOCNS.Database.Context;
using AJOCNS.Database.Interfaces;
using AJOCNS.Database.Repositories;
using AJOCNS.Domain.BackgroundJobs;
using AJOCNS.Domain.Interfaces;
using AJOCNS.Domain.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 25 * 1024 * 1024;
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IGraduationRecordRepository, GraduationRecordRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IMentorRepository, MentorRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();
builder.Services.AddScoped<IGraduationRecordService, GraduationRecordService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IMentorService, MentorService>();

builder.Services.AddHostedService<EventStatusUpdateService>();
builder.Services.AddHostedService<JobStatusUpdateService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";

     
    });


var app = builder.Build();

// Convert oversized-request-body errors into a friendly response instead of a raw HTTP 400.
var bodyTooLargeHtml =
    "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>File too large</title>" +
    "<style>body{font-family:Segoe UI,Arial,sans-serif;display:flex;min-height:100vh;margin:0;align-items:center;justify-content:center;background:#f5f7fb;color:#142033}.box{background:#fff;border-radius:16px;box-shadow:0 10px 30px rgba(16,35,63,.08);padding:40px;max-width:420px;text-align:center}.icon{font-size:56px;margin-bottom:12px}h1{font-size:22px;margin:0 0 8px}p{color:#4b5563;margin:0 0 20px}.btn{display:inline-block;text-decoration:none;background:#2563eb;color:#fff;font-weight:700;padding:10px 22px;border-radius:10px}</style></head>" +
    "<body><div class=\"box\"><div class=\"icon\">&#128196;</div>" +
    "<h1>File too large</h1>" +
    "<p>Your file exceeds the 10 MB limit. Please choose a smaller PDF and try again.</p>" +
    "<a class=\"btn\" href=\"javascript:history.back()\">Go back</a></div></body></html>";
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex) when (ex is BadHttpRequestException or InvalidDataException && context.Request.HasFormContentType)
    {
        try
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(bodyTooLargeHtml);
        }
        catch
        {
            // response may already be aborted by the server
        }
    }
});

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
