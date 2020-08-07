namespace Softveda.Todo.Shared.Middleware
{
	public class CertificateConfigOption
	{
		public const string CertificateConfig = "CertificateConfig";

		public string Header { get; set; }
		public bool SkipValidation { get; set; }
		public string Subject { get; set; }
		public string Issuer { get; set; }
		public string ThumbPrint { get; set; }
	}
}
