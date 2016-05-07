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
		public void Identity_actor_should_confirm_user_creation_success()
		{
			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor()));

			var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage("ticketsection1", 2)).Result;

			Assert.True(result.Successful);

			result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage("ticketsection2", 9)).Result;

			Assert.True(result.Successful);
		}
	}
}

