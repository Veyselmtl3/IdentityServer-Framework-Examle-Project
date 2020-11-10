using IdentityServerProject.AuthServer.Data;
using IdentityServerProject.AuthServer.Repository;
using IdentityServerProject.AuthServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace IdentityServerProject.AuthServer
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
            services.AddLocalApiAuthentication(); //Art�k AuthServer projem bir api gibi �al��acak d��ar�ya endpointler a�abilecek

            services.AddDbContext<CustomDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DbContextExample"));
            });
            services.AddScoped<ICustomUserRepository, CustomUserService>();

            var assemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name; //contextlerim proje i�inde de�il �dentityserver4 paketi i�inde oldugu i�in 

            services
                .AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = c => c.UseSqlServer(Configuration.GetConnectionString("DbContextExample"), sqloptions => sqloptions.MigrationsAssembly(assemblyName));
                }) //ConfiguRAT�ONDb Context'e denk gelir. client, resource ve scope'lar� veri taban�na kaydedecigimiz ayar metodum
                .AddOperationalStore(c =>
                {
                    c.ConfigureDbContext = c => c.UseSqlServer(Configuration.GetConnectionString("DbContextExample"), sqloptions => sqloptions.MigrationsAssembly(assemblyName));
                }) 
                //artik verilerimi database attigima gore memory kullanmaya gerek yok yoruma aliyorum

                //.AddInMemoryApiResources(Config.GetApiResources())
                //.AddInMemoryApiScopes(Config.GetApiScopes())
                //.AddInMemoryClients(Config.GetClients())
                //.AddInMemoryIdentityResources(Config.GetIdentityResources()) //olu�turdugum �yeler i�in clientler hangi bilgileri tutacak onlar� veriyorum
                                                                             //.AddTestUsers(Config.GetTestUsers().ToList()) //olu�turdugum fake user'lar� verdim
                .AddDeveloperSigningCredential()// development ortam�nda public ve private key haz�rlayacak. json web tokenleri imzalamak i�in uygulamaya girilen public keyin kar��l��� olan private key ile kar��la�t�r�r.
                .AddProfileService<CustomProfileService>() //claimlerin nerden eklenecegi bilmesi i�in eklendi.
                .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();


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
            app.UseIdentityServer(); //middleware eklendi.
            app.UseStaticFiles();

            app.UseRouting();

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
