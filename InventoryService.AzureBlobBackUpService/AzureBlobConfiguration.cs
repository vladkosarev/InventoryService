using System;
using System.Configuration;

namespace InventoryService.AzureBlobBackUpService
{
    public class AzureBlobConfiguration
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="storageBlobContainer">if storageBlobContainer is not set, it will be retrieved from appSettings by the name 'StorageContainerReference'</param>
        public AzureBlobConfiguration(string storageBlobContainer = null)
        {
            StorageContainerReference = string.IsNullOrWhiteSpace(storageBlobContainer) ? GetAppSettingsValueByName("StorageContainerReference") : storageBlobContainer;
            StorageConnectionStringSettings = GetAppSettingsValueByName("StorageConnectionString");
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