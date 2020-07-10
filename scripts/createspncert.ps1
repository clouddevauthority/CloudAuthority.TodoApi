$subjectName = "CN=AzureCliServicePrincipal"
$dnsName = "porgs.onmicrosoft.com"
$certStoreLocation = "cert:\CurrentUser\My"
$provider = "Microsoft Enhanced RSA and AES Cryptographic Provider"

$certFileName = "$($subjectName.Replace('CN=', '')).pfx"
$password = "p@ssw0rd"
$notAfter = (Get-Date).AddYears(1) # Valid for 1 year

$thumb = (New-SelfSignedCertificate -Subject $subjectName -DnsName $dnsName -CertStoreLocation $certStoreLocation -KeyExportPolicy Exportable -Provider $provider -NotAfter $notAfter).Thumbprint
$password = ConvertTo-SecureString -String $pwd -Force -AsPlainText
Export-PfxCertificate -cert "cert:\CurrentUser\my\$thumb" -FilePath $certFileName -Password $password

# https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest
$azureADTenantID = "d4b735a0-309a-44a0-a9c2-1bc074e5198c"
$subscriptionID = "015acb31-86ab-4cae-a43d-b6baaf6a527"
$appName = "http://AzureCliCertSpn"
$certName = "AzureCliCertSpn"
$keyVaultName = "management-automation-kv"
$rgName = "Management"
$location = "westus"
$certPermissions = "backup create delete deleteissuers get getissuers import list listissuers managecontacts manageissuers purge recover restore setissuers update"
$keyPermissions = "backup create decrypt delete encrypt get import list purge recover restore sign unwrapKey update verify wrapKey"
$secretPermissions = "backup delete get list purge recover restore set"
$storagePermissions = "backup delete deletesas get getsas list listsas purge recover regeneratekey restore set setsas update"
az ad sp create-for-rbac -name $appName --role contributor --scopes /subscriptions/$subscriptionID --create-cert
az ad sp list --display-name AzureCliCertSpn --query "[].appId" -o tsv
az keyvault create -g $rgName -n $keyVaultName --location $location
az keyvault set-policy -g $rgName -n $keyVaultName  --spn $appName --certificate-permissions $certPermissions --key-permissions $keyPermissions --secret-permissions $secretPermissions --storage-permissions $storagePermissions
az keyvault certificate import --vault-name $keyVaultName -n $certName -f "$($certName).pem" 
