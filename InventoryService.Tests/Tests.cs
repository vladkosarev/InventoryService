using System;
using Akka.Actor;
using Xunit;
using Akka.TestKit.Xunit2;
using InventoryService;
using InventoryService.Repository;

namespace InventoryService.Tests
{
	public class TestClass : TestKit
	{
		[Fact]
		public void Should_not_over_reserve()
		{
			var productId = "product1";
			var inventoryService = new InMemoryInventoryServiceRepository();
			inventoryService.WriteQuantityAndReservations (productId, 10, 0);

			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, 2), TimeSpan.FromSeconds(1)).Result;

			Assert.True(result.Successful);

			result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, 9), TimeSpan.FromSeconds(1)).Result;

			Assert.False(result.Successful);
		}

		[Fact]
		public void Should_be_able_to_rserve_all()
		{
			var productId = "product1";
			var inventoryService = new InMemoryInventoryServiceRepository();
			inventoryService.WriteQuantityAndReservations (productId, 10, 0);

			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, 10), TimeSpan.FromSeconds(1)).Result;

			Assert.True(result.Successful);
		}

		[Fact]
		public void Should_be_able_to_rserve_multiple_products()
		{
			var inventoryService = new InMemoryInventoryServiceRepository();

			var productId1 = "product1";
			inventoryService.WriteQuantityAndReservations (productId1, 10, 0);
			var productId2 = "product2";
			inventoryService.WriteQuantityAndReservations (productId2, 10, 0);

			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId1, 8), TimeSpan.FromSeconds(1)).Result;
			Assert.True(result.Successful);

			result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId2, 6), TimeSpan.FromSeconds(1)).Result;
			Assert.True(result.Successful);
		}
	}
}

