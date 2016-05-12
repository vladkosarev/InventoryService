using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace InventoryService.Repository
{
    public class AzureTableRepository : IInventoryServiceRepository
    {
        private class Quantity : TableEntity
        {
            public Quantity(string productId)
            {
                this.PartitionKey = productId;
                this.RowKey = "quantity";
            }

            public Quantity()
            {
            }

            public int Value { get; set; }
        }

        private class Reserved : TableEntity
        {
            public Reserved(string productId)
            {
                this.PartitionKey = productId;
                this.RowKey = "reserved";
            }

            public Reserved()
            {
            }

            public int Value { get; set; }
        }

        private readonly CloudStorageAccount _storageAccount;
        private readonly string _tableName;

        public AzureTableRepository(string tableName = "products")
        {
            _tableName = tableName;
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        }

        public Task<int> ReadQuantity(string productId)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadReservations(string productId)
        {
            throw new NotImplementedException();
        }

        public async Task<Tuple<int, int>> ReadQuantityAndReservations(string productId)
        {
            var tableClient = _storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(_tableName);
            var tableQuery = new TableQuery<Quantity>()
                .Where(TableQuery.GenerateFilterCondition(
                    "PartitionKey", QueryComparisons.Equal, productId));
            TableContinuationToken continuationToken = null;
            tableQuery.TakeCount = 2;
            var queryResult =
                await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);

            var quantity = queryResult.FirstOrDefault(x => x.RowKey == "quantity").Value;
            var reserved = queryResult.FirstOrDefault(x => x.RowKey == "reserved").Value;

            return new Tuple<int, int>(quantity, reserved);
        }

        public Task<bool> WriteQuantity(string productId, int quantity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
        {
            var tableClient = _storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(_tableName);

            //var batch = new TableBatchOperation();
            //batch.InsertOrReplace(new Quantity(productId) { Value = quantity });
            //batch.InsertOrReplace(new Reserved(productId) { Value = reservationQuantity });

            //var result = await table.ExecuteBatchAsync(batch);
            await Task.WhenAll(
                table.ExecuteAsync(TableOperation.InsertOrReplace(new Quantity(productId) {Value = quantity}))
                ,
                table.ExecuteAsync(TableOperation.InsertOrReplace(new Reserved(productId) {Value = reservationQuantity}))
                );

            return true;
        }

        public Task Flush()
        {
            throw new NotImplementedException();
        }

        public async Task<Dictionary<string, int>> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
