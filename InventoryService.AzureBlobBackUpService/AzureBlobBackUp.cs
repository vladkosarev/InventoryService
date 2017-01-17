using InventoryService.BackUpService;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using CloudBlobClient = Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

namespace InventoryService.AzureBlobBackUpService
{
    public class AzureBlobBackUp : IBackUpService
    {
        private CloudBlobClient BlobClient { get; set; }
        private AzureBlobConfiguration AzureBlobConfiguration { get; set; }

        public AzureBlobBackUp()
        {
        }

        public bool BackUp(string name, string content)
        {
            if (BlobClient == null)
            {
                AzureBlobConfiguration = new AzureBlobConfiguration();
                var connectionString = AzureBlobConfiguration.StorageConnectionStringSettings;
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                BlobClient = storageAccount.CreateCloudBlobClient();

                BlobClient.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 5);
            }

            var containerReference = AzureBlobConfiguration.StorageContainerReference;
            var container = BlobClient.GetContainerReference(containerReference);
            if (container == null) throw new Exception("Null ContainerReference");

            var blockBlob = container.GetBlockBlobReference(name);

            blockBlob.UploadText(content);
            return true;
        }
    }
}