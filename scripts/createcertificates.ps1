# Root CA
New-SelfSignedCertificate -DnsName "azure.cloudauthority.dev" -Subject "CN=azure.cloudauthority.dev, O=CloudAuthority, C=AU" -CertStoreLocation "cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(20) -KeyUsageProperty All -KeyUsage CertSign, CRLSign, DigitalSignature

# Open Windows 10 Manage User Certificate in control panel
# Find and cut the root cert from store "Personal" and paste in to "Trusted Root Certification Authorities"

$rootCertThumbprint = "<get it from certiticate store, certificate details>"
$certPassword = "p@ssw0rd"

$mypwd = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
Get-ChildItem -Path cert:\CurrentUser\my\$rootCertThumbprint | Export-PfxCertificate -FilePath root_ca_azure.cloudauthority.dev.pfx -Password $mypwd
Export-Certificate -Cert cert:\CurrentUser\my\$rootCertThumbprint -FilePath root_ca_azure.cloudauthority.dev.cer

# Child Cert from Root CA
$rootcert = ( Get-ChildItem -Path cert:\CurrentUser\My\$rootCertThumbprint )

New-SelfSignedCertificate -certstorelocation cert:\CurrentUser\my -dnsname "cloudauth-apim.azure-api.net, api.cloudauthority.dev" -Signer $rootcert -NotAfter (Get-Date).AddYears(2) -Subject "CN=cloudauth-apim.azure-api.net, O=CloudAuthority, C=AU" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")

$clientCertThumbprint = "<get it from certiticate store, certificate details>"

$mypwd = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
Get-ChildItem -Path cert:\CurrentUser\my\$clientCertThumbprint | Export-PfxCertificate -FilePath child_ca_cloudauth-apim.azure-api.pfx -Password $mypwd
Export-Certificate -Cert cert:\CurrentUser\my\$clientCertThumbprint -FilePath child_ca_cloudauth-apim.azure-api.cer