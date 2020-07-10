using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Softveda.Todo.Api.InMemory
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
				Host.CreateDefaultBuilder(args)
						.ConfigureWebHostDefaults(webBuilder =>
						{
							webBuilder.UseStartup<Startup>();
							webBuilder.ConfigureLogging(logging =>
							{
								logging.ClearProviders();
								logging.AddConsole();
								logging.AddDebug();
								logging.AddAzureWebAppDiagnostics();
							});
							webBuilder.ConfigureKestrel(o =>
							{
								o.ConfigureHttpsDefaults(o =>
									o.ClientCertificateMode =
											ClientCertificateMode.RequireCertificate);
							});
						});
	}
}
