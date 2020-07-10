dotnet new tool-manifest
dotnet tool install --version 5.4.1 Swashbuckle.AspNetCore.Cli

dotnet swagger tofile --output ../src/api/Softveda.Todo.Api.InMemory/OpenApi.yaml --yaml ../src/api/Softveda.Todo.Api.InMemory/bin/Debug/netcoreapp3.1/Softveda.Todo.Api.InMemory.dll v1
dotnet swagger tofile --output ../src/api/Softveda.Todo.Api.InMemory/OpenApi.json ../src/api/Softveda.Todo.Api.InMemory/bin/Debug/netcoreapp3.1/Softveda.Todo.Api.InMemory.dll v1

dotnet swagger tofile --output ../src/api/Softveda.Todo.Api.Table/OpenApi.json ../src/api/Softveda.Todo.Api.Table/bin/Debug/netcoreapp3.1/Softveda.Todo.Api.Table.dll v1
dotnet swagger tofile --output ../src/api/Softveda.Todo.Api.Table/OpenApi.yaml --yaml ../src/api/Softveda.Todo.Api.Table/bin/Debug/netcoreapp3.1/Softveda.Todo.Api.Table.dll v1