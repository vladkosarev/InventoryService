using System;
using System.Configuration;

namespace InventoryService.AzureBlobBackUpService
{
    public class AzureBlobConfiguration
    {
       public AzureBlobConfiguration(string storageBlobContainer = null)
        {
            StorageContainerReference = string.IsNullOrWhiteSpace(storageBlobContainer) ? GetAppSettingsValueByName("AzureStorageContainerReference") : storageBlobContainer;
            StorageConnectionStringSettings = GetAppSettingsValueByName("AzureStorageConnectionString");
        }

        public static string GetAppSettingsValueByName(string appSettingsName)
        {
            var containerReference = ConfigurationManager.AppSettings[appSettingsName];

            if (string.IsNullOrWhiteSpace(containerReference))
            {
                throw new Exception(appSettingsName + " for azure blob required  not found in appSettings");
            }
            return containerReference;
        }

        public string StorageConnectionStringSettings { get; set; }

        public string StorageContainerReference { get; set; }
    }
}