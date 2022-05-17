using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NSE.MessageBus;
using NSE.Pagamentos.API.Data;
using NSE.Pagamentos.API.Data.Repository;
using NSE.Pagamentos.API.Models;
using NSE.Pagamentos.API.Services;
using NSE.Pagamentos.CardAntiCorruption;
using NSE.Pagamentos.Facade;
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

builder.Services.AddDbContext<PagamentosContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<IPagamentoFacade, PagamentoCartaoCreditoFacade>();

builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
builder.Services.AddScoped<PagamentosContext>();

builder.Services.AddMessageBus("host=localhost:5672;publisherConfirms=true;timeout=10")
               .AddHostedService<PagamentoIntegrationHandler>();

builder.Services.AddMessageBus("host=localhost:5672;publisherConfirms=true;timeout=10");
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "NerdStore Enterprise Pagamentos API",
        Description = "Esta API faz parte do curso ASP.NET Core Enterprise Applications.",
        Contact = new OpenApiContact() { Name = "Eduardo Pires", Email = "contato@desenvolvedor.io" },
        License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"); });
}

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