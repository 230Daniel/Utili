using System;
using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace UtiliBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Main.InitialiseAsync().GetAwaiter().GetResult();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: "allowAll",
                    builder =>
                    {
                        builder.SetIsOriginAllowed(x => x == "http://localhost:3000")
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            services.AddAuthentication().AddCookie();

            services.AddAuthentication(options =>
            {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddDiscord(options =>
            {
                options.ClientId = Main.Config.DiscordClientId;
                options.ClientSecret = Main.Config.DiscordClientSecret;
                options.AccessDeniedPath = "/";
                options.Scope.Add("email");
                options.Scope.Add("guilds");
                options.SaveTokens = true;
                options.ClaimActions.MapAll();
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                await next();
            });

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("allowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
