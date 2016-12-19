using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryService.BackUpService;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using CloudBlobClient = Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient;
using CloudBlobContainer = Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer;
using CloudBlockBlob = Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob;
using CloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount;

namespace InventoryService.AzureBlobBackUpService
{
    public class AzureBlobBackUp:IBackUpService
    {
        private CloudBlobClient BlobClient { get; set; }
        private List<string> BlobNames { get; set; }
        private AzureBlobConfiguration AzureBlobConfiguration { get; set; }

        public AzureBlobBackUp()
        {
            AzureBlobConfiguration = new AzureBlobConfiguration();
            var connectionString = /*connectionString ??*/ AzureBlobConfiguration.StorageConnectionStringSettings;
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            BlobClient = storageAccount.CreateCloudBlobClient();

            BlobClient.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 5);
        }
        public bool BackUp(string name, string content)
        {
            var container = GetCloudBlobContainer(AzureBlobConfiguration.StorageContainerReference);
            //todo write 
            throw new NotImplementedException();
        }
        public bool Exists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }
            var fileNameOnly = Path.GetFileName(fileName.ToLower());
            var actualBlobFileName = GetActualBlobFileName(fileNameOnly);
            return !string.IsNullOrEmpty(actualBlobFileName) && GetCloudBlobContainer(AzureBlobConfiguration.StorageContainerReference).GetBlockBlobReference(actualBlobFileName).Exists();
        }
        #region privates

        private string GetActualBlobFileName(string fileName)
        {
            BlobNames = GetCloudBlobContainer(AzureBlobConfiguration.StorageContainerReference).ListBlobs().Select(b =>
            {
                var cloudBlockBlob = b as CloudBlockBlob;
                return cloudBlockBlob != null ? cloudBlockBlob.Name : null;
            }).ToList();

            return BlobNames.Count <= 0 ? null : BlobNames.FindAll(x => String.Equals(x, fileName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
        }

        private CloudBlobContainer GetCloudBlobContainer(string containerReference)
        {
            var container = BlobClient.GetContainerReference(containerReference);
            if (container == null) throw new Exception("Null ContainerReference");

            if (!container.Exists()) throw new Exception("Blob Container does not exist : " + containerReference);
            return container;
        }

        #endregion privates
    }
}
