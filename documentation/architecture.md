# Architecture

The solution is following the modular monolith approach. Read more [here](https://modular-monolith.knowledge.devmentors.io/fundamentals/overview).

It is also adhering to Clean/Onion/Hexagonal Architecture principles. Read more [here](https://betterprogramming.pub/the-clean-architecture-beginners-guide-e4b7058c1165). [Here](https://miro.medium.com/max/462/1*0Pg6_UsaKiiEqUV3kf2HXg.png) is a visual that will help you uderstand the following.

We are using DDD principles in identifying/separating bounded contexts and implementing business domain logic. Read more [here](https://martinfowler.com/tags/domain%20driven%20design.html).

## System architecture
The `sln` structure (not the folder structure but the architectural grouping of `.csproj`s): 

- src
 - Domain
 - Presentation
 - Integrations
 - Shared

### Explanations
* `Domain/Incepted.Domain.*` projects: The center of the Onion/Clean circle on a sln level. Projects that contain the business logic and (counter intuitively) the data/db integrations (for now). Each project adheres to the Clean architecture design itself: the `Domain` subfolder is the center of the architecture and follows DDD principles. The `Application` subfolder is the next layer and contains the use cases (as a service class for now but can be refactored to CQRS). The `Repo` folder is the next layer of persistence integration. Other integrations used by the application layer are external projects to this one. The presentation integration is programmatic at this point, consumed by the `Incepted.API` project.
* `Presentation/Incepted.API` project: The single HTTP REST API for all `Domain` projects. Each Domain has its own controller. We should technically have an API presentation layer per Domain project but we opted for simplicity as Blazor wasm hosted comes preconfigured with a single API. 
* `Presentation/Incepted.Client` project: The SPA client. Uses the single API as a back-end.
* `Shared/Incepted.Shared` project: Cross cutting concerns and common DTOs between Domains and Clients. 

### Principles
* One `Domain` project per bounded context. 
* Separate datastore per bounded context (not necessarily on an infra level, can be separate sql servers, or same sql server with one db per domain, or same sql server with one db with one schema per domain, depends on performance and security specs).
* Single REST API for all domains with one controller per domain.
* No direct communication between domains. Application layers coordinate that if needed with in mem communication for now. It may evolve into an in memory async bus and even separate the domain into another repo.
* The only public classes/interfaces are the `I{Domain name}Service` and the Domain aggregates/entities/value types. The rest are internal.

## Code Architecture
* Mixing functional programming princicples with OO ones by using [Options](https://github.com/nlkl/Optional) and pushing impure code (IO, Exceptions) to the ends of the core bussiness modules (WIP, not all side effects are covered). Also using immutable concepts like ImmutableList and init setters as much as possible. It's not perfect, it's work in progress.
* Test first. All tests can run with no dependencies in the build stage. We have UI component tests for the `Client` project, REST integration tests for the `API` project and unit tests for everything else. 
* Dependency inversion: dependencies always flow from the center of the onion architecture (Domain folders) to the next layer (Application folders) and the next (Repos, Presentation and other integrations). Domain classes _only_ depend on each other. All functionality that the Application needs exists in interfaces inside its folder (or dedicated project) and are implemented by the integrations as needed.  

## Thinking behind the architecture
We are trying to create an architecture that will offer the most flexibility for whatever shape the Bussiness Domain may take in the future, however the integrations might change and whatever the system architecture might be (monolith of granular microservices). Each `Domain` project should be able to be extracted at any point into its own repo/service with a dedicated presentation layer (REST, gRPC, async) and its own data store and other integrations. 

## Acknowledged tech debt
* There is a single `update` endpoint and domain action for all kinds of updating of a deal from the UI. This means that even if we change a single thing about a deal, the whole thing will be sent over the wire. We exchanged code simplicity for latency and higher bandwidth. 
* Service methods retrieve the whole entity from the db (may be several tables), act on it and save it whole again. We exchanged bigger db reads and writes for a simpler repo interface and reusability (instead of different methods to retrieve/update differents parts of an entity or aggregate).