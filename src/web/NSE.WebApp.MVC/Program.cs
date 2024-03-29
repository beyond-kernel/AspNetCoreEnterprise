using Microsoft.Extensions.DependencyInjection;
using NSE.WebApp.MVC.Configuration;
using NSE.WebApp.MVC.Extensions;
using NSE.WebApp.MVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddIdentityConfiguration();
builder.Services.AddControllersWithViews();
builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.RegisterServices(); 
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IComprasBffService, ComprasBffService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//app.UseExceptionHandler("/erro/500");
//app.UseStatusCodePagesWithRedirects("erro/{0}");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
//}

app.UseIdentityConfiguration();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Catalogo}/{action=Index}/{id?}");

app.Run();
