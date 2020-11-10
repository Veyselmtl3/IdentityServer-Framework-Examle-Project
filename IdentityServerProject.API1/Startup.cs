using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServerProject.API1
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
            //al�nan tokeni jwt.io sitesi ile i�erisinde ta��nan bilgilere bak�labilir.

            //kimlik dogrulamas� yapal�m
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,options =>
            {
                options.Authority = "https://localhost:5001";
                options.Audience = "resource_api1"; //bana bir token geldi�i zaman aut alan�nda bu olmal�. Yani tokeni alan kullan�c� bu alana eri�ebilirmi onu kontrol ediyor
            }); //schema girmek zorunday�m

            //kimli�i dogrulanm�� bir kullan�c�n�n yetkilendirmesi
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ReadProduct", options => // ReadProduct bu yetkiye sahip olan client bu api'de sadece read metodlar�na istek atabilsin.
                {
                     options.RequireClaim("scope", "api1.read");
                 });

                options.AddPolicy("UpdateOrCreate", options =>
                 {
                     options.RequireClaim("scope", new[] { "api1.update", "api1.create" });
                 });
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
