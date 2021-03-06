﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic; // For InputBox

namespace StoreUI
{
    public partial class PopupOrder : Form
    {
        string SQL = "";
        List<OleDbParameter> sqlParameters = new List<OleDbParameter>();
        int numAffectedRows = 0;
        string OrderID = "";
        string CustomerID = ""; // Customer ID within this form

        //If editing a preexisting order, initialize the popup the ID, otherwise assume that a new order is being added
        public PopupOrder(string ID)
        {
            InitializeComponent();

            ColumnHeader header1 = new ColumnHeader();
            header1.Text = "InventoryItemID";
            header1.Width = 100;
            header1.TextAlign = HorizontalAlignment.Center;
            lstvwOrderedProducts.Columns.Add(header1);
            ColumnHeader header2 = new ColumnHeader();
            header2.Text = "Supplier";
            header2.Width = 75;
            header2.TextAlign = HorizontalAlignment.Center;
            lstvwOrderedProducts.Columns.Add(header2);
            ColumnHeader header3 = new ColumnHeader();
            header3.Text = "Product";
            header3.Width = 75;
            header3.TextAlign = HorizontalAlignment.Center;
            lstvwOrderedProducts.Columns.Add(header3);
            ColumnHeader header4 = new ColumnHeader();
            header4.Text = "Price";
            header4.Width = 50;
            header4.TextAlign = HorizontalAlignment.Center;
            lstvwOrderedProducts.Columns.Add(header4);
            ColumnHeader header5 = new ColumnHeader();
            header5.Text = "Quantity";
            header5.Width = 70;
            header5.TextAlign = HorizontalAlignment.Center;
            lstvwOrderedProducts.Columns.Add(header5);

            if(ID == "")
            {
                btnAdd.Text = "Add Order";
                this.Text = "New Order";

                //Set the customer order box to all customers
                SQL = "SELECT CustomerID, LastName, FirstName FROM Customers";
                DataTable customersdt = DataAccess.Read(SQL, null);
                foreach(DataRow dr in customersdt.Rows)
                {
                    cmbbxCustomers.Items.Add(dr["CustomerID"].ToString() + " " + dr["FirstName"].ToString() + " " + dr["LastName"].ToString());
                }

                this.CustomerID = customersdt.Rows[0]["CustomerID"].ToString(); // Default: Set the customer ID to the first customer

                //Populate listview with store inventory
                SQL = "SELECT InventoryItemID, SupplierID, ProductID, QuantityInInventory FROM SupplierProducts";
                DataTable dt = DataAccess.Read(SQL, null);
                DataTable suppliernamedt = new DataTable();
                DataTable productnamedt = new DataTable();
                foreach(DataRow dr in dt.Rows)
                {
                    ListViewItem item = new ListViewItem(dr["InventoryItemID"].ToString());

                    //Retrieve Supplier Name
                    SQL = "SELECT SupplierName FROM Suppliers WHERE SupplierID=" + dr["SupplierID"].ToString();
                    suppliernamedt = DataAccess.Read(SQL, null);
                    item.SubItems.Add(suppliernamedt.Rows[0]["SupplierName"].ToString());

                    //Retrieve Product Name
                    SQL = "SELECT ProductName FROM Products WHERE ProductID=" + dr["ProductID"].ToString();
                    productnamedt = DataAccess.Read(SQL, null);
                    item.SubItems.Add(productnamedt.Rows[0]["ProductName"].ToString());

                    SQL = "SELECT Price FROM Products WHERE ProductID=" + dr["ProductID"].ToString();
                    DataTable pricedt = DataAccess.Read(SQL, null);
                    if (pricedt != null)
                        item.SubItems.Add(pricedt.Rows[0]["Price"].ToString());
                    else
                        item.SubItems.Add("0.00");
                    item.SubItems.Add("0/" + dr["QuantityInInventory"].ToString());
                    item.SubItems.Add("0");
                    item.Checked = false;
                    lstvwOrderedProducts.Items.Add(item);
                }

                cmbbxCustomers.SelectedIndex = 0;
                cmbbxShippingPref.SelectedIndex = 0;
            }
            else
            {
                btnAdd.Text = "Edit Order";
                this.Text = "Edit Order";
                this.OrderID = ID;
                
                // Set the customer combobox to the customer whose order is being edited
                SQL = "SELECT CustomerID, ShippingCost, ShippingPreference FROM OrderInvoice WHERE OrderID=" + OrderID;
                DataTable ordercustomersdt = DataAccess.Read(SQL, null);
                SQL = "SELECT LastName, FirstName FROM Customers WHERE CustomerID=" + ordercustomersdt.Rows[0]["CustomerID"];
                DataTable customersdt = DataAccess.Read(SQL, null);
                cmbbxCustomers.Items.Add(ordercustomersdt.Rows[0]["CustomerID"].ToString() + " " + customersdt.Rows[0]["FirstName"].ToString() + " " + customersdt.Rows[0]["LastName"].ToString());
                cmbbxCustomers.SelectedIndex = 0;
                cmbbxShippingPref.SelectedIndex = cmbbxShippingPref.FindStringExact(ordercustomersdt.Rows[0]["ShippingPreference"].ToString());

                this.CustomerID = ordercustomersdt.Rows[0]["CustomerID"].ToString(); // Default: Set the customer ID to the first customer
                txtbxShippingCost.Text = ordercustomersdt.Rows[0]["ShippingCost"].ToString();

                SQL = "SELECT InventoryItemID, Quantity FROM OrderProduct WHERE OrderID=" + ID;
                DataTable dt = DataAccess.Read(SQL, null);
                DataTable suppliernamedt = new DataTable();
                DataTable productnamedt = new DataTable();
                foreach (DataRow dr in dt.Rows)
                {
                    ListViewItem item = new ListViewItem(dr["InventoryItemID"].ToString());

                    SQL = "SELECT SupplierID, ProductID, QuantityInInventory FROM SupplierProducts WHERE InventoryItemID="
                        + dr["InventoryItemID"].ToString();
                    DataTable productdt = DataAccess.Read(SQL, null);

                    //Retrieve Supplier Name
                    SQL = "SELECT SupplierName FROM Suppliers WHERE SupplierID=" + productdt.Rows[0]["SupplierID"].ToString();
                    suppliernamedt = DataAccess.Read(SQL, null);
                    item.SubItems.Add(suppliernamedt.Rows[0]["SupplierName"].ToString());

                    //Retrieve Product Name
                    SQL = "SELECT ProductName FROM Products WHERE ProductID=" + productdt.Rows[0]["ProductID"].ToString();
                    productnamedt = DataAccess.Read(SQL, null);
                    item.SubItems.Add(productnamedt.Rows[0]["ProductName"].ToString());

                    SQL = "SELECT Price FROM Products WHERE ProductID=" + productdt.Rows[0]["ProductID"].ToString();
                    DataTable pricedt = DataAccess.Read(SQL, null);
                    if (pricedt != null)
                        item.SubItems.Add(pricedt.Rows[0]["Price"].ToString());
                    else
                        item.SubItems.Add("0.00");

                    item.SubItems.Add(productdt.Rows[0]["QuantityInInventory"].ToString());
                    item.SubItems.Add(dr["Quantity"].ToString());
                    //item.Checked = true;
                    lstvwOrderedProducts.Items.Add(item);                    
                }
            }
        }

        //If the user selects a row, check its checkbox (default: only directly clicking the box will check it)
        private void lstvwOrderedProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstvwOrderedProducts.SelectedItems.Count > 0)
            {
                foreach(ListViewItem item in lstvwOrderedProducts.SelectedItems)
                {
                    item.Checked = true;
                    string q = Interaction.InputBox("Enter Product Quantity: ", "Order Product Quantity");
                    item.SubItems[4].Text = q;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(btnAdd.Text == "Add Order")
            {
                // Add an order invoice
                SQL = "INSERT INTO OrderInvoice (CustomerID, DateOrdered, ShippingPreference, ShippingCost, TrackingID, Completed) "
                    + "VALUES (@customerid, @dateordered, @shippingpref, @shippingcost, @trackingid, @completed)";
                sqlParameters.Clear();
                sqlParameters.Add(new OleDbParameter("@customerid", this.CustomerID)); // cmbbxCustomers.Text.Split(' ')[0]
                sqlParameters.Add(new OleDbParameter("@dateordered", DateTime.Now.ToOADate())); // USE SPECIAL OLE DB OBJECT FOR DATETIME
                if (cmbbxShippingPref.Text == "")
                    sqlParameters.Add(new OleDbParameter("@shippingpref", "Standard (2 - 8 Days)"));
                else
                    sqlParameters.Add(new OleDbParameter("@shippingpref", cmbbxShippingPref.Text));
                if (txtbxShippingCost.Text == "")
                    sqlParameters.Add(new OleDbParameter("@shippingcost", "0.00"));
                else
                    sqlParameters.Add(new OleDbParameter("@shippingcost", Double.Parse(txtbxShippingCost.Text)));
                sqlParameters.Add(new OleDbParameter("@trackingid", 1234)); // What should the tracking ID be??
                sqlParameters.Add(new OleDbParameter("@completed", false));

                numAffectedRows = DataAccess.Create(SQL, sqlParameters);
                if (numAffectedRows > 0)
                {
                    // Retrieve order invoice key
                    SQL = "SELECT LAST(OrderID) AS rk FROM OrderInvoice";
                    DataTable keydt = DataAccess.Read(SQL, null);
                    this.OrderID = keydt.Rows[0]["rk"].ToString();

                    // Use order invoice key to add ordered products
                    foreach(ListViewItem lvi in lstvwOrderedProducts.Items)
                    {
                        if(lvi.Checked == true)
                        {
                            SQL = "INSERT INTO OrderProduct (OrderID, InventoryItemID, Quantity) VALUES (@orderid, @inventoryitemid, @quantity)";
                            sqlParameters.Clear();
                            sqlParameters.Add(new OleDbParameter("@orderid", this.OrderID));
                            sqlParameters.Add(new OleDbParameter("@inventoryitemid", lvi.SubItems[0].Text));
                            if (lvi.SubItems[4].Text == "" || lvi.SubItems[4].Text.Split('/')[0] == "" || lvi.SubItems[4].Text.Split('/')[0] == "0")
                                sqlParameters.Add(new OleDbParameter("@quantity", 0));
                            else
                                sqlParameters.Add(new OleDbParameter("@quantity", lvi.SubItems[4].Text.Split('/')[0]));

                            numAffectedRows = DataAccess.Create(SQL, sqlParameters);
                            if (numAffectedRows < 1)
                            {
                                MessageBox.Show("An error occured. " + "" + " was not added to the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }

                        }
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show("An error occured. This order was not added to the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else //Edit
            {
                // Edit an order invoice
                SQL = "UPDATE OrderInvoice SET CustomerID=@customerid, ShippingPreference=@shippingpref, ShippingCost=@shippingcost, "
                    + "Completed=@completed WHERE OrderID=@orderid";
                sqlParameters.Clear();
                sqlParameters.Add(new OleDbParameter("@customerid", this.CustomerID)); // cmbbxCustomers.Text.Split(' ')[0]
                //sqlParameters.Add(new OleDbParameter("@dateordered", DateTime.Now.ToOADate())); // CANNOT EDIT DATE ORDERED?
                if (cmbbxShippingPref.Text == "")
                    sqlParameters.Add(new OleDbParameter("@shippingpref", "Standard (2 - 8 Days)"));
                else
                    sqlParameters.Add(new OleDbParameter("@shippingpref", cmbbxShippingPref.Text));
                if (txtbxShippingCost.Text == "")
                    sqlParameters.Add(new OleDbParameter("@shippingcost", "0.00"));
                else
                    sqlParameters.Add(new OleDbParameter("@shippingcost", txtbxShippingCost.Text));
                //sqlParameters.Add(new OleDbParameter("@trackingid", 0)); // CANNOT EDIT TRACKING ID?
                sqlParameters.Add(new OleDbParameter("@completed", false));
                sqlParameters.Add(new OleDbParameter("@orderid", this.OrderID));

                numAffectedRows = DataAccess.Update(SQL, sqlParameters);
                if (numAffectedRows > 0)
                {
                    foreach (ListViewItem lvi in lstvwOrderedProducts.Items)
                    {
                        if (lvi.Checked == true)
                        {
                            SQL = "UPDATE OrderProduct SET Quantity=@quantity WHERE OrderID=@orderid AND InventoryItemID=@inventoryitemid";
                            sqlParameters.Clear();
                            if (lvi.SubItems[4].Text == "")
                                sqlParameters.Add(new OleDbParameter("@quantity", 0));
                            else
                                sqlParameters.Add(new OleDbParameter("@quantity", lvi.SubItems[4].Text));
                            sqlParameters.Add(new OleDbParameter("@orderid", this.OrderID));
                            sqlParameters.Add(new OleDbParameter("@inventoryitemid", lvi.SubItems[0].Text));
                            
                            numAffectedRows = DataAccess.Update(SQL, sqlParameters);
                            if (numAffectedRows < 1)
                            {
                                MessageBox.Show("An error occured. " + "" + " was not edited in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }

                        }
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmbbxCustomers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbbxCustomers.Text != "") // Selected Count > 0 doesn't work for combobox, test this check
                this.CustomerID = cmbbxCustomers.Text.Split(' ')[0];
        }

        private void lstvwOrderedProducts_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            //Foreach checked item
            //Popup about quantity
            foreach (ListViewItem item in lstvwOrderedProducts.Items)
            {
                if (item.Checked == true)
                {
                    item.Checked = true;
                    string q = Interaction.InputBox("Enter Product Quantity for Order: ", "Order Product Quantity");
                    item.SubItems[4].Text = q + "/" + item.SubItems[4].Text.Split('/')[1];
                }
            }
        }
    }
}
