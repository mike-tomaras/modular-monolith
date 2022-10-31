using Incepted.Infra.Shared;
using Pulumi;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.KeyVault;
using Pulumi.AzureNative.KeyVault.Inputs;
using SecretArgs = Pulumi.AzureNative.KeyVault.SecretArgs;
using Deployment = Pulumi.Deployment;

namespace Incepted.Infra;

class InceptedSecOpsStack : Stack
{
    public InceptedSecOpsStack()
    {
        var config = new Config();
        var location = config.Require("azn-location");

        var resourceGroup = new ResourceGroup("rsgrp", new ResourceGroupArgs
        {
            ResourceGroupName = NameGenerator.GetName("spike-kv-rsgrp", location)
        });
        
        var portalStack = new StackReference($"incepted/portal/{Deployment.Instance.StackName}");

        var keyvault = CreateKeyVault(resourceGroup, location, config, portalStack);
        CreateDbConnStrSecret(resourceGroup, keyvault, location, portalStack);
        CreateAppInsightsSecrets(resourceGroup, keyvault, portalStack);
        CreateDealFilesStorageBlobContainerAndConnStringSecret(resourceGroup, location, keyvault);
        CreateMailerSendSecret(resourceGroup, keyvault, config);
    }

    private Vault CreateKeyVault(ResourceGroup resourceGroup, string location, Config config, StackReference portalStack)
    {
        var azPulumiAppId = config.Require("azn-clientId");
        var azTenantId = config.Require("azn-tenantId");

        var webappPrincipalId = portalStack.GetOutput("portalWebAppPrincipalId").Apply(id => id != null ? id.ToString() : throw new ArgumentException("webapp principalId is null when trying to assign it to the key vault access policy. Did you not output it from the portal project, same stack? Run that first."));

        var keyvault = new Vault("kv", new VaultArgs
        {
            ResourceGroupName = resourceGroup.Name,
            VaultName = NameGenerator.GetEnvName("kvprtl", location),
            Location = location,
            Properties = new VaultPropertiesArgs
            {
                AccessPolicies =
                {
                    new AccessPolicyEntryArgs
                    {
                        ObjectId = webappPrincipalId,
                        Permissions = new PermissionsArgs
                        {
                            Certificates =
                            {
                                "get",
                                "list"
                            },
                            Secrets =
                            {
                                "get",
                                "list",
                            },
                        },
                        TenantId = azTenantId,
                    },
                    new AccessPolicyEntryArgs
                    {
                        ObjectId = azPulumiAppId,
                        Permissions = new PermissionsArgs
                        {
                            Certificates =
                            {
                                "get",
                                "list",
                                "create",
                            },
                            Secrets =
                            {
                                "get",
                                "list",
                                "set",
                                "delete"
                            },
                        },
                        TenantId = azTenantId,
                    },
                },
                EnabledForDeployment = true,
                EnabledForDiskEncryption = true,
                EnabledForTemplateDeployment = true,
                Sku = new SkuArgs
                {
                    Family = "A",
                    Name = Pulumi.AzureNative.KeyVault.SkuName.Standard
                },
                TenantId = azTenantId,
            }
        });

        return keyvault;
    }

    private void CreateDbConnStrSecret(ResourceGroup resourceGroup, Vault keyvault, string location, StackReference portalStack)
    {
        var cosmosDbConnStr = portalStack.GetOutput("cosmosDbConnStr").Apply(id => id != null ? id.ToString() : throw new ArgumentException("cosmos db account key is null when trying to assign it to add it to the vault. Did you not output it from the portal project, same stack? Run that first."));

        var sqlServerName = NameGenerator.GetEnvName(ResourceNames.PortalDbServer, location);
        var dbName = NameGenerator.GetEnvName(ResourceNames.PortalDbName, location);

        new Secret("dbConnString", new SecretArgs
        {
            VaultName = keyvault.Name,
            ResourceGroupName = resourceGroup.Name,
            SecretName = SecretNames.PortalDb,
            Properties = new SecretPropertiesArgs
            {
                Value = cosmosDbConnStr,
            },
        },
        new CustomResourceOptions { Parent = keyvault });
    }

    private void CreateAppInsightsSecrets(ResourceGroup resourceGroup, Vault keyvault, StackReference portalStack)
    {
        var appInsightsInstrumentationKey = portalStack.GetOutput("appInsightsInstrumentationKey").Apply(key => key != null ? key.ToString() : throw new ArgumentException("App insights key is null when trying to add it to the key vault. Did you not output it from the portal project, same stack? Run that first."));

        //add secrets
        var appInsightsKeySecret = new Secret("appInsightsKey", new SecretArgs
        {
            VaultName = keyvault.Name,
            ResourceGroupName = resourceGroup.Name,
            SecretName = SecretNames.AppInsightsInstrumentationKey,
            Properties = new SecretPropertiesArgs
            {
                Value = appInsightsInstrumentationKey
            },
        },
        new CustomResourceOptions { Parent = keyvault });

        new Secret("appInsightsConnStr", new SecretArgs
        {
            VaultName = keyvault.Name,
            ResourceGroupName = resourceGroup.Name,
            SecretName = SecretNames.AppInsightsConnString,
            Properties = new SecretPropertiesArgs
            {
                Value = appInsightsInstrumentationKey.Apply(key => $"InstrumentationKey={key}")
            },
        },
        new CustomResourceOptions { Parent = keyvault });
    }

    private void CreateDealFilesStorageBlobContainerAndConnStringSecret(ResourceGroup resourceGroup, string location, Vault keyvault)
    {
        var storageAccountName = NameGenerator.GetName("dealfiles", location).Replace("-", "");

        var storageKeys = ListStorageAccountKeys
            .Invoke(new ListStorageAccountKeysInvokeArgs
            {
                AccountName = storageAccountName,
                ResourceGroupName = NameGenerator.GetName(ResourceNames.PortalResourceGroup, location)
            });

        new Secret("dealFilesStorageConnString", new SecretArgs
        {
            VaultName = keyvault.Name,
            ResourceGroupName = resourceGroup.Name,
            SecretName = SecretNames.DealFilesBlobConnString,
            Properties = new SecretPropertiesArgs
            {
                Value = storageKeys.Apply(keysList => $"DefaultEndpointsProtocol=https;" +
                $"AccountName={storageAccountName};" +
                $"AccountKey={keysList.Keys.First().Value};" +
                $"EndpointSuffix=core.windows.net")
            },
        },
        new CustomResourceOptions { Parent = keyvault });
    }

    private void CreateMailerSendSecret(ResourceGroup resourceGroup, Vault keyvault, Config config)
    {
        var apiKey = config.Require("mailerSendApiKey");
        new Secret("mailerSendApiKey", new SecretArgs
        {
            VaultName = keyvault.Name,
            ResourceGroupName = resourceGroup.Name,
            SecretName = SecretNames.MailerSendApiKey,
            Properties = new SecretPropertiesArgs
            {
                Value = apiKey
            },
        },
        new CustomResourceOptions { Parent = keyvault });
    }
}
