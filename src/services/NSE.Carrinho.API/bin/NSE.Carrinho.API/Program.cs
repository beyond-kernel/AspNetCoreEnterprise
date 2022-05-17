using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NSE.Carrinho.API.Configuration;
using NSE.Carrinho.API.Data;
using NSE.Carrinho.API.Services;
using NSE.MessageBus;
using NSE.WebAPI.Core.Identidade;
using NSE.WebAPI.Core.Usuario;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddDbContext<CarrinhoContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Total",
        builder =>
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
});

// Add services to the container.

// API
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAspNetUser, AspNetUser>();

builder.Services.AddScoped<CarrinhoContext>();

builder.Services.AddMessageBus("host=localhost:5672;publisherConfirms=true;timeout=10").AddHostedService<CarrinhoIntegrationHandler>();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "NerdStore Enterprise Carrinho API",
        Description = "Esta API possui os produtos do carrinho da aplicacao.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact() { Name = "Rafael", Url = new Uri("https://opensource.org/license/MIT") },
        License = new Microsoft.OpenApi.Models.OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/license/MIT") }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT desta maneira: Bearer {seu token}",
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"); });
//}

//app.UseApiConfiguration(app.Environment);

//app.UseAuthConfiguration();

app.MapControllers();

app.Run();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Total");

app.UseAuthConfiguration();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});