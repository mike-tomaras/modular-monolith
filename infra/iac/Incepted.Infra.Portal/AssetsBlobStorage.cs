using Incepted.Infra.Shared;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;


namespace Incepted.Infra;
internal static class AssetsBlobStorage
{
    public static void Create(ResourceGroup resourceGroup, string location)
    {
        var storageAccount = new StorageAccount("sa", new StorageAccountArgs
        {
            AccountName = NameGenerator.GetName("assets", location).Replace("-", ""),
            ResourceGroupName = resourceGroup.Name,
            Sku = new SkuArgs
            {
                Name = SkuName.Standard_LRS
            },
            Kind = Kind.StorageV2,
            AllowBlobPublicAccess = true,
            MinimumTlsVersion = "TLS1_2"
        },
        new CustomResourceOptions { Parent = resourceGroup });
        
        new BlobContainer("ctnr-logos", new BlobContainerArgs
        {
            ContainerName = "logos",
            AccountName = storageAccount.Name,
            PublicAccess = PublicAccess.Blob,
            ResourceGroupName = resourceGroup.Name,
        },
        new CustomResourceOptions { Parent = storageAccount });

        //MANUAL: upload the logos
    }
}
