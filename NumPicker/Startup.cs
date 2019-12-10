
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RWD.Toolboox.Ui.Middleware.SecurityHeaders;
using RWD.Toolbox.Ui.Middleware.CspHeader;

namespace NumPicker
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
            // for use within ConfigureServices
            var appSettings = new AppSettings();
            Configuration.GetSection("ApplicationSettings").Bind(appSettings);

            // app config settings
            services.Configure<AppSettings>(Configuration.GetSection("ApplicationSettings"));

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //    options.CheckConsentNeeded = context => true;
            //    options.MinimumSameSitePolicy = SameSiteMode.None;
            //});

            services.AddControllersWithViews();
            services.AddRazorPages();
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
                app.UseExceptionHandler("/Home/Error");
            }

            // security headers            
            app.UseSecurityHeadersMiddleware(new SecurityHeadersBuilder()
                           .AddDefaultSecurePolicy()
                           .AddStrictTransportSecurity()
                           );

            // CSP header
            app.UseCspMiddleware(builder =>
            {
                builder.Default_Src
                       .AllowSelf();

                builder.Scripts_Src
                       .AllowSelf()
                       .Allow("https://cdnjs.cloudflare.com");

                builder.Styles_Src
                       .AllowSelf()
                       .Allow("https://maxcdn.bootstrapcdn.com")
                       .Allow("https://cdnjs.cloudflare.com");

                builder.Fonts_Src
                       .AllowSelf()
                       .Allow("https://maxcdn.bootstrapcdn.com");

                builder.Imgs_Src
                       .AllowSelf()
                       .AllowData()
                       .Allow("https://realworlddevelopers.com");

                builder.Connect_Src
                    .AllowSelf()
                    .Allow("https://cdnjs.cloudflare.com")
                    .Allow("https://maxcdn.bootstrapcdn.com")
                    .Allow("https://realworlddevelopers.com");

                builder.Object_Src
                    .AllowSelf();

                builder.Frame_Ancestors
                    .AllowNone();

                // builder.ReportUri = "api/CspReport/report";
            });


            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseCookiePolicy();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });

        }
    }
}
