using System;
using System.Net;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.ConstrainedExecution;

namespace Softveda.Todo.Shared.Middleware
{

	public class ClientCertValidationMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ClientCertValidationMiddleware> _logger;
		private readonly CertificateConfigOption _config;

		public ClientCertValidationMiddleware(RequestDelegate next,
			ILogger<ClientCertValidationMiddleware> logger,
			IOptions<CertificateConfigOption> options)
		{
			_next = next;
			_logger = logger;
			_config = options.Value;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			_logger?.LogInformation("ClientCertValidationMiddleware Executing");
			// Skip check on health check endpoint
			if (string.Compare(context.Request.Path, "/Health", comparisonType: StringComparison.OrdinalIgnoreCase) == 0)
			{
				await _next.Invoke(context);
				return;
			}
			// Skip check if config value is true
			if (_config.SkipValidation)
			{
				await _next.Invoke(context);
				return;
			}
			try
			{
				var certHeader = context.Request.Headers[_config.Header];
				var clientCertBytes = Convert.FromBase64String(certHeader);
				var certificate = new X509Certificate2(clientCertBytes);
								
				var isValidCert = IsValidClientCertificate(certificate);

				if (!isValidCert)
				{
					_logger.LogWarning("Invalid Certificate: Request Forbidden");
					context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
					return;
				}
				await _next.Invoke(context);
			}
			catch
			{
				_logger.LogWarning("Certificate Exception: Request Forbidden");
				context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
			}
		}

		private bool IsValidClientCertificate(X509Certificate2 certificate)
		{
			// In this example we will only accept the certificate as a valid certificate if all the conditions below are met:
			// 1. The certificate is not expired and is active for the current time on server.
			// 2. The subject name of the certificate
			// 3. The issuer name of the certificate
			// 4. The thumbprint of the certificate
			//
			// This example does NOT test that this certificate is chained to a Trusted Root Authority (or revoked) on the server 
			// and it allows for self signed certificates
			//

			if (certificate is null)
			{
				_logger.LogWarning("Invalid Certificate: certificate not present error");
				return false;
			}

			var certSubject = certificate.Subject.Trim();
			var certIssuer = certificate.Issuer.Trim();
			var certThumbprint = certificate.Thumbprint.Trim().ToUpperInvariant();

			// 1. Check time validity of certificate
			if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 ||
				DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0)
			{
				_logger.LogWarning("Invalid Certificate: time validity error");
				return false;
			}

			// 2. Check subject name of certificate
			if (string.CompareOrdinal(certSubject, _config.Subject) != 0)
			{
				_logger.LogWarning("Invalid Certificate: subject name error");
				return false;
			}

			// 3. Check issuer name of certificate
			if (string.CompareOrdinal(certIssuer, _config.Issuer) != 0)
			{
				_logger.LogWarning("Invalid Certificate: issuer name error");
				return false;
			}

			// 4. Check thumbprint of certificate
			if (string.CompareOrdinal(certThumbprint, _config.ThumbPrint.ToUpperInvariant()) != 0)
			{
				_logger.LogWarning("Invalid Certificate: thumbprint error");
				return false;
			}

			return true;

			// 5. To test if the certificate chains to a Trusted Root Authority uncomment the code below
			//return IsValidCertificateChain(certificate);
			
		}

		private static bool IsValidCertificateChain(X509Certificate2 certificate)
		{
			X509Chain certChain = new X509Chain()
			{
				ChainPolicy = new X509ChainPolicy
				{
					RevocationMode = X509RevocationMode.NoCheck,
					//RevocationFlag = X509RevocationFlag.EntireChain,
					//UrlRetrievalTimeout = TimeSpan.FromSeconds(5),
				}
			};

			bool isValidCertChain = certChain.Build(certificate);
			if (!isValidCertChain) return false;

			if (certChain.ChainStatus.Any(status => status.Status != X509ChainStatusFlags.UntrustedRoot))
				return false;

			foreach (var chElement in certChain.ChainElements)
			{
				if (!chElement.Certificate.Verify())
				{
					isValidCertChain = false;
					break;
				}
			}
			if (!isValidCertChain) return false;

			return true;
		}
	}

	public static class ClientCertMiddlewareExtensions
	{
		public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<ClientCertValidationMiddleware>();
		}
		public static IApplicationBuilder UseClientCertMiddleware(this IApplicationBuilder builder, IOptions<CertificateConfigOption> options)
		{
			return builder.UseMiddleware<ClientCertValidationMiddleware>(options);
		}
	}
}
