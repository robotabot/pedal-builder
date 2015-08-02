using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace PedalBuilds
{
    public partial class Form1 : Form
    {
        static SQLiteConnection con = new SQLiteConnection(@"Data Source=.\Pedals.sqlite;Version=3");
        List<PedalPart> parts = new List<PedalPart>();
        List<Component> componentList = new List<Component>();
        List<PedalBuild> builds = new List<PedalBuild>(); 

        public Form1()
        {
            InitializeComponent();
        }

        //On load, create the database and fill the lists of pedals and components.
        private void Form1_Load(object sender, EventArgs e)
        {

            CreateDb.Create();

            FillPedalList();
            FillComponentList();
        }

        //Update the display of the selected pedals information.
        //Refresh the list of parts for this pedal.
        private void lstPedals_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                gBoxParts.Text = e.Item.SubItems[0].Text +" Partlist";
                txtPedalName.Text = e.Item.SubItems[0].Text;
                rTxtPedalNotes.Text = e.Item.SubItems[2].Text;
                FillPartList();
                btnDeletePedal.Enabled = true;
            }

        }

        //Create a new pedal and save to the database.
        //Refresh the list of pedals.
        private void btnNewPedal_Click(object sender, EventArgs e)
        {
            if (txtPedalName.TextLength != 0)
            {
                string notes = rTxtPedalNotes.TextLength > 0 ? rTxtPedalNotes.Text : "";

                try
                {
                    con.Open();

                    string addPedalSql = "INSERT INTO pedals (name, notes, builds) VALUES ('" + txtPedalName.Text +
                        "','" + notes +
                        "'," + 0 + ")";
                    using (SQLiteCommand cmd = new SQLiteCommand(addPedalSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                lstPedalComponents.Items.Clear();
                FillPedalList();
            }
        }

        //Retrieve the pedals from the database and display them.
        private void FillPedalList()
        {
            lstPedals.Items.Clear();

            try
            {
                con.Open();

                string getPedalsSql = "SELECT * FROM pedals ORDER BY name ASC";

                using (SQLiteCommand cmd = new SQLiteCommand(getPedalsSql, con))
                {
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string pedalName = reader["name"].ToString();
                        string pedalNotes = reader["notes"].ToString();
                        Int64 pedalBuilds = Convert.ToInt64(reader["builds"]);
                        Int64 pedalId = Convert.ToInt64(reader["id"]);

                        ListViewItem item = new ListViewItem(pedalName);
                        item.SubItems.Add(pedalBuilds.ToString());
                        item.SubItems.Add(pedalNotes);
                        item.Tag = pedalId;
                        lstPedals.Items.Add(item);
                    }
                }

                con.Close();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }

        //Retrieve the components from the database and display them.
        private void FillComponentList()
        {
            lstComponents.Items.Clear();

            try
            {

                con.Open();

                string getCmpSql = "SELECT * FROM components ORDER BY type, value ASC";

                using (SQLiteCommand cmd = new SQLiteCommand(getCmpSql, con))
                {
                    SQLiteDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(reader["type"].ToString());
                        item.SubItems.Add(reader["value"].ToString());
                        item.SubItems.Add(reader["notes"].ToString());
                        item.SubItems.Add(reader["url"].ToString());
                        item.SubItems.Add(reader["price"].ToString());
                        item.Tag = reader["id"];

                        lstComponents.Items.Add(item);
                    }
                }

                con.Close();

                gBoxComponents.Text = lstComponents.Items.Count + " Components";
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        //Show the selected component's information.
        private void lstComponents_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                FillComponentTextBoxes(e.Item);
                btnDeleteComponent.Enabled = true;
            }
        }

        private void FillComponentTextBoxes(ListViewItem item)
        {
            txtCmpType.Text = item.SubItems[0].Text;
            txtCmpValue.Text = item.SubItems[1].Text;
            txtCmpNotes.Text = item.SubItems[2].Text;
            txtCmpURL.Text = item.SubItems[3].Text;
            txtCmpPrice.Text = item.SubItems[4].Text;
        }

        //TODO retain item in view
        //TODO refresh the part list as well
        //Update a component in the database.
        //Refresh the list of components.
        private void btnUpdateComponent_Click(object sender, EventArgs e)
        {
            if (txtCmpValue.TextLength != 0 && txtCmpType.TextLength != 0 && lstComponents.SelectedItems.Count > 0)
            {
                string cmpNotes = txtCmpNotes.TextLength > 0 ? txtCmpNotes.Text : "";
                string cmpURL = txtCmpURL.TextLength > 0 ? txtCmpURL.Text : "";
                double result;
                bool isDouble = Double.TryParse(txtCmpPrice.Text, out result);
                double cmpPrice = isDouble ? result : 0.00;

                try
                {
                    con.Open();

                    string updateCmpSql = "UPDATE components SET type = '" +
                        txtCmpType.Text +
                        "', value = '" + txtCmpValue.Text +
                        "', notes = '" + cmpNotes +
                        "', url = '" + cmpURL +
                        "', price = " + cmpPrice +
                        " WHERE id = " + lstComponents.SelectedItems[0].Tag;

                    using (SQLiteCommand cmd = new SQLiteCommand(updateCmpSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                

                FillComponentList();
            }
        }

        //Insert a new component into the database.
        //Refresh the list of components.
        private void btnNewCmp_Click(object sender, EventArgs e)
        {
            


            if (txtCmpValue.TextLength != 0 && txtCmpType.TextLength != 0)
            {
                string cmpNotes = txtCmpNotes.TextLength > 0 ? txtCmpNotes.Text : "";
                string cmpURL = txtCmpURL.TextLength > 0 ? txtCmpURL.Text : "";
                double result;
                bool isDouble = Double.TryParse(txtCmpPrice.Text, out result);
                double cmpPrice = isDouble ? result : 0.00;

                try
                {
                    con.Open();

                    string addCmpSql = "INSERT INTO components (type, value, notes, url, price) VALUES ('" +
                        txtCmpType.Text +
                        "','" + txtCmpValue.Text +
                        "','" + cmpNotes +
                        "','" + cmpURL +
                        "'," + cmpPrice +
                        ")";
                    using (SQLiteCommand cmd = new SQLiteCommand(addCmpSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
    
                    con.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                FillComponentList();
                txtCmpType.Clear();
                txtCmpValue.Clear();
                txtCmpURL.Clear();
                txtCmpNotes.Clear();
                txtCmpPrice.Clear();
            }
        }

        //TODO retain item in view
        //Delete this component from the database.
        //Refresh the component list and part list.
        private void btnCmpDelete_Click(object sender, EventArgs e)
        {
            string selectPedalsSql = "SELECT DISTINCT name FROM pedals INNER JOIN partlist ON pedals.id = partlist.pedal_id AND partlist.component_id =" + lstComponents.SelectedItems[0].Tag;
            string deleteCmpSql = "DELETE FROM components WHERE id =" + lstComponents.SelectedItems[0].Tag;
            string deleteCmpPartlistSql = "DELETE FROM partlist WHERE component_id =" + lstComponents.SelectedItems[0].Tag;
            string pedals = "";
            DialogResult confirmComponentDelete;

            try
            {
                con.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(selectPedalsSql, con))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedals += reader["name"] + "\n";
                        }
                    }
                }

                if (String.IsNullOrEmpty(pedals))
                {
                    confirmComponentDelete = MessageBox.Show("This component is not used in any of your pedals." + pedals, "Delete Component", MessageBoxButtons.OKCancel);
                }
                else
                {
                    confirmComponentDelete = MessageBox.Show("You will delete this component from the following pedals:\n" + pedals, "Delete Component", MessageBoxButtons.OKCancel);
                }

                if (confirmComponentDelete == DialogResult.OK)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(deleteCmpSql, con))
                    {
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = deleteCmpPartlistSql;

                        cmd.ExecuteNonQuery();
                    }
                }
                

                con.Close();

                FillComponentList();
                FillPartList();

                txtCmpType.Clear();
                txtCmpValue.Clear();
                txtCmpNotes.Clear();
                txtCmpURL.Clear();
                txtCmpPrice.Clear();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }

        //Delete this pedal from the database.
        //Refresh the pedal and part lists.
        private void btnDeletePedal_Click(object sender, EventArgs e)
        {
            try
            {
                con.Open();

                string deletePdlSql = "DELETE FROM pedals WHERE id =" + lstPedals.SelectedItems[0].Tag;

                using (SQLiteCommand cmd = new SQLiteCommand(deletePdlSql, con))
                {
                    cmd.ExecuteNonQuery();
                }

                deletePdlSql = "DELETE FROM partlist WHERE pedal_id =" + lstPedals.SelectedItems[0].Tag;

                using (SQLiteCommand cmd = new SQLiteCommand(deletePdlSql, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();

                txtPedalName.Clear();
                rTxtPedalNotes.Clear();

                FillPedalList();
                lstPedalComponents.Items.Clear();
            }
            catch (SQLiteException ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        //Update this pedal in the database.
        //Refresh the list of pedals.
        private void btnPdlUpdate_Click(object sender, EventArgs e)
        {
            if (txtPedalName.TextLength != 0 && lstPedals.SelectedItems.Count > 0 && lstPedals.Items.Count > 0)
            {
                string notes = rTxtPedalNotes.TextLength > 0 ? rTxtPedalNotes.Text : "";

                try
                {
                    con.Open();

                    string updateCmpSql = "UPDATE components SET name = '" +
                        txtPedalName +
                        "', notes = '" + notes +
                        " WHERE id = " + lstPedals.SelectedItems[0].Tag;

                    using (SQLiteCommand cmd = new SQLiteCommand(updateCmpSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    con.Close();

                    txtPedalName.Clear();
                    rTxtPedalNotes.Clear();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                FillPedalList();
            }
        }

        //Add a component to the pedal.
        //Insert the component into the partlist table.
        //Refresh the part list.
        private void btnAddComponentToPedal_Click(object sender, EventArgs e)
        {
            if (txtAddComponentToPedalName.TextLength > 0 && lstPedals.SelectedItems.Count == 1 &&
                lstComponents.SelectedItems.Count == 1 && lstPedals.Items.Count > 0 && lstComponents.Items.Count > 0)
            {
                try
                {
                    con.Open();

                    string addCmpSql = "INSERT INTO partlist (partname, component_id, pedal_id) VALUES ('" +
                                       txtAddComponentToPedalName.Text +
                                       "'," + lstComponents.SelectedItems[0].Tag +
                                       "," + lstPedals.SelectedItems[0].Tag + ")";

                    using (SQLiteCommand cmd = new SQLiteCommand(addCmpSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }

                FillPartList();
            }
        }

        //Refreshes the list of parts.
        //Updates the total cost of the parts.
        private void FillPartList()
        {
            if (lstPedals.SelectedItems.Count > 0 && lstPedals.Items.Count > 0)
            {
                lstPedalComponents.Items.Clear();

                try
                {
                    con.Open();

                    string getPdlInfoSql = "SELECT * FROM partlist WHERE pedal_id =" + lstPedals.SelectedItems[0].Tag + " ORDER BY partname ASC";

                    using (SQLiteCommand cmd = new SQLiteCommand(getPdlInfoSql, con))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PedalPart part = new PedalPart();
                                part.id = Convert.ToInt64(reader["id"]);
                                part.Name = reader["partname"].ToString();
                                part.PedalId = Convert.ToInt64(reader["pedal_id"]);
                                part.ComponentId = Convert.ToInt64(reader["component_id"]);
                                parts.Add(part);
                            }
                        }

                        foreach (PedalPart pedalPart in parts)
                        {
                            string getCmpInfoSql = "SELECT type, value, price FROM components WHERE id =" + pedalPart.ComponentId;

                            cmd.CommandText = getCmpInfoSql;

                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    pedalPart.Type = reader["type"].ToString();
                                    pedalPart.Value = reader["value"].ToString();
                                    pedalPart.Price = Convert.ToDecimal(reader["price"]);
                                }
                            }
                        }
                    }

                    foreach (PedalPart pedalPart in parts)
                    {
                        ListViewItem item = new ListViewItem(pedalPart.Name);
                        item.SubItems.Add(pedalPart.Type);
                        item.SubItems.Add(pedalPart.Value);
                        item.SubItems.Add(pedalPart.PedalId.ToString());
                        item.SubItems.Add(pedalPart.ComponentId.ToString());
                        item.Tag = pedalPart.id;

                        lstPedalComponents.Items.Add(item);
                    }

                    con.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
                UpdatePedalCost(parts);
                parts.Clear();
            }
        }

        //Removes a component from a pedal.
        //Deletes the component from the partlist table.
        private void btnRemoveComponentFromPedal_Click(object sender, EventArgs e)
        {
            if (lstPedalComponents.SelectedItems[0] != null && lstPedalComponents.Items.Count > 0)
            {
                try
                {
                    con.Open();

                    string deleteCmpFrmPdlSql = "DELETE FROM partlist WHERE id =" + lstPedalComponents.SelectedItems[0].Tag;

                    using (SQLiteCommand cmd = new SQLiteCommand(deleteCmpFrmPdlSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
 
                    con.Close();
                }
                catch (SQLiteException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
                lstPedalComponents.Items.Clear();
                FillPartList();
            }
        }

        //Updates the pedals cost
        private void UpdatePedalCost(List<PedalPart> parts)
        {
            decimal pedalCost = new decimal(0.00);
            foreach (PedalPart pedalPart in parts)
            {
                pedalCost += pedalPart.Price;
            }

            pedalCost = decimal.Round(pedalCost, 2);
            lblShowPedalCost.Text = "$" + pedalCost;
        }

        //TODO variable for cost; tryparse?
        //Adds pedal(s) to the list to be built.
        //Refreshes the list of components requirted for the pedals to be built.
        private void btnAddToBuildList_Click(object sender, EventArgs e)
        {
            if (lstPedals.SelectedItems[0] != null && lstPedals.Items.Count > 0 && Convert.ToInt16(txtBuildQuantity.Text) > 0)
            {

                try
                {
                    decimal buildCost = new decimal(0.00);
                    ListViewItem selectedPedal = lstPedals.SelectedItems[0];
                    string selectedPedalName = selectedPedal.SubItems[0].Text;
                    Int64 selectedPedalId = Convert.ToInt64(lstPedals.SelectedItems[0].Tag);
                    decimal selectedPedalCost =
                        Convert.ToDecimal(lblShowPedalCost.Text.Substring(1, lblShowPedalCost.Text.Length - 1));
                    Int16 selectedPedalQty = Convert.ToInt16(txtBuildQuantity.Text);

                    PedalBuild build = new PedalBuild(selectedPedalName, selectedPedalId, selectedPedalCost, selectedPedalQty);
                    builds.Add(build);

                    foreach (PedalBuild pedalBuild in builds)
                    {
                        buildCost += pedalBuild.Price * pedalBuild.Quantity;
                    }

                    ListViewItem item = new ListViewItem(build.Name);
                    item.SubItems.Add(build.Quantity.ToString());
                    lstBuilds.Items.Add(item);
                    lblShowBldCost.Text = buildCost.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                FillBuildComponents();
            }
        }

        //Removes a pedal from the list to be built
        //Refreshes the list of components requirted for the pedals to be built.
        private void btnRemoveFrmBuildList_Click(object sender, EventArgs e)
        {

            if (lstBuilds.SelectedItems.Count > 0 && lstBuilds.Items.Count > 0)
            {
                PedalBuild removeBuild = builds.Find(b =>
                    b.Name ==
                    lstBuilds.SelectedItems[0].SubItems[0].Text);

                builds.Remove(removeBuild);

                lstBuilds.Items.Clear();

                foreach (PedalBuild pedalBuild in builds)
                {
                    ListViewItem item = new ListViewItem(pedalBuild.Name);
                    item.SubItems.Add(pedalBuild.Quantity.ToString());
                    lstBuilds.Items.Add(item);
                }

                FillBuildComponents();
                btnRemoveFromBuildList.Enabled = false;
            }
        }

        //Retrieve the components needed for all pedals to be built.
        //Collate multiples of a component
        private void FillBuildComponents()
        {
            lstBuildComponents.Items.Clear();
            try
            {
                con.Open();

                foreach (PedalBuild pedalBuild in builds)
                {
                    string getBldCmpSql = "SELECT * FROM components INNER JOIN partlist ON components.id = partlist.component_id AND partlist.pedal_id =" + pedalBuild.PedalId + " ORDER BY type, value ASC";
                    SQLiteCommand cmd = new SQLiteCommand(getBldCmpSql, con);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string componentType = reader["type"].ToString();
                            string componentValue = reader["value"].ToString();
                            string componentNotes = reader["notes"].ToString();
                            string componentUrl = reader["url"].ToString();
                            Int64 componentPrice = Convert.ToInt64(reader["price"]);
                            Int64 componentID = Convert.ToInt64(reader["id"]);

                            Component component = new Component(componentType, componentValue, componentNotes, componentUrl, componentPrice, componentID);

                            // Handle the quantity of each pedal in the build: add a component for every one.
                            for (int i = 0; i < pedalBuild.Quantity; i++)
                            {
                                componentList.Add(component);
                            }
                        }
                    }
                }

                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // Create a new list of components to display them with the total needed for all builds. 
            var groupedList = componentList.GroupBy(x => x.Id)
                .Select(x => new
                {
                    Count = x.Count(),
                    Type = x.First().Type,
                    Value = x.First().Value,
                    Notes = x.First().Notes,
                    Url = x.First().Url
                });

            foreach (var component in groupedList)
            {
                ListViewItem item = new ListViewItem(component.Type);
                item.SubItems.Add(component.Value);
                item.SubItems.Add(component.Count.ToString());
                item.SubItems.Add(component.Notes);
                item.SubItems.Add(component.Url);
                lstBuildComponents.Items.Add(item);
            }

            componentList.Clear();

            gBoxBuildComponents.Text = lstBuildComponents.Items.Count + " Components Required to Build Pedals";
        }

        //Start the user's default browser if a component in the build list is double clicked and the component has a url.
        private void lstBuildComponents_DoubleClick(object sender, EventArgs e)
        {
            string url = lstBuildComponents.SelectedItems[0].SubItems[4].Text;
            if (url.Length > 0)
            {
                Process.Start(url);
            }
        }
      
        //TODO move to SEED class and separate component methods.  Get lists of components to seed.
        //Seeds the database with many resistor values.
         public void SeedResistors()
        {
            string[] resistors = new string[]
            {
                "0",
                "1",
                "2.2",
                "4.7",
                "5.1",
                "6.8",
                "9.1",
                "10",
                "15",
                "20",
                "22",
                "27",
                "30",
                "33",
                "39",
                "47",
                "51",
                "56",
                "62",
                "68",
                "82",
                "91",
                "110",
                "120",
                "150",
                "180",
                "200",
                "220",
                "240",
                "270",
                "300",
                "330",
                "360",
                "470",
                "510",
                "560",
                "680",
                "820",
                "910",
                "1K",
                "1.2K",
                "1.5K",
                "1.8K",
                "2K",
                "2.2K",
                "2.4K",
                "2.7K",
                "3K",
                "3.3K",
                "3.6K",
                "3.9K",
                "4.3K",
                "4.7K",
                "5.1K",
                "5.6K",
                "6.2K",
                "6.8K",
                "7.5K",
                "8.2K",
                "9.1K",
                "10K",
                "11K",
                "12K",
                "13K",
                "14K",
                "15K",
                "16K",
                "18K",
                "20K",
                "22K",
                "24K",
                "27K",
                "30K",
                "33K",
                "36K",
                "39K",
                "43K",
                "47K",
                "51K",
                "56K",
                "62K",
                "68K",
                "75K",
                "82K",
                "91K",
                "100K",
                "110K",
                "120K",
                "130K",
                "140K",
                "150K",
                "160K",
                "180K",
                "200K",
                "220K",
                "240K",
                "270K",
                "300K",
                "330K",
                "360K",
                "390K",
                "430K",
                "470K",
                "510K",
                "560K",
                "620K",
                "680K",
                "750K",
                "820K",
                "910K",
                "1M",
                "2.2M"
            };

             try
             {
                 con.Open();
                 foreach (string resistor in resistors)
                 {
                     string addCmpSql = "INSERT INTO components (type, value, notes, url, price) VALUES ('Resistor', '" +
                     resistor + "' ,'', ''," + 0.00 + ")";

                     using (SQLiteCommand cmd = new SQLiteCommand(addCmpSql, con))
                     {
                         cmd.ExecuteNonQuery();
                     }
                 }

                 con.Close();
             }
             catch (SQLiteException ex)
             {
                 MessageBox.Show(ex.Message);
             }
        }

        //Menu item to seed resistors.
         private void resistorsToolStripMenuItem_Click(object sender, EventArgs e)
         {
             var confirmResistorSeed = MessageBox.Show("This will fill the component list with many resistors.\n" + "This action cannot be undone.", "Seed Resistors?", MessageBoxButtons.OKCancel);

             if (confirmResistorSeed == DialogResult.OK)
             {
                 SeedResistors();
                 FillComponentList();
             }
             
         }

        //Automatically search the list of components
         private void txtSearch_TextChanged(object sender, EventArgs e)
         {
             searchComponents(txtSearch.Text);
         }

        //When a component is clicked in the builds components list, find it in the component list.
         private void lstBuildComponents_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
         {
             if (e.IsSelected)
             {
                 searchComponents(e.Item.SubItems[1].Text);
             }
             
         }

        //Search the component list for the passed string.
        //Only exact matches unfortunately.
        private void searchComponents(string searchString)
        {
            if (searchString.Length > 0)
            {
                ListViewItem item = lstComponents.FindItemWithText(searchString, true, 0, false);
                if (item != null)
                {
                    lstComponents.TopItem = item;
                }
            }
        }

        //When a component is clicked in the part list, show it in the component list.
        private void lstPedalComponents_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                searchComponents(e.Item.SubItems[1].Text);
            }
        }

        //Enable/disable the Add Component and Remove Component buttons.
        private void txtAddComponentToPedalName_TextChanged(object sender, EventArgs e)
        {
            if (txtAddComponentToPedalName.Text.Length > 0 && lstPedals.SelectedItems.Count == 1)
            {
                btnAddComponentToPedal.Enabled = true;
                btnRemoveComponentFromPedal.Enabled = true;
            }
            else
            {
                btnAddComponentToPedal.Enabled = false;
                btnRemoveComponentFromPedal.Enabled = false;
            }
        }

        //Enable/disable the New Pedal and Update Pedal buttons.
        private void txtPedalName_TextChanged(object sender, EventArgs e)
        {
            if (txtPedalName.Text.Length > 0)
            {
                btnNewPedal.Enabled = true;
            }
            else
            {
                btnNewPedal.Enabled = false;
            }

            if (txtPedalName.Text.Length > 0 && lstPedals.SelectedItems.Count == 1)
            {
                btnPdlUpdate.Enabled = true;
            }
            else
            {
                btnPdlUpdate.Enabled = false;
            }
        }

        //Enable/disable the New Component and Update Component buttons.
        private void txtCmpType_TextChanged(object sender, EventArgs e)
        {
            if (txtCmpType.Text.Length > 0 && txtCmpValue.Text.Length > 0)
            {
                btnNewComponent.Enabled = true;
            }
            else
            {
                btnNewComponent.Enabled = false;
            }

            if (txtCmpType.Text.Length > 0 && txtCmpValue.Text.Length > 0 && lstComponents.SelectedItems.Count == 1)
            {
                btnUpdateComponent.Enabled = true;
            }
            else
            {
                btnUpdateComponent.Enabled = false;
            }
        }

        //Enable/Disable the New Component and Update Component buttons.
        private void txtCmpValue_TextChanged(object sender, EventArgs e)
        {
            if (txtCmpType.Text.Length > 0 && txtCmpValue.Text.Length > 0)
            {
                btnNewComponent.Enabled = true;
            }
            else
            {
                btnNewComponent.Enabled = false;
            }

            if (txtCmpType.Text.Length > 0 && txtCmpValue.Text.Length > 0 && lstComponents.SelectedItems.Count == 1)
            {
                btnUpdateComponent.Enabled = true;
            }
            else
            {
                btnUpdateComponent.Enabled = false;
            }
        }

        //Enable/disable the Add to Build List button.
        private void txtBuildQuantity_TextChanged(object sender, EventArgs e)
        {
            long number;
            bool isInteger = Int64.TryParse(txtBuildQuantity.Text.ToString(), out number);
            if (isInteger && lstPedals.SelectedItems.Count == 1)
            {
                btnAddToBuildList.Enabled = true;
            }
            else
            {
                btnAddToBuildList.Enabled = false;
            }
        }

        //Enable/disable the Remove From Build List button.
        private void lstBuilds_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (lstBuilds.SelectedItems.Count == 1)
            {
                btnRemoveFromBuildList.Enabled = true;
            }
        }
    }
}
