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
		public void Should_be_able_to_reserve_max(){
			Prop.ForAll<int>(i => Reserve(i, 0, i))
				.QuickCheckThrowOnFailure();
		}

		[Fact]
		public void Should_not_be_able_to_reserve_more_than_max() {
			Prop.ForAll(
				Arb.From<int> ()
				, Arb.From (Gen.Choose (1, int.MaxValue))
				, (total, overflow) => {
					return !Reserve (total, 0, total + overflow);
				})
				.QuickCheckThrowOnFailure();
		}

		public bool Reserve(int initialQuantity, int initialReservations, int reserveQuantity)
		{
			var productId = "product1";
			var inventoryService = new InMemoryInventoryServiceRepository();
			inventoryService.WriteQuantityAndReservations (productId, initialQuantity, initialReservations);

			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(1)).Result;

			return result.Successful;
		}
	}
}

