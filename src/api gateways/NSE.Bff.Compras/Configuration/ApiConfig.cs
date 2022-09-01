using NSE.WebAPI.Core.Identidade;

namespace NSE.Bff.Compras.Configuration
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
        {
            services.AddControllers();

            return services;
        }


        public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.UseHttpsRedirection();
             
            app.UseAuthConfiguration();

            app.UseAuthentication();

            app.UseCors("Total");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            return app;
        }
    }
}
