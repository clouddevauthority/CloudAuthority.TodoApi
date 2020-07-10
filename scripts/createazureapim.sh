#!/bin/bash

region=westus
rg=edgenetwork-rg
prefix=cloudauth
az group create --location $region --name $rg
az apim create -g $rg -n "$prefix-apim" -l $region \
  --sku-name Developer \
  --publisher-email azure@acloudauthority.dev --publisher-name CloudAuthority \
  --tags env=prod app=management persist=true
