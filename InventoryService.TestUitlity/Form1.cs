using Akka.Actor;
using InventoryService.AkkaInMemoryServer;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Response;
using InventoryService.Tests;
using System;
using System.Collections.Generic;
using System.Configuration;
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
                var productName = "productName";
                var inventory = new RealTimeInventory(
                    productName,
                    Convert.ToInt32(InitialQuantity.Text),
                    Convert.ToInt32(InitialReservation.Text),
                    Convert.ToInt32(InitialHold.Text));
                InventoryServiceServer helper = new InventoryServiceServer(new TestPerformanceService(), new InventoryServerOptions()
                {
                    StorageType = typeof(Storage.InMemoryLib.InMemory),
                    InitialInventory = inventory,
                    ClientActorSystem = ActorSystem
                });

                var t = helper.ReserveAsync(ActorSystem.ActorSelection(textBox1.Text), 1);

                var task = ActorSystem.ActorSelection(textBox1.Text).ResolveOne(TimeSpan.FromSeconds(5));
                // task.ConfigureAwait(false);

                task.ContinueWith(r =>
                {
                    IInventoryServiceCompletedMessage result = null;
                    var newUpdate = Convert.ToInt32(NewQuantity.Text);
                    switch (cmbOoperation.SelectedItem.ToString())
                    {
                        case "ReadInventory":
                            result = helper.GetInventoryAsync(productName).WaitAndGetOperationResult();
                            break;

                        case "Reserve":
                            result = helper.ReserveAsync(inventory, newUpdate).WaitAndGetOperationResult();
                            break;

                        case "UpdateQuantity":
                            result = helper.UpdateQuantityAsync(inventory, newUpdate).WaitAndGetOperationResult();
                            break;

                        case "UpdateQuantityAndHold":
                            result = helper.UpdateQuantityAndHoldAsync(inventory, newUpdate).WaitAndGetOperationResult();
                            break;

                        case "PlaceHold":
                            result = helper.PlaceHoldAsync(inventory, newUpdate).WaitAndGetOperationResult();
                            break;

                        case "Purchase":
                            result = helper.PurchaseAsync(inventory, newUpdate).WaitAndGetOperationResult();
                            break;

                        case "PurchaseFromHolds":
                            result = helper.PurchaseFromHoldsAsync(inventory, newUpdate).WaitAndGetOperationResult();
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
                                list.Add(aggregateException.ErrorMessage);
                            richTextBox1.Text = errorMessage?.Error?.ErrorMessage + " - " + string.Join(" ", list);
                        }
                        else
                        {
                            richTextBox1.Text = "";
                        }
                    }
                });
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
            textBox1.Text = ConfigurationManager.AppSettings["RemoteInventoryActorAddress"];

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