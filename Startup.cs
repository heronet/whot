using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using Utils.SignalR;

namespace whot
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors();
            services.AddSignalR();
            services.AddScoped<TokenService>();
            services.AddSingleton<ActiveTracker>();
            services.AddWhotServices(_configuration); // Most of the configurations are in this Extension
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors(configurePolicy =>
            {
                configurePolicy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:4200");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("hubs/chat");
                endpoints.MapHub<ActiveHub>("hubs/active");
            });
        }
    }
}
