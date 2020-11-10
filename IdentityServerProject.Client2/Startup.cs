using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServerProject.Client2
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
            //authorization server la haberle�memizi yazal�m.
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies"; //olu�acak olan cookie ad�. Client1 ile giri� yap�ld�g�nda f12 tu�una bakarak cookie olu�up olu�mad�g�na bakabilirsiniz
                options.DefaultChallengeScheme = "oidc"; //open id connect
            })
                .AddCookie("Cookies", options =>
                {
                    options.AccessDeniedPath = "/Home/AccessDenied"; //customer rolune sahip ki�i adminin g�rebilecegi metoda eri�meye �al���rsa otomatik accesdenied sayfas�na y�nlendiriyo ben burada istedigi sayfaya gitmesin benim istedigim sayfaya gitsin istiyorum
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    options.Authority = "https://localhost:5001"; //tokeni dag�tan yetkili merkez neresi buraya onu yazal�m. Yani AuthServer projemin ayaga kalkacag� port
                    options.ClientId = "Client2-MVC";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token"; //code dedi�im authorization token id_token ise tokeni dogrulamak i�in
                    options.GetClaimsFromUserInfoEndpoint = true; //arka planda userinfo endpointine istek at�p cookide yer alan kullan�c� bilgilerini ad� rol�n� vs getirecek kaynak = "https://identityserver4.readthedocs.io/en/latest/endpoints/userinfo.html",
                    options.SaveTokens = true;
                    options.Scope.Add("api1.read"); //verilen token bilgisi i�erisinde bu scopeda g�ster diyorum
                    options.Scope.Add("offline_access");
                    options.Scope.Add("CountryAndCity"); //kullan�c�n�n �lke ve �ehir bilgisini �ag�r�yorum burada �ag�rm�� oldugum scope degerleri Config class�nda tan�mlam�� oldugum IdentityResource degerleridir.

                    options.Scope.Add("Roles");
                    options.ClaimActions.MapUniqueJsonKey("country", "country"); //mapleme i�lemi
                    options.ClaimActions.MapUniqueJsonKey("city", "city");
                    options.ClaimActions.MapUniqueJsonKey("role", "role");

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        RoleClaimType = "role"
                    };
                });

            services.AddControllersWithViews();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
