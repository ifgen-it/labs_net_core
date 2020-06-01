using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Practice3.Domain.Abstract;
using Practice3.Domain.Concrete;

namespace Practice3
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
            services.AddControllers()
                      // for noncycle json serializing
                      // Install - Package Microsoft.AspNetCore.Mvc.NewtonsoftJson
                      .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                      );

            services.AddDbContext<MovieDbContext>(contextLifetime: ServiceLifetime.Scoped, ServiceLifetime.Scoped);
            services.AddScoped<IMovieRepository, EFMovieRepository>();

            /*services.AddDbContext<MovieDbContext>(contextLifetime: ServiceLifetime.Singleton, ServiceLifetime.Singleton);
            services.AddSingleton<IMovieRepository, EFMovieRepository>();*/

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
