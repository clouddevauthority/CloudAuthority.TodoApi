#!/bin/bash

srcPath=../src/api/Softveda.Todo.Api.InMemory
rm -rf $srcPath"/pub"
dotnet publish -o $srcPath"/pub" $srcPath"/Softveda.Todo.Api.InMemory.csproj"
cd $srcPath"/pub"
zip site.zip *
cd -
test -f $srcPath"/pub/site.zip" && echo "deployment zip package exists."

rgName=todoapi-rg
appPrefix=todoapi-inmemory
az webapp deployment source config-zip -g $rgName -n $appPrefix"-pri" --src $srcPath"/pub/site.zip"
az webapp deployment source config-zip -g $rgName -n $appPrefix"-sec" --src $srcPath"/pub/site.zip"
