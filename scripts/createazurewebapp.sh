#!/bin/bash

rgName=todoapi-rg
planPrefix=todoapi
appPrefix=todoapi-inmemory
priLocation=westus
secLocation=eastus

az group create --location $priLocation --name $rgName

# az appservice plan create -g $rgName -n $appPrefix-"f1-plan" --sku F1 --tags env=dev app=$appPrefix persist=false

# primary
# create plan
az appservice plan create -g $rgName -n $planPrefix"-s1-plan-pri" --location $priLocation \
 --sku S1 --tags env=dev app=$appPrefix persist=false
# create webapp
az webapp create -g $rgName --plan $planPrefix"-s1-plan-pri" -n $appPrefix"-pri" \
 --tags env=dev app=$appPrefix persist=false
# Configure webapp
az webapp update -g $rgName -n $appPrefix"-pri" --client-affinity-enabled false --https-only true
az webapp log config -g $rgName -n $appPrefix"-pri" --application-logging true --level information
az webapp config show -g $rgName -n $appPrefix"-pri" -o yaml


# secondary
# create plan
az appservice plan create -g $rgName -n $planPrefix-"s1-plan-sec" --location $secLocation \
 --sku S1 --tags env=dev app=$appPrefix persist=false
# create webapp
az webapp create -g $rgName --plan $planPrefix"-s1-plan-sec" -n $appPrefix"-sec" \
 --tags env=dev app=$appPrefix persist=false
# configure webapp
az webapp update -g $rgName -n $appPrefix"-sec" --client-affinity-enabled false --https-only true
az webapp log config -g $rgName -n $appPrefix"-sec" --application-logging true --level information
az webapp config show -g $rgName -n $appPrefix"-sec" -o yaml

# configure client certificates
az webapp update -g $rgName -n $appPrefix"-pri" --set clientCertEnabled=true
az webapp update -g $rgName -n $appPrefix"-pri" --set clientCertExclusionPaths="/Health"

az webapp update -g $rgName -n $appPrefix"-sec" --set clientCertEnabled=true
az webapp update -g $rgName -n $appPrefix"-sec" --set clientCertExclusionPaths="/Health"


# configure access control
apimpublicip=$(az apim show -g edgenetwork-rg -n cloudauth-apim --query "publicIpAddresses[0]" -o tsv)

az webapp config access-restriction add -g $rgName -n $appPrefix"-pri" \
    --rule-name 'Allow APIM Only' --action Allow --ip-address $apimpublicip --priority 100

az webapp config access-restriction add -g $rgName -n $appPrefix"-sec" \
    --rule-name 'Allow APIM Only' --action Allow --ip-address $apimpublicip --priority 100