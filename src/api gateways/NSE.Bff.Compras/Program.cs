using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NSE.Bff.Compras.Configuration;
using NSE.Bff.Compras.Extensions;
using NSE.Bff.Compras.Services;
using NSE.WebAPI.Core.Extensions;
using NSE.WebAPI.Core.Identidade;
using NSE.WebAPI.Core.Usuario;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true, true)
    .AddEnvironmentVariables();


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAspNetUser, AspNetUser>();

builder.Services.AddSwaggerGen();

builder.Services.AddApiConfiguration();

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddCors(opt => { opt.AddPolicy("Total", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()); });

builder.Services.Configure<AppServicesSettings>(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "NerdStore Enterprise BFF API GATEWAY",
        Description = "Esta api � o BFF da aplicacao.",
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



builder.Services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

builder.Services.AddHttpClient<ICatalogoService, CatalogoService>()
    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
    .AddPolicyHandler(PollyExtensions.EsperarTentar())
    .AddTransientHttpErrorPolicy(
        p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.AddHttpClient<ICarrinhoService, CarrinhoService>()
    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
    .AddPolicyHandler(PollyExtensions.EsperarTentar())
    .AddTransientHttpErrorPolicy(
        p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.AddHttpClient<IPedidoService, PedidoService>()
    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
    .AddPolicyHandler(PollyExtensions.EsperarTentar())
    .AddTransientHttpErrorPolicy(
        p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.AddHttpClient<IClienteService, ClienteService>()
    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
    .AddPolicyHandler(PollyExtensions.EsperarTentar())
    .AddTransientHttpErrorPolicy(
        p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseApiConfiguration(app.Environment);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();


app.MapControllers();

app.Run();