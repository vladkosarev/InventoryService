//using Akka.Actor;
//using Akka.Event;
//using InventoryService.Messages;
//using InventoryService.Messages.Request;
//using InventoryService.WebUIHost;

//namespace InventoryService.Actors
//{
//    public class SignalRNotificationsActor : ReceiveActor
//    {
//        public readonly ILoggingAdapter Logger = Context.GetLogger();
//        private SignalRNotificationService SignalRNotificationService { set; get; }

//        public SignalRNotificationsActor()
//        {
//            SignalRNotificationService = new SignalRNotificationService();
//            Receive<GetMetricsCompletedMessage>(message =>
//            {
//                SignalRNotificationService.SendMessageSpeed(message.MessageSpeedPersecond);
//            });

//            Receive<QueryInventoryListCompletedMessage>(message =>
//            {
//                SignalRNotificationService.SendInventoryList(message);
//                Logger.Debug("total inventories in inventory service : " + message?.RealTimeInventories?.Count);
//            });

//            Receive<IRequestMessage>(message =>
//            {
//                SignalRNotificationService.SendIncomingMessage(message.GetType().Name + " : " + message.Update + " for " + message.ProductId);
//                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message);
//            });
//            Receive<ServerNotificationMessage>(message =>
//            {
//                SignalRNotificationService.SendServerNotification(message.ServerMessage);
//            });
//        }
//    }
//}