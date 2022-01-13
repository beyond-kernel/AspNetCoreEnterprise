using Microsoft.AspNetCore.Authentication.Cookies;

namespace NSE.WebApp.MVC.Configuration
{
    public static class IdentityConfig
    {
        public static void AddIdentityConfiguration(this IServiceCollection services) 
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opt => 
            {
                opt.LoginPath = "/login";
                opt.AccessDeniedPath = "/acesso-negado";
            });
        }   
        
        public static void UseIdentityConfiguration(this IApplicationBuilder app) 
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
