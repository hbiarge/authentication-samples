using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MvcCorporate.Infrastructure;

namespace MvcCorporate
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
            var authority = $"{Configuration["AzureAd:Instance"]}{Configuration["AzureAd:TenantId"]}";
            var clientId = Configuration["AzureAd:ClientId"];
            var clientSecret = Configuration["AzureAd:ClientSecret"];

            services.Configure<AuthOptions>(options =>
            {
                options.Authority = authority;
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.ApiResourceId = Configuration["AzureAd:ApiResourceId"];
            });

            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options =>
                {
                    options.Authority = authority;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.UseTokenLifetime = true;
                    options.CallbackPath = Configuration["AzureAd:CallbackPath"];
                    options.Resource = Configuration["AzureAd:ApiResourceId"];
                    options.SaveTokens = true;
                    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                    options.GetClaimsFromUserInfoEndpoint = false;
                });

            services.AddSession();

            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            })
            .AddRazorPagesOptions(options =>
            {
                options.Conventions.AllowAnonymousToFolder("/Account");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            // Acquire a Token for the API and cache it using ADAL. In the Api page, we'll use the cache to acquire a token to the API
            var userObjectId = context.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            var clientCred = new ClientCredential(context.Options.ClientId, context.Options.ClientSecret);
            var authContext = new AuthenticationContext(
                authority: context.Options.Authority,
                tokenCache: new NaiveSessionCache(userObjectId, context.HttpContext.Session));
            var authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
                authorizationCode: context.ProtocolMessage.Code,
                redirectUri: new Uri(context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]),
                clientCredential: clientCred,
                resource: Configuration["AzureAd:ApiResourceId"]);

            // Notify the OIDC middleware that we already took care of code redemption.
            context.HandleCodeRedemption();
        }
    }
}
