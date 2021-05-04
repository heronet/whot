using System.Text;
using System.Threading.Tasks;
using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Models;

namespace Extensions
{
    public static class WhotServiceExtension
    {
        /// <summary>
        /// Extension Method to Add Configuration Services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <returns>A reference to this object after the operation has completed</returns>
        public static IServiceCollection AddWhotServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(configuration.GetConnectionString("SQLite"));
            });
            services.AddIdentityCore<WhotUser>(setupAction =>
            {
                setupAction.User.RequireUniqueEmail = true;
                setupAction.Password.RequireNonAlphanumeric = false;
                setupAction.Password.RequireDigit = false;
                setupAction.Password.RequiredLength = 4;
                setupAction.Password.RequireLowercase = false;
                setupAction.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<AppDbContext>().AddSignInManager<SignInManager<WhotUser>>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT_SECRET"])),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            return services;
        }
    }
}