using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ChatGPT.Models;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();
builder.Services.AddControllersWithViews();


var connection = builder.Configuration.GetConnectionString("Data Source=(localdb)\\\\MSSQLLocalDB;Initial Catalog=ChatApp;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

builder.Services.AddDbContext<ChatAppContext>(options => options.UseSqlServer(connection));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
              .AddCookie(options =>
              {
                  options.LoginPath = "/Authorization/Login";
                  options.LogoutPath = "/Authorization/Login";
              });
builder.Services.AddAuthorization();


var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();




app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

static void Main(string[] args)
{
    CreateHostBuilder(args).Build().Run();
}

app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;
    var path = statusCodeContext.HttpContext.Request.Path;

    response.ContentType = "text/plain; charset=UTF-8";
    if (response.StatusCode == 403)
    {
        await response.WriteAsync($"Path: {path}. Access Denied ");
    }
    else if (response.StatusCode == 404)
    {
        await response.WriteAsync($"Resource {path} Not Found");
    }
}); 

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}");


app.Run();
