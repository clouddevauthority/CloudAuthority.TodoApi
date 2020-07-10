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
using Microsoft.AspNetCore.Authentication.Certificate;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Softveda.Todo.Shared.Middleware;

namespace Softveda.Todo.Api.InMemory
{
	public class Startup
	{
		private static readonly string siteName =
			Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? "localhost";

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
			services.Configure<CertificateConfigOption>(Configuration.GetSection("CertificateConfig"));

			services.AddAuthentication(
				CertificateAuthenticationDefaults.AuthenticationScheme)
				.AddCertificate(options =>
				{
					options.AllowedCertificateTypes = CertificateTypes.SelfSigned;
					options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

					options.Events = new CertificateAuthenticationEvents
					{
						OnCertificateValidated = CertificateValidated,
						OnAuthenticationFailed = CertificateAuthenticationFailed
					};
				});

			
			services.AddControllers();
			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1",
						new OpenApiInfo
						{
							Version = "v1",
							Title = _env.ApplicationName,
							Description = $"An InMemory backed Todo API on {siteName}",
							TermsOfService = new Uri("https://opensource.org/licenses/MIT"),
							Contact = new OpenApiContact
							{
								Name = "Pratik Khasnabis",
								Email = "api@softveda.net",
								Url = new Uri("https://github.com/softveda")
							},
							License = new OpenApiLicense
							{
								Name = "The MIT License",
								Url = new Uri("https://opensource.org/licenses/MIT")
							}

						});
				options.IgnoreObsoleteActions();
				options.IgnoreObsoleteProperties();

				OpenApiSecurityScheme bearerSecurityScheme = new OpenApiSecurityScheme()
				{
					Name = "Bearer",
					BearerFormat = "JWT",
					Scheme = "bearer",
					Description = "Specify the authorization token.",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
				};

				
				options.AddSecurityDefinition("jwt_auth", bearerSecurityScheme);
				

				OpenApiSecurityScheme apiKeySecurityScheme = new OpenApiSecurityScheme
				{
					Description = "Api key needed to access the endpoints. ocp-apim-subscription-key: value",
					In = ParameterLocation.Header,
					Name = "ocp-apim-subscription-key",
					Type = SecuritySchemeType.ApiKey
				};
				options.AddSecurityDefinition("api_key", apiKeySecurityScheme);

				OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
				{
						{bearerSecurityScheme, new string[] { }},
						{apiKeySecurityScheme, new string[] { }},
				};

				options.AddSecurityRequirement(securityRequirements);

				//OpenApiSecurityScheme oauth2ImplicitsecurityScheme = new OpenApiSecurityScheme()
				//{
				//	Type = SecuritySchemeType.OAuth2,
				//	Flows = new OpenApiOAuthFlows
				//	{
				//		AuthorizationCode = new OpenApiOAuthFlow
				//		{
				//			AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
				//			TokenUrl = new Uri("/auth-server/connect/token", UriKind.Relative),
				//			Scopes = new Dictionary<string, string>
				//				{
				//					{"readAccess", "Access read operations"},
				//					{"writeAccess", "Access write operations"}
				//				}
				//		}
				//	}
				//};

				//options.AddSecurityDefinition("oauth2", oauth2ImplicitsecurityScheme);				

				options.AddServer(new OpenApiServer
				{
					Url = "https://localhost:5001",
					Description = $"Environment: {_env.EnvironmentName}"
				});

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);
			});
		}

		public Task CertificateValidated(CertificateValidatedContext context)
		{
			// Check subject or thumbprint
			if (context.ClientCertificate.Subject != 
				"CN=cloudauth-apim.azure-api.net, O=CloudAuthority, C=AU")
			{
				context.Fail("Subject is unexpected");
			}
			else
			{
				context.Success();
			}

			return Task.CompletedTask;
		}

		public Task CertificateAuthenticationFailed(CertificateAuthenticationFailedContext context)
		{
			context.Fail("Certificate Authentication Failed");

			return Task.CompletedTask;
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
													ILogger<Startup> logger)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.Use(async (context, next) =>
			{
				// Request method, scheme, and path
				logger.LogDebug("Request Method: {Method}", context.Request.Method);
				logger.LogDebug("Request Scheme: {Scheme}", context.Request.Scheme);
				logger.LogDebug("Request Path: {Path}", context.Request.Path);

				// Headers
				logger.LogDebug("Header: ");
				foreach (var header in context.Request.Headers)
				{
					logger.LogDebug("\t{Key}: {Value}", header.Key, header.Value);
				}

				// Connection: RemoteIp
				logger.LogDebug("Request RemoteIp: {RemoteIpAddress}",
						context.Connection.RemoteIpAddress);

				await next();
			});

			//app.UseCertificateForwarding();
			app.UseClientCertMiddleware();
			

			app.UseAuthentication();

			// Inject Azure App Service name in a custom response header
			// to know from which App Service the response is returned
			app.Use(async (context, next) =>
			{
				context.Response.Headers.Add("APP-SERVICE-SITE-NAME", siteName);
				await next.Invoke();
			});


			app.UseSwagger();
			if (env.IsDevelopment())
			{
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API InMemory v1");
					c.RoutePrefix = string.Empty; // To serve the Swagger UI at the app's root 
				});
			}

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
