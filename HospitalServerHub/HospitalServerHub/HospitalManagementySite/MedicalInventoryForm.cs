using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HospitalManagementySite
{
    public partial class MedicalInventoryForm : Form
    {
        private HospitalManagementDBEntities1 context;
        private HubConnection connection;
        const int LOW_SUPPLY_TRIGGER = 100;
        const int RESTOCK_QUANTITY = 500;
        public MedicalInventoryForm()
        {
            InitializeComponent();
            InitializeSignalR();
            context = new HospitalManagementDBEntities1();
        }

        private void buttonAddNewSupply_Click(object sender, EventArgs e)
        {
            string itemName = textBoxItemName.Text;
            string quantity = textBoxQuantity.Text;
            string supplierName = textBoxSupplierName.Text;
            string supplierPhone = textBoxSupplierPhone.Text;

            if (checkValidInventory(itemName, quantity, supplierName, supplierPhone)) 
            {
                MedicalInventory medicalInventory = new MedicalInventory();
                medicalInventory.ItemName = itemName;
                medicalInventory.Quantity = Int32.Parse(quantity);
                medicalInventory.SupplierName = supplierName;
                medicalInventory.SupplierPhone = supplierPhone;
                context.MedicalInventories.Add(medicalInventory);
                context.SaveChanges();
            }
            loadToDataGridView();
        }

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            using (var context = new HospitalManagementDBEntities1())
            {
                string itemName = textBoxItemName.Text;
                string quantity = textBoxQuantity.Text;
                string supplierName = textBoxSupplierName.Text;
                string supplierPhone = textBoxSupplierPhone.Text;

                var invToUpdate = context.MedicalInventories.FirstOrDefault(p => p.ItemName == itemName);
                if (invToUpdate != null)
                {
                    invToUpdate.ItemName = itemName;
                    invToUpdate.Quantity = Int32.Parse(quantity);
                    invToUpdate.SupplierName = supplierName;
                    invToUpdate.SupplierPhone = supplierPhone;

                    context.SaveChanges();                   
                    loadToDataGridView();
                    MessageBox.Show("Item updated successfully.");

                    if (invToUpdate.Quantity < LOW_SUPPLY_TRIGGER)
                    {
                        // use signalR 
                        try
                        {

                            Thread.Sleep(5000);
                            ChatBox.Items.Add("Hospital Management System detects low stock for " + itemName);
                            ChatBox.Items.Add("Request to refill quantity: " + RESTOCK_QUANTITY);
                            await connection.InvokeAsync("SendMessage", invToUpdate.SupplierName, invToUpdate.SupplierPhone, invToUpdate.ItemName, RESTOCK_QUANTITY);


                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }

                }
                else
                {
                    MessageBox.Show("Item not found.");
                }
            }
        }



        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int selectedId = Int32.Parse(dataGridView1.CurrentRow.Cells[0].Value.ToString());
                var deleteMedInv =
                        from medInv in context.MedicalInventories
                        where medInv.MedicalInventoryId == selectedId
                        select medInv;

                foreach (var inv in deleteMedInv)
                {
                    context.MedicalInventories.Remove(inv);
                }

                try
                {
                    context.SaveChanges();
                    loadToDataGridView();
                    MessageBox.Show("Delete sucessful");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                MessageBox.Show("Please select an item to be deleted from datagridview");
            }
            
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            loadToDataGridView();
        }

        private async void InitializeSignalR()
        {
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5041/inventory")
                .Build();

            // receive messages
            connection.On<string, string, string, int>("ReceiveMessage", (supplierName, supplierPhone, itemName, quantity) =>
            {
                //string responseMessage = supplierName + " received the order request for the item: " + itemName + " with quantity: " + quantity;
                Invoke((Action)(() =>
                {
                    string messageText = "Send refill from: " + $"{supplierName} {itemName}: {quantity}";

                    Thread.Sleep(3000);
                    restockInventory(itemName, quantity);
                    //ChatBox.Items.Add(responseMessage);
                    ChatBox.Items.Add(messageText);
                }));
            });

            try
            {
                await connection.StartAsync();
                MessageBox.Show("Connection to the hub was sucessful");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void buttonRefillSupply_Click(object sender, EventArgs e)
        {
            // sending messages
            string supplierName = textBoxSupplierName.Text;
            string supplierPhone = textBoxSupplierPhone.Text;
            string itemName = textBoxItemName.Text;
            string quantity = textBoxQuantity.Text;

            try
            {
                ChatBox.Items.Add("Hospital request refill " + itemName + ": " + quantity);
                await connection.InvokeAsync("SendMessage", supplierName, supplierPhone, itemName, Int32.Parse(quantity));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void restockInventory(string itemName, int quantity)
        {
            var query =
                from medInvent in context.MedicalInventories
                select medInvent;

            foreach (MedicalInventory inv in query)
            {
                if (inv.ItemName.Equals(itemName))
                {
                    inv.Quantity += quantity;
                }
            }


            try
            {
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            loadToDataGridView();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            textBoxItemName.Text = dataGridView1.CurrentRow.Cells[1].Value.ToString();
            textBoxQuantity.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
            textBoxSupplierName.Text = dataGridView1.CurrentRow.Cells[3].Value.ToString();
            textBoxSupplierPhone.Text = dataGridView1.CurrentRow.Cells[4].Value.ToString();
        }

        private void loadToDataGridView()
        {
            var query = from inv in context.MedicalInventories
                        select new
                        {
                            MedicalInventoryId = inv.MedicalInventoryId,
                            ItemName = inv.ItemName,
                            Quantity = inv.Quantity,
                            SupplierName = inv.SupplierName,
                            SupplierPhone = inv.SupplierPhone
                            // Add more fields as needed
                        };

            // Bind the result to DataGridView
            dataGridView1.DataSource = query.ToList();
        }

        private bool checkValidInventory(string itemName, string quantity, string supplierName, string supplierPhone)
        {
            bool result = true;

            if (itemName == "")
            {
                MessageBox.Show("Please enter item name.");
                return false;
            }
            else if (quantity == "")
            {
                MessageBox.Show("Please enter item quantity.");
                return false;
            }
            else if (supplierName == "")
            {
                MessageBox.Show("Please enter supplier name.");
                return false;
            }
            else if (supplierPhone == "")
            {
                MessageBox.Show("Please enter supplier phone.");
                return false;
            }
            return result;
        }
    }
}
