using System;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Database;
using Disqord;
using Disqord.Http.Default;
using Disqord.OAuth2;
using Disqord.Rest;
using Stripe;
using UtiliBackend.Authorisation;
using UtiliBackend.Extensions;
using UtiliBackend.Middleware;
using UtiliBackend.Services;

namespace UtiliBackend
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .WithOrigins(_configuration["Frontend:Origin"])
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()));

            services.AddSingleton<IAuthorizationPolicyProvider, PolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, DiscordAuthorisationHandler>();
            services.AddSingleton<IAuthorizationHandler, DiscordGuildAuthorisationHandler>();
            services.AddSingleton<IAuthorizationMiddlewareResultHandler, ResultHandler>();
            services.AddRestClient();
            services.AddToken(Disqord.Token.Bot(_configuration["Discord:Token"]));

            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(30);
                options.IncludeSubDomains = true;
            });

            services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            services.AddControllers();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddHttpContextAccessor();
            services.AddOptions();
            services.AddMemoryCache();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                // Same-site none is safe because the cookie is http-only
                options.Cookie.SameSite = SameSiteMode.None;
            })
            .AddDiscord(options =>
            {
                options.ClientId = _configuration["Discord:ClientId"];
                options.ClientSecret = _configuration["Discord:ClientSecret"];
                options.AccessDeniedPath = "/";
                options.Scope.Add("email");
                options.Scope.Add("guilds");
                options.SaveTokens = true;
                options.ClaimActions.MapAll();
            });

            services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-XSRF-TOKEN";
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddBearerClientFactory();
            services.AddSingleton<DiscordClientService>();
            services.AddSingleton<DiscordUserGuildsService>();
            services.AddSingleton<DiscordRestService>();
            services.AddHostedService<SlotDeletionService>();

            services.AddDbContext<DatabaseContext>();

            services.AddScoped<Services.CustomerService>();
            services.AddSingleton(new StripeClient(_configuration["Stripe:SecretKey"]));

            // Set a 10 second timeout on connections to hopefully avoid deadlock of DiscordUserGuildsService and other similar services
            // All other timeouts in the client have reasonable default values
            services.Configure<DefaultHttpClientFactoryConfiguration>(options =>
                options.ClientConfiguration = handler =>
                    handler.ConnectTimeout = TimeSpan.FromSeconds(10));

            services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(_configuration.GetSection("IpRateLimitPolicies"));

            DatabaseContextExtensions.DefaultPrefix = _configuration["Other:DefaultPrefix"];
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseIpRateLimiting();

            if (!env.IsDevelopment())
            {
                app.UseMiddleware<AlwaysHttpsMiddleware>();
                app.UseHsts();
            }

            app.UseMiddleware<NoCacheMiddleware>();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<UserAccountsMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
