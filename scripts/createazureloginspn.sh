#!/bin/bash

# https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest
azureADTenantID = "d4b735a0-309a-44a0-a9c2-1bc074e5198c"
subscriptionID = "015acb31-86ab-4cae-a43d-b6baaf6a527"
appName = "http://AzureCliCertSpn"
certName="AzureCliCertSpn"
keyVaultName="management-automation-kv"
rgName="Management"
region="westus"

az ad sp create-for-rbac -name $appName --role contributor \
  --scopes /subscriptions/$subscriptionID --create-cert
az ad sp list --display-name AzureCliCertSpn --query "[].appId" -o tsv
az keyvault create -g $rgName -n $keyVaultName --region $region
az keyvault set-policy -g $rgName -n $keyVaultName  --spn $appName \
  --certificate-permissions $certPermissions --key-permissions $keyPermissions --secret-permissions $secretPermissions --storage-permissions $storagePermissions
az keyvault certificate import --vault-name $keyVaultName -n $certName \
  -f "$certName.pem" 