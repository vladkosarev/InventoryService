using System;
using System.Collections.Generic;
using Akka.Actor;

namespace InventoryService
{
	public class InventoryActor : ReceiveActor
	{
		private Dictionary<string, IActorRef> ticketSections = new Dictionary<string, IActorRef>();

		public InventoryActor()
		{
			Receive<ReserveMessage>(message =>
				{
					var exists = ticketSections.ContainsKey(message.TicketSection);
					if (!exists) {
						var ticketSectionActorRef = Context.ActorOf(Props.Create(() => new InventorySectionActor(message.TicketSection)), message.TicketSection);
						ticketSections.Add(message.TicketSection, ticketSectionActorRef);
					}
					ticketSections[message.TicketSection].Forward(message);
				});
		}
	}
}

