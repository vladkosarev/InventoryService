using System;
using Xunit;
using FsCheck;
using System.Linq;
using System.Collections.Generic;
using InventoryService.Repository;
using InventoryService;
using Akka.Actor;
using Akka.TestKit.Xunit2;

namespace InventoryService.Tests
{
	public class PropertyTests : TestKit
	{
		[Fact]
		public void Should_be_able_to_reserve_max_for_multiple_products() {
			Prop.ForAll<IList<Tuple<Guid, int, int>>>(l => {
				var initialInventory = 
					l.Select(i => new Tuple<string,int,int>(i.Item1.ToString(), i.Item2, i.Item3)).ToList();
				
				var repository = InitializeInventoryServiceRepository(initialInventory);

				foreach (var item in l) {
					Assert.True(Reserve(repository, item.Item2 - item.Item3, item.Item1.ToString()));
				}
			}
			).QuickCheckThrowOnFailure();
		}

		[Fact]
		public void Should_not_be_able_to_reserve_over_max_for_multiple_products() {
			Prop.ForAll<IList<Tuple<Guid, int, int, uint>>>(l => {
				var initialInventory = 
					l.Select(i => new Tuple<string,int,int>(i.Item1.ToString(), i.Item2, i.Item3)).ToList();

				var repository = InitializeInventoryServiceRepository(initialInventory);

				foreach (var item in l) {
					Assert.False(Reserve(repository, (int)(item.Item2 - item.Item3 + item.Item4 + 1), item.Item1.ToString()));
				}
			}
			).QuickCheckThrowOnFailure();
		}

		[Fact]
		public void Should_be_able_to_reserve_max() {
			Prop.ForAll<Guid, int>((productId, quantity) => {
				var repository = InitializeInventoryServiceRepository(
					new List<Tuple<string,int,int>>() {new Tuple<string,int,int>(productId.ToString(), quantity, 0)});
				Reserve(repository, quantity, productId.ToString());
			}
			).QuickCheckThrowOnFailure();
		}
			
		[Fact]
		public void Should_be_able_to_purchase_max(){
			Prop.ForAll<int>(i => Purchase(i, 0, i))
				.QuickCheckThrowOnFailure();
		}

		[Fact]
		public void Should_not_be_able_to_reserve_more_than_max() {
			Prop.ForAll(
				Arb.From<int> ()
				, Arb.From (Gen.Choose (1, int.MaxValue))
				, (total, overflow) => {
					var repository = InitializeInventoryServiceRepository(
						new List<Tuple<string,int,int>>() {new Tuple<string,int,int>("product1", total, 0)});
					return !Reserve (repository, total + overflow);
				})
				.QuickCheckThrowOnFailure();
		}

		[Fact]
		public void Should_not_be_able_to_purchase_more_than_max() {
			Prop.ForAll(
				Arb.From<int> ()
				, Arb.From (Gen.Choose (1, int.MaxValue))
				, (total, overflow) => {
					return !Purchase (total, 0, total + overflow);
				})
				.QuickCheckThrowOnFailure();
		}

		public IInventoryServiceRepository InitializeInventoryServiceRepository(IList<Tuple<string,int,int>> productInventory)
		{
			var inventoryService = new InMemoryInventoryServiceRepository();

			foreach (var product in productInventory) {
				inventoryService.WriteQuantityAndReservations (product.Item1, product.Item2, product.Item3);
			}

			return inventoryService;
		}

		public bool Reserve(IInventoryServiceRepository repository, int reserveQuantity, string productId = "product1")
		{
			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(repository)));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(1)).Result;

			return result.Successful;
		}

		public bool Purchase(int initialQuantity, int initialReservations, int purchaseQuantity, string productId = "product1")
		{
			var inventoryService = new InMemoryInventoryServiceRepository();
			inventoryService.WriteQuantityAndReservations (productId, initialQuantity, initialReservations);

			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var result = inventoryActor.Ask<PurchasedMessage>(new PurchaseMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(1)).Result;

			return result.Successful;
		}
	}
}

