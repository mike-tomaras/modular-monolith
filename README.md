[![CI](https://github.com/inceptedio/portal/actions/workflows/main.yml/badge.svg)](https://github.com/inceptedio/portal/actions/workflows/main.yml)

# App
This app contains all the code for the incepted platform.

## Coding stack (for a Windows setup)
* dotnet core 6
* Blazor WASM hosted - [Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-6.0#blazor-webassembly)
* MudBlazor - [Docs](https://mudblazor.com/getting-started/installation#prerequisites)
* Serilog - [Docs](https://github.com/serilog/serilog/wiki/Getting-Started)
    * Using a Seq sink for local development and a local Seq server running in a container (See "Devving").
    * [Tutorial](https://nblumhardt.com/2019/11/serilog-blazor/) on how to setup Serilog for streaming logs from the wasm client to the API sinks.
* Testing:
    * [Nunit](https://docs.nunit.org/articles/nunit/intro.html)
    * [BUnit](https://bunit.dev/docs/getting-started/writing-tests.html?tabs=xunit)
    * [FluentAssertions](https://fluentassertions.com/introduction)
    * Moq/Autofixture
* Infra
    * [azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli)
    * [pulumi](https://app.pulumi.com/incepted)

## Infra stack
* AppService
* CosmosDB (with embedded Azure Synapse Analytics)
* Keyvalut
* Azure BLOB storage

## How to work
### Initial setup
* Download repo.
* Install [CosmosDB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) and run it.
* Replace all `{{some config}}` instances in `init.ps1` and run it.
 * Please replicate the steps in the init script if it does not complete correctly.
 * If it hasn't, start the Seq server with `docker run --name seq -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest`. You can see the log stream at `localhost:5341`.
 * If it hasn't, start the local BLOB service with  `docker run --name azurite -p 10000:10000 -v {{path to your repo}}/infra/az-local-storage:/workspace mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0 --oauth basic --cert /workspace/certs/127.0.0.1.pem --key /workspace/certs/127.0.0.1-key.pem -l /workspace/files`.

### Devving
* (Visual Studio) Debug locally with F5.
* (VS Code) Debug locally with `dotnet run`.
* Run the tests with `dotnet test` from the `test` folder. 
    * There is a pre-commit hook that will run the tests first.
    * The API csproj has integration tests, with fake services.
    * The Client csproj has component tests, using bUnit.
    * The rest of the csproj have unit tests. 

* Razor syntax misbehaving in VS? Clear the folder `C:\Users\{your name}\AppData\Local\Microsoft\VisualStudio\17.0_{some hash}\ComponentModelCache` and restart. 

### CI + infra
* [CI pipeline](/documentation/ci.md)
* [Infrastructure as code](/documentation//iac.md)

### Security
* [Authentication and authorization](/documentation/auth.md)
* [Secrets](/documentation/secrets.md)

### Architecture
* [Notes on architecture approach](/documentation/architecture.md)

### Database
* [Notes on the selection and implementation of the db](/documentation/db.md)

## Notes
* This is a _first iteration_ solution, striving for minimal complexity at the expense of increased coupling. It will evolve as it grows.
* There are mutliple "services" (`Domain.*` projects) but only one API project with a controller per "service". The bounded context services are all separate projects but have the same presentation layer: the HTTP API `Incepted.API` project. The bounded context services that are used only by other bounded context services are contacted via the programmatic API within the sln (not HTTP).
