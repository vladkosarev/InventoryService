using System;
using Akka.Actor;
using Xunit;
using Akka.TestKit.Xunit2;
using InventoryService;

namespace InventoryService.Tests
{
	public class TestClass : TestKit
	{
		[Fact]
		public void Should_not_over_reserve()
		{
			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor()));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage("product1", 2)).Result;

			Assert.True(result.Successful);

			result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage("product1", 9)).Result;

			Assert.False(result.Successful);
		}
	}
}

