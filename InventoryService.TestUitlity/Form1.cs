using Akka.Actor;
using InventoryService.Messages.Models;
using InventoryService.Messages.Response;
using InventoryService.Storage.InMemoryLib;
using InventoryService.Tests;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventoryService.TestUitlity
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            try
            {
                TestHelper helper = new TestHelper(new InMemory());

         
                var productName = "productName";
                //var inventoryActor = helper.InitializeAndGetInventoryActor(new RealTimeInventory(
                //        productName,
                //        Convert.ToInt32(InitialQuantity.Text),
                //        Convert.ToInt32(InitialReservation.Text),
                //        Convert.ToInt32(InitialHold.Text)), ActorSystem);
                var resmoteAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];


             var t=   helper.Reserve(ActorSystem.ActorSelection(resmoteAddress), 1); 

                var task = ActorSystem.ActorSelection(resmoteAddress).ResolveOne(TimeSpan.FromSeconds(5));
                task.ConfigureAwait(false);
                Task.WaitAll(task);
                var inventoryActor = task.Result;

                IInventoryServiceCompletedMessage result = null;
                var newUpdate = Convert.ToInt32(NewQuantity.Text);
                switch (cmbOoperation.SelectedItem.ToString())
                {
                    case "ReadInventory":
                        result = helper.GetInventory(inventoryActor, productName).WaitAndGetOperationResult();
                        break;

                    case "Reserve":
                        result = helper.Reserve(inventoryActor, newUpdate, productName).WaitAndGetOperationResult();
                        break;

                    case "UpdateQuantity":
                        result = helper.UpdateQuantity(inventoryActor, newUpdate, productName).WaitAndGetOperationResult();
                        break;

                    case "UpdateQuantityAndHold":
                        result = helper.UpdateQuantityAndHold(inventoryActor, newUpdate, productName).WaitAndGetOperationResult();
                        break;

                    case "PlaceHold":
                        result = helper.Hold(inventoryActor, newUpdate, productName).WaitAndGetOperationResult();
                        break;

                    case "Purchase":
                        result = helper.Purchase(inventoryActor, newUpdate, productName).WaitAndGetOperationResult();
                        break;

                    case "PurchaseFromHolds":
                        result = helper.PurchaseFromHolds(inventoryActor, newUpdate, productName).WaitAndGetOperationResult();
                        break;
                }

                if (result != null)
                {
                    ResultQuantity.Text = result.RealTimeInventory.Quantity.ToString();
                    ResultHold.Text = result.RealTimeInventory.Holds.ToString();
                    ResultReservation.Text = result.RealTimeInventory.Reserved.ToString();

                    if (!result.Successful)
                    {
                        var errorMessage = result as InventoryOperationErrorMessage;
                        var list = new List<string>();
                        var aggregateException = errorMessage?.Error;
                        if (aggregateException != null)
                            list.Add(aggregateException.Message);
                        richTextBox1.Text = errorMessage?.Error?.Message + " - " + string.Join(" ", list);
                    }
                    else
                    {
                        richTextBox1.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                richTextBox1.Text = ex.Message + ex?.InnerException?.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbOoperation.SelectedIndex = 1;
            ActorSystem = ActorSystem.Create("InventoryService-Client");
            button1.PerformClick();
        }

        public static ActorSystem ActorSystem { get; set; }

        private void InitialQuantity_TextChanged(object sender, EventArgs e)
        {
            NewQuantity.Text = InitialQuantity.Text;
        }

        private void InitialReservation_TextChanged(object sender, EventArgs e)
        {
        }

        private void InitialHold_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var quantity = new Random().Next(5, 25);
            var reservation = new Random().Next(5, quantity);
            var hold = new Random().Next(0, quantity);

            InitialQuantity.Text = quantity.ToString();
            InitialReservation.Text = reservation.ToString();
            InitialHold.Text = hold.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var quantity = new Random().Next(5, 25);

            NewQuantity.Text = quantity.ToString();
        }
    }
}