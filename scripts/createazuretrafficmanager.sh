#!/bin/bash

rgName=todoapi-rg
appPrefix=todoapi-inmemory

az network traffic-manager profile create -g $rgName -n $appPrefix"-tm" \
  --unique-dns-name $appPrefix --path "/inMemory/Health" \
  --routing-method Weighted --ttl 10 --protocol HTTPS --port 443 \
  --tags env=dev app=$appPrefix persist=false

webAppPriId="/subscriptions/015acb31-86ab-4cae-a43d-b6baaf6a5275/resourceGroups/todoapi-rg/providers/Microsoft.Web/sites/todoapi-inmemory-pri"
az network traffic-manager endpoint create -g $rgName -n pri-endoint \
  --profile-name $appPrefix"-tm" --weight 1 \  
  --type azureEndpoints --target-resource-id $webAppPriId

webAppSecId="/subscriptions/015acb31-86ab-4cae-a43d-b6baaf6a5275/resourceGroups/todoapi-rg/providers/Microsoft.Web/sites/todoapi-inmemory-sec"
az network traffic-manager endpoint create -g $rgName -n sec-endoint \
  --profile-name $appPrefix"-tm" --weight 1 \
  --type azureEndpoints --target-resource-id $webAppSecId