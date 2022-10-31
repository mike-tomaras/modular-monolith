using Incepted.Infra.Shared;
using Pulumi;
using Pulumi.AzureNative.DocumentDB;
using Pulumi.AzureNative.Insights;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using DocDbInputs = Pulumi.AzureNative.DocumentDB.Inputs;

namespace Incepted.Infra;

class InceptedPortalStack : Stack
{
    [Output("portalWebAppPrincipalId")] public Output<string> PortalWebAppPrincipalId { get; set; }
    [Output("appInsightsInstrumentationKey")] public Output<string> AppInsightsInstrumentationKey { get; set; }
    [Output("cosmosDbConnStr")] public Output<string> CosmosDbConnStr { get; set; }


    public InceptedPortalStack()
    {
        var config = new Config();
        var location = config.Require("azn-location");
        
        var resourceGroup = new ResourceGroup("rsgrp", new ResourceGroupArgs
        {
            ResourceGroupName = NameGenerator.GetName(ResourceNames.PortalResourceGroup, location)
        });

        //AssetsBlobStorage.Create(resourceGroup, location);
        CreateCosmosDb(resourceGroup, location);
        CreateWebapp(resourceGroup, location);
    }

    private void CreateCosmosDb(ResourceGroup resourceGroup, string location)
    {

        var cosmosdbAccount = new DatabaseAccount("cosmosdbaccount", new DatabaseAccountArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = NameGenerator.GetName("docdbaccnt", location),
            DatabaseAccountOfferType = DatabaseAccountOfferType.Standard,
            Locations =
            {
                new DocDbInputs.LocationArgs
                {
                    LocationName = resourceGroup.Location,
                    FailoverPriority = 0,
                },
            },
            BackupPolicy = new DocDbInputs.PeriodicModeBackupPolicyArgs
            { 
                PeriodicModeProperties = new DocDbInputs.PeriodicModePropertiesArgs
                {
                    BackupIntervalInMinutes = 240,
                    BackupRetentionIntervalInHours = 8,                    
                },
                Type = "Periodic"

            },
            EnableAnalyticalStorage = true,
            EnableFreeTier = true,
            ConsistencyPolicy = new DocDbInputs.ConsistencyPolicyArgs
            {
                DefaultConsistencyLevel = DefaultConsistencyLevel.Session,
            },
        },
        new CustomResourceOptions { Parent = resourceGroup });

        var connStr = ListDatabaseAccountConnectionStrings.Invoke(new ListDatabaseAccountConnectionStringsInvokeArgs
        {
            AccountName = cosmosdbAccount.Name,
            ResourceGroupName = resourceGroup.Name
        });
        CosmosDbConnStr = Output.CreateSecret(connStr).Apply(cs => cs.ConnectionStrings.First().ConnectionString);

        // Cosmos DB Database
        var db = new SqlResourceSqlDatabase("sqldb", new SqlResourceSqlDatabaseArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = cosmosdbAccount.Name,
            DatabaseName = "portaldb",
            Location = resourceGroup.Location,
            Resource = new DocDbInputs.SqlDatabaseResourceArgs
            {
                Id = "portaldb",
            },
            Options = new DocDbInputs.CreateUpdateOptionsArgs
            {
                AutoscaleSettings = new DocDbInputs.AutoscaleSettingsArgs { MaxThroughput = 1000 }
            }
        },
        new CustomResourceOptions { Parent = cosmosdbAccount });

        // Cosmos DB SQL Containers
        new SqlResourceSqlContainer("dealcontainer", new SqlResourceSqlContainerArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = cosmosdbAccount.Name,
            DatabaseName = db.Name,
            ContainerName = "deals",
            Location = resourceGroup.Location,
            Resource = new DocDbInputs.SqlContainerResourceArgs
            {
                Id = "deals",
                PartitionKey = new DocDbInputs.ContainerPartitionKeyArgs { Paths = { "/dealId" }, Kind = "Hash", Version = 1 },
                IndexingPolicy = new DocDbInputs.IndexingPolicyArgs
                {
                    Automatic = true,
                    ExcludedPaths = new[]
                    {
                        new DocDbInputs.ExcludedPathArgs
                        {
                            Path = "/*"
                        }
                    },
                    IncludedPaths = new[]
                    {
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/Type/?"
                        },
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/InsuranceCompanyId/?"
                        },
                    },
                    IndexingMode = IndexingMode.Consistent,
                }
            },
        },
        new CustomResourceOptions { Parent = db });

        new SqlResourceSqlContainer("companycontainer", new SqlResourceSqlContainerArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = cosmosdbAccount.Name,
            DatabaseName = db.Name,
            ContainerName = "companies",
            Location = resourceGroup.Location,
            Resource = new DocDbInputs.SqlContainerResourceArgs
            {
                Id = "companies",
                PartitionKey = new DocDbInputs.ContainerPartitionKeyArgs { Paths = { "/companyId" }, Kind = "Hash", Version = 1 },
                IndexingPolicy = new DocDbInputs.IndexingPolicyArgs
                {
                    Automatic = true,
                    ExcludedPaths = new[]
                    {
                        new DocDbInputs.ExcludedPathArgs
                        {
                            Path = "/*"
                        }
                    },
                    IncludedPaths = new[]
                    {
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/Type/?",
                        },
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/CompanyType/?",
                        },
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/UserId/?",
                        },
                    },
                    IndexingMode = IndexingMode.Consistent,
                }
            },
        },
        new CustomResourceOptions { Parent = db });

        new SqlResourceSqlContainer("deallistcontainer", new SqlResourceSqlContainerArgs
        {
            ResourceGroupName = resourceGroup.Name,
            AccountName = cosmosdbAccount.Name,
            DatabaseName = db.Name,
            ContainerName = "deals-list",
            Location = resourceGroup.Location,
            Resource = new DocDbInputs.SqlContainerResourceArgs
            {
                Id = "deals-list",
                PartitionKey = new DocDbInputs.ContainerPartitionKeyArgs { Paths = { "/companyId" }, Kind = "Hash", Version = 1 },
                IndexingPolicy = new DocDbInputs.IndexingPolicyArgs
                {
                    Automatic = true,
                    ExcludedPaths = new[] //TODO
                    {
                        new DocDbInputs.ExcludedPathArgs
                        {
                            Path = "/*"
                        }
                    },
                    IncludedPaths = new[]
                    {
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/Type/?",
                        },
                        new DocDbInputs.IncludedPathArgs
                        {
                            Path = "/SubmissionId/?",
                        },
                    },
                    IndexingMode = IndexingMode.Consistent,
                }
            },
        },
        new CustomResourceOptions { Parent = db });
    }

    private void CreateWebapp(ResourceGroup resourceGroup, string location)
    {
        var appServicePlan = new AppServicePlan("asp", new AppServicePlanArgs
        {
            ResourceGroupName = resourceGroup.Name,
            Name = NameGenerator.GetName("appserviceplan", location),
            Kind = "App",
            Sku = new SkuDescriptionArgs
            {
                Tier = "Free",
                Name = "F1",
            }
        },
        new CustomResourceOptions { Parent = resourceGroup });

        var appInsights = new Component("appInsights", new ComponentArgs
        {
            ResourceName = NameGenerator.GetName("web-portal", location),
            ApplicationType = "web",
            Kind = "web",
            ResourceGroupName = resourceGroup.Name,
        },
        new CustomResourceOptions { Parent = resourceGroup });
        AppInsightsInstrumentationKey = appInsights.InstrumentationKey;

        var webapp = new WebApp("app", new WebAppArgs
        {
            Name = NameGenerator.GetName("web-portal", location),
            ResourceGroupName = resourceGroup.Name,
            ServerFarmId = appServicePlan.Id,
            HttpsOnly = true,
            Identity = new ManagedServiceIdentityArgs
            {
                Type = Pulumi.AzureNative.Web.ManagedServiceIdentityType.SystemAssigned
            }
        },
        new CustomResourceOptions { Parent = appServicePlan });
        PortalWebAppPrincipalId = webapp.Identity.Apply(i => i.PrincipalId != null ? i.PrincipalId : throw new ArgumentException("webapp Identity is null when trying to assigning it to the key vault access policy"));

        CreateWebAppConfig(resourceGroup, webapp, location);


        new StorageAccount("files-sa", new StorageAccountArgs
        {
            AccountName = NameGenerator.GetName("dealfiles", location).Replace("-", ""),
            ResourceGroupName = resourceGroup.Name,
            Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            Kind = Pulumi.AzureNative.Storage.Kind.StorageV2,
            EnableHttpsTrafficOnly = true,
            AllowBlobPublicAccess = false,
            MinimumTlsVersion = "TLS1_2"
        },
        new CustomResourceOptions { Parent = resourceGroup });
    }

    private void CreateWebAppConfig(ResourceGroup resourceGroup, WebApp webapp, string location)
    {
        var keyvaultName = NameGenerator.GetEnvName("kvprtl", location);

        new WebAppApplicationSettings("appSettings", new WebAppApplicationSettingsArgs
        {
            Name = webapp.Name,
            ResourceGroupName = resourceGroup.Name,
            Properties =
            {
                {
                    "XDT_MicrosoftApplicationInsights_Mode",
                    "recommended"
                },
                {
                    "APPINSIGHTS_INSTRUMENTATIONKEY",
                    Output.Format($"@Microsoft.KeyVault(VaultName={keyvaultName};SecretName={SecretNames.AppInsightsInstrumentationKey})")
                },
                {
                    "APPLICATIONINSIGHTS_CONNECTION_STRING",
                    Output.Format($"@Microsoft.KeyVault(VaultName={keyvaultName};SecretName={SecretNames.AppInsightsConnString})")
                },
                {
                    "ApplicationInsightsAgent_EXTENSION_VERSION",
                    "~3"
                },
                {
                    "ASPNETCORE_ENVIRONMENT", //MANUAL: Make it a Deployment Slot Setting
                    "prod"
                },
                {
                    "dbConnString", //MANUAL: Make it a Deployment Slot Setting
                    Output.Format($"@Microsoft.KeyVault(VaultName={keyvaultName};SecretName={SecretNames.PortalDb})")
                },
                {
                    "DealFiles__StorageConnString", //MANUAL: Make it a Deployment Slot Setting
                    Output.Format($"@Microsoft.KeyVault(VaultName={keyvaultName};SecretName={SecretNames.DealFilesBlobConnString})")
                },
                {
                    "MailerSend__ApiKey", //MANUAL: Make it a Deployment Slot Setting
                    Output.Format($"@Microsoft.KeyVault(VaultName={keyvaultName};SecretName={SecretNames.MailerSendApiKey})")
                },
            }
        },
        new CustomResourceOptions { Parent = webapp });
    }
}
