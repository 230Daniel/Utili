using System;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(30);
                options.IncludeSubDomains = true;
            });

            // https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/IpRateLimitMiddleware#setup

            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddMvc();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddCors(options =>
            {
                options.AddPolicy("frontend",
                    builder =>
                    {
                        builder.WithOrigins(Main.Config.Frontend)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
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
            //app.UseIpRateLimiting(); // <-- Keep this at the top

            if (env.IsDevelopment()) 
                app.UseDeveloperExceptionPage();
            else
            {
                Console.WriteLine("Development");
                app.Use((ctx, next) =>
                {
                    ctx.Request.Scheme = "https";
                    return next();
                });
                app.UseHsts();
            }
                

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("frontend");
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
