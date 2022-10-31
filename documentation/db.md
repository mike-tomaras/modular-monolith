# Database

We have opted in for CosmosDB, a NoSQL database. The decision was made for with the following in mind:
* We planned the schema in a relational db and estimated the implementation effort to be similar.
* Each deal is pretty self contained so a document structure is not complex. The only big denormalization needed that adds complexity is for the company/deal relationship but since it's only one we accept it (for now).
* We also had to add some denormalization for loading the lists of deals (submission/feedback/live) but that adds RU cost when running any update.
* The pricing of CosmosDB can be proportional to the usage and has a nice free tier (1000 RU/s) that will give us a free db at least until the MVP (the lowest production level SQL DB costs several hundred USD per month). 
* There is a built in integration with Azure Synapse for data analytics straight from the NoSQL db. The document values are turned into a column database ready for analytics and ETL/ELT pipelines. 
* The operational overhead of CosmosDB is significantly lower than that of an SQL Server (even if it's a managed one). 

## Schema
There is a single db account and a single database called `portaldb`. There is at least one container per Domain. 

### Company domain
* There is a `companies` container with the `companyId` being the partition key.
  * There is a `Company` data model with all the details of a company, including its employees. 
  * There is an `EmployeeLookup` data model to find an employee by userId and then get his companyId from there. A denormalization for lower RU consumption when looking for the company of an employee.
### Deal domain
* There is a `deals` container with the `dealId` being the partition key.
  * There is a data model per stage of the deal (a `submission` one, a `feedback` one and a `livedeal` one). Each one contains all the details it needs, including assignees. Some details between them are duplicated (denormalized), for example Enterprise Value and Deal Name.
* There is a `deals-list` container with the `companyId` being the partition key.
  * Another denormalization to allow for the search of all deals of a company since the `deals` container is partitioned by dealId and would make searching by companyId too expensive in RU terms. 
  * There is a single data model for the list item of any stage of the deal (they are pretty similar). 

## Notes
* We do optimistic concurrency updating by using the built in `_etag` property that CosmosDB adds to every document. We only need it for the main domain objects of `Company`, `Submission` and `Feedback`.
* When saving more than one type of document in a container we use the `Type` attribute to distinguish between them. 
* The CosmosDB nuget uses `Newtonsoft` for json but the rest of our solution uses `System.Text.Json.Serialization` so there are some places where we tag properties/constructors with both. 

## TODO
* Updating employee details in the companies domain and container needs to update assignees in the deal domain and container. 
* Consider using a cache (in mem at the start) to speed up queries and save even more on RUs.