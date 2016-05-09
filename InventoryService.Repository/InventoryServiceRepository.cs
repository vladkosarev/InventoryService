using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace InventoryService.Repository
{
	public interface IInventoryServiceRepository 
	{
		Task<int> ReadQuantity(string productId);
		Task<int> ReadReservations(string productId);
		Task<Tuple<int,int>> ReadQuantityAndReservations(string productId);
		Task<bool> WriteQuantity(string productId, int Quantity);
		Task<bool> WriteReservations(string productId, int ReservationQuantity);
		Task<bool> WriteQuantityAndReservations(string productId, int Quantity, int ReservationQuantity);
	}

	public class InMemoryInventoryServiceRepository : IInventoryServiceRepository
	{
		private ConcurrentDictionary<string, Tuple<int,int>> productInventories = 
			new ConcurrentDictionary<string, Tuple<int,int>> ();

		public async Task<int> ReadQuantity(string productId)
		{
			if (!productInventories.ContainsKey(productId))
				throw new InvalidOperationException ();
			return productInventories[productId].Item1;
		}

		public async Task<int> ReadReservations(string productId)
		{
			if (!productInventories.ContainsKey(productId))
				throw new InvalidOperationException();
			return productInventories[productId].Item2;
		}
				
		public async Task<Tuple<int,int>> ReadQuantityAndReservations(string productId)
		{
			if (!productInventories.ContainsKey(productId))
				throw new InvalidOperationException();
			return productInventories[productId];
		}
				
		public async Task<bool> WriteQuantity(string productId, int quantity)
		{
			if (!productInventories.ContainsKey(productId))
				productInventories.TryAdd(productId, new Tuple<int,int>(quantity, 0));
			else		
				productInventories[productId] = new Tuple<int,int>(quantity, productInventories[productId].Item2);
			return true;
		}
				
		public async Task<bool> WriteReservations(string productId, int reservationQuantity)
		{
			if (!productInventories.ContainsKey(productId))
				productInventories.TryAdd(productId, new Tuple<int,int>(0, reservationQuantity));
			else		
				productInventories[productId] = new Tuple<int,int>(productInventories[productId].Item1, reservationQuantity);
			return true;
		}
				
		public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
		{
			if (!productInventories.ContainsKey(productId))
				productInventories.TryAdd(productId, new Tuple<int,int>(quantity, reservationQuantity));
			else		
				productInventories[productId] = new Tuple<int,int>(quantity, reservationQuantity);
			return true;
		}
	}
}

