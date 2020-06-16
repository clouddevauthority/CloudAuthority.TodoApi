using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Softveda.Todo.Api.Table
{
	public class Startup
	{
    private static readonly string siteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");

    public Startup(IConfiguration configuration, IHostEnvironment env)
		{
			Configuration = configuration;
      _env = env;
		}

		public IConfiguration Configuration { get; }
    public IHostEnvironment _env { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1",
            new OpenApiInfo
            {
              Version = "v1",
              Title = "Azure Table Todo API",
              Description = $"An Azure Table Storage backed Todo API on {siteName}",
              TermsOfService = new Uri("https://opensource.org/licenses/MIT"),
              Contact = new OpenApiContact
              {
                Name = "Pratik Khasnabis",
                Email = "p**k@outlook.com",
                Url = new Uri("https://github.com/softveda")
              },
              License = new OpenApiLicense
              {
                Name = "The MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
              }

            });
        c.IgnoreObsoleteActions();
        c.IgnoreObsoleteProperties();

        c.AddServer(new OpenApiServer
        {
          Url = "https://localhost:5001",
          Description = $"Environment: {_env.EnvironmentName}"
        });

        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
      });
    }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

      // Inject Azure App Service name in a custom response header
      // to know from which App Service the response is returned
      app.Use(async (context, next) =>
      {
        context.Response.Headers.Add("APP-SERVICE-NAME", siteName);
        await next.Invoke();
      });

      app.UseSwagger();
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
        c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root 
      });

      app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
