using Akka.Actor;
using InventoryService.Messages.Models;
using InventoryService.Messages.Response;
using InventoryService.Storage.InMemoryLib;
using InventoryService.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
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

                bool initializationSuccess;
                var productName = "productName";
                var inventoryActor = helper.InitializeAndGetInventoryActor(new RealTimeInventory(
                        productName,
                        Convert.ToInt32(InitialQuantity.Text),
                        Convert.ToInt32(InitialReservation.Text),
                        Convert.ToInt32(InitialHold.Text)), ActorSystem);

                IInventoryServiceCompletedMessage result = null;
                var newUpdate = Convert.ToInt32(NewQuantity.Text);
                switch (cmbOoperation.SelectedItem.ToString())
                {
                    case "ReadInventory":
                        result = helper.GetInventory(inventoryActor, productName);
                        break;

                    case "Reserve":
                        result = helper.Reserve(inventoryActor, newUpdate, productName);
                        break;

                    case "UpdateQuantity":
                        result = helper.UpdateQuantity(inventoryActor, newUpdate, productName);
                        break;

                    case "UpdateQuantityAndHold":
                        result = helper.UpdateQuantityAndHold(inventoryActor, newUpdate, productName);
                        break;

                    case "PlaceHold":
                        result = helper.Hold(inventoryActor, newUpdate, productName);
                        break;

                    case "Purchase":
                        result = helper.Purchase(inventoryActor, newUpdate, productName);
                        break;

                    case "PurchaseFromHolds":
                        result = helper.PurchaseFromHolds(inventoryActor, newUpdate, productName);
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
                        var aggregateException = errorMessage?.Error?.Flatten();
                        if (aggregateException != null)
                            list.AddRange(from x in aggregateException?.InnerExceptions select x.Message);
                        richTextBox1.Text = errorMessage?.Error?.Flatten().Message + " - " + string.Join(" ", list);
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
            ActorSystem = ActorSystem.Create("Utility");
            button1.PerformClick();
        }

        public ActorSystem ActorSystem { get; set; }

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