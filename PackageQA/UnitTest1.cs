using Akka.Actor;
using Akka.Configuration;
using InventoryService.AkkaInMemoryServer;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Storage.InMemoryLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace PackageQA
{
    [TestClass]
    public class UnitTest1
    {
      
        [TestMethod]
        public void TestMethod2()
        {
            var serverOptions = new InventoryServerOptions()
            {
                InventoryActorAddress = "akka.tcp://InventoryService-Server@localhost:10000/user/InventoryActor",
                ServerEndPoint = "http://*:10088/",
                StorageType = typeof(InMemory),
                ServerActorSystemName = "InventoryService-Server",
                ServerActorSystemConfig = @"
                  akka.actor{provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""}
                  akka.remote.helios.tcp {
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 10000
                      transport-protocol = tcp
                      hostname = ""localhost""
                  }
              "
            };
            var InventoryserviceServer = new InventoryServiceServer(serverOptions);

            var res = InventoryserviceServer.inventoryActor.Path;
            var res2 = InventoryserviceServer.inventoryActor.Ask(new GetInventoryMessage("sample")).Result;
        }

        [TestMethod]
        public void TestMethod1()
        {
            var initialInventory = new RealTimeInventory("ticketsections-100", 10, 0, 0);
            var serverOptions = new InventoryServerOptions()
            {
                InitialInventory = initialInventory,
                InventoryActorAddress = "akka.tcp://InventoryService-Server@localhost:10000/user/InventoryActor",
                ServerEndPoint = "http://*:10088/",
                StorageType = typeof(InMemory),
                ServerActorSystemName = "InventoryService-Server",
                ServerActorSystemConfig = @"
                  akka.actor{provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""}
                  akka.remote.helios.tcp {
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 10000
                      transport-protocol = tcp
                      hostname = ""localhost""
                  }
              "
            };

            using (var server = new InventoryServiceServer(serverOptions))
            {
                var mySystem = Akka.Actor.ActorSystem.Create("mySystem", ConfigurationFactory.ParseString(@"
                  akka.actor{provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""}
                  akka.remote.helios.tcp {
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 0
                      transport-protocol = tcp
                      hostname = ""localhost""
                  }
              "));
                var inventoryActor = mySystem.ActorSelection(serverOptions.InventoryActorAddress);

                var result =
                 server.inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(
                        initialInventory.ProductId, 20));

                result.ConfigureAwait(false);

                Task.WaitAll(result);

                if (result.Result.Successful)
                {
                  Console.WriteLine(result.Result.RealTimeInventory);
                }
                else
                {
                  Console.WriteLine(result.Result.RealTimeInventory);
                }
            }
        }
    }
}