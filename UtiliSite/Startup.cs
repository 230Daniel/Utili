using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace UtiliSite
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Main.Initialise();

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute("/dashboard", "dashboard/{guild?}");
                options.Conventions.AddPageRoute("/dashboard/core", "dashboard/{guild?}/core");
                options.Conventions.AddPageRoute("/dashboard/autopurge", "dashboard/{guild?}/autopurge");
                options.Conventions.AddPageRoute("/dashboard/voicelink", "dashboard/{guild?}/voicelink");
                options.Conventions.AddPageRoute("/dashboard/voiceroles", "dashboard/{guild?}/voiceroles");
                options.Conventions.AddPageRoute("/dashboard/messagefilter", "dashboard/{guild?}/messagefilter");
                options.Conventions.AddPageRoute("/dashboard/roles", "dashboard/{guild?}/roles");
                options.Conventions.AddPageRoute("/dashboard/channelmirroring", "dashboard/{guild?}/channelmirroring");
            });

            services.AddAuthentication().AddCookie();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddDiscord(options =>
                {
                    options.ClientId = Main._config.DiscordClientId;
                    options.ClientSecret = Main._config.DiscordClientSecret;
                    options.AccessDeniedPath = "/";
                    options.Scope.Add("email");
                    options.Scope.Add("guilds");
                    options.SaveTokens = true;
                    options.ClaimActions.MapAll();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            app.UseExceptionHandler(builder => builder.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>().Error;
                Console.WriteLine("error");
            }));

            // Initialise the database without using cache.
            Database.Database.Initialise(false);
        }
    }
}
