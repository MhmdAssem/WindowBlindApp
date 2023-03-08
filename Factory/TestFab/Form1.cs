using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports;

using System.Drawing.Printing;




namespace TestFab
{
    public partial class Form1 : Form
    {
         Boolean Login = false;
         Config newconfig = new Config();
         static string ConfigFileName = @"Job Configs.xml";
         public static XmlNodeList xnList;
         DataGridViewButtonColumn SelectButton;//edit buttons on the grid
         DataGridViewButtonColumn RemoveButton;//edit buttons on the grid
         int AlphaCounter;
         
         static Dictionary<string, string> FabricRollwidth = new Dictionary<String, string>();
       //  static Dictionary<string, string> LatheType = new Dictionary<String, string>();
            static Dictionary<string, int> ControlTypevalues = new Dictionary<string, int>();
            static Dictionary<string, List<string>> LatheType = new Dictionary<String, List<string>>();
            static Dictionary<int, List<int>> SequenceNumber = new Dictionary<int, List<int>>();

        public Form1()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
                        
            if ((txtLoginName.Text == "") || (cbTableNumber.Text == ""))
            {
                
                MessageBox.Show("Please enter login details", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLoginName.Focus();

            }
            else if (!Login)
            {
                cbsearch.Text = "Please, select search citeria";

                if(Directory.Exists(newconfig.LogPath))
                {
                    if (!File.Exists(newconfig.LogPath + "Log.xls"))
                    {
                        using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.LogPath + "Log.xls;Extended Properties='Excel 8.0;HDR=Yes'"))
                        {
                            conn.Open();
                            OleDbCommand cmd = new OleDbCommand("CREATE TABLE [LogData] ([CB Number] string, [Barcode] string,[Table Number] string,[Name] string,[Datetime] string)", conn);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(newconfig.LogPath + "Log.xls");
                        if (fi.CreationTime < DateTime.Now.AddMonths(-1))
                        {
                            //fi.Delete();

                            File.Move(newconfig.LogPath + "Log.xls", newconfig.LogPath +"\\Archieve\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + "Log.xls");

                            using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.LogPath + "Log.xls;Extended Properties='Excel 8.0;HDR=Yes'"))
                            {
                                conn.Open();
                                OleDbCommand cmd = new OleDbCommand("CREATE TABLE [LogData] ([CB Number] string, [Barcode] string,[Table Number] string,[Name] string,[Datetime] string)", conn);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    //check how old is the file
                }

                //load rollwith and lathe :TODO
                if(File.Exists(@newconfig.RollWidth))
                {
                    string query;

                    using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.RollWidth + ";Extended Properties='Excel 8.0;HDR=Yes'"))
                    {
                        conn.Open();
                        query = "select * from [Sheet1$]";
                        OleDbCommand cm = new OleDbCommand(query, conn);
                        cm.CommandText = query;
                        OleDbDataReader rdr = (OleDbDataReader)cm.ExecuteReader();
                        DataTable dt = new DataTable();

                        dt.Load(rdr);
                        //:TODO add to dictionary
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                FabricRollwidth.Add(row[0].ToString().ToUpper(), row[1].ToString());

                            }
                        }
                    }
                }


                if (File.Exists(@newconfig.Lathe))
                {
                    string query;

                    using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.Lathe + ";Extended Properties='Excel 8.0;HDR=Yes'"))
                    {
                        conn.Open();
                        query = "select * from [Sheet1$]";
                        OleDbCommand cm = new OleDbCommand(query, conn);
                        cm.CommandText = query;
                        OleDbDataReader rdr = (OleDbDataReader)cm.ExecuteReader();
                        DataTable dt = new DataTable();

                        dt.Load(rdr);
                        //:TODO add to dictionary
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                string key = row[0].ToString().TrimEnd();
                                string val = row[1].ToString().TrimEnd();

                                if (LatheType.ContainsKey(key))
                                {
                                    LatheType[key].Add(val);
                                }
                                else
                                {
                                    List<string> list = new List<string>();
                                    list.Add(val);
                                    LatheType[key] = list;
                                }
                            }
                        }
                    }
                }

                if (File.Exists(@newconfig.WidthDeduct))
                {
                    string query;
                    using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.WidthDeduct + ";Extended Properties='Excel 8.0;HDR=Yes'"))
                    {
                        conn.Open();
                        query = "select * from [Sheet1$]";
                        OleDbCommand cm = new OleDbCommand(query, conn);
                        cm.CommandText = query;
                        OleDbDataReader rdr = (OleDbDataReader)cm.ExecuteReader();
                        DataTable dt = new DataTable();

                        dt.Load(rdr);
                        //:TODO add to dictionary
                        if (dt.Rows.Count > 0)
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                ControlTypevalues.Add(row[0].ToString().ToUpper(), Convert.ToInt32(row[1].ToString()));

                            }
                        }
                    }

                     
                }

                txtLoginName.Enabled = false;
                cbTableNumber.Enabled = false;
                Login = true;
                btnLogin.Text = "Logout";
                // btnLogout.Visible = true;
                //btnLogin.Visible = false;
                btnConfig.Visible = false;
                gbSelection.Enabled = true;
                gbReview.Enabled = true;
                gbAction.Enabled = true;
                tbCBNumber.Focus();
                
                
                //ControlTypevalues.Add("Easylink Control", -25);
                //ControlTypevalues.Add("Easylink Centre", -22);
                //ControlTypevalues.Add("Easylink Idler", -24);
                //ControlTypevalues.Add("Motorised", -35);
                //ControlTypevalues.Add("Motor Control", -30);
                //ControlTypevalues.Add("WT Motor", -35);
                //ControlTypevalues.Add("RTS Motor", -35);
                //ControlTypevalues.Add("VS Motor", -35);
                //ControlTypevalues.Add("VSWT Motor", -33);
                //ControlTypevalues.Add("Wire Free", -30);




                if (dgvReview.Columns.Count == 0)
                {
                    dgvReview.Columns.Add("Department", "Department");
                    dgvReview.Columns.Add("Customer", "Customer");
                    dgvReview.Columns.Add("CB Number", "CB Number");
                    dgvReview.Columns.Add("Blind Number", "Blind Number"); // assign this in txt file only
                    dgvReview.Columns.Add("Quantity", "Quantity");
                    dgvReview.Columns.Add("Measured Width", "Measured Width");
                    dgvReview.Columns.Add("Measured Drop", "Measured Drop");
                    dgvReview.Columns.Add("Control Type", "Control Type");
                    dgvReview.Columns.Add("Hem", "Hem");
                    dgvReview.Columns.Add("Fabric Type", "Fabric Type");
                    dgvReview.Columns.Add("Fabric Colour", "Fabric Colour");
                    dgvReview.Columns.Add("Trim Type", "Trim Type");
                    dgvReview.Columns.Add("Track Colour", "Track Colour");
                    dgvReview.Columns.Add("Roll Width", "Roll Width");
                    dgvReview.Columns.Add("Pull Colour", "Pull Colour");
                    dgvReview.Columns.Add("Alpha Number", "Alpha Number"); //assign this is txt file only
                    dgvReview.Columns.Add("Barcode", "Barcode");
                    dgvReview.Columns.Add("Total Blinds", "Total Blinds"); //assign this is txt file only
                    dgvReview.Columns.Add("Cut Width", "Cut Width");
                    dgvReview.Columns.Add("Control Side", "Control Side"); // Added on 8-10-2016

                    RemoveButton = new DataGridViewButtonColumn();
                    RemoveButton.HeaderText = "Remove";
                    RemoveButton.Text = "Remove";
                    RemoveButton.UseColumnTextForButtonValue = true;
                    RemoveButton.Width = 80;
                    dgvReview.Columns.Add(RemoveButton);
                    dgvReview.Columns["Department"].Visible = false;
                   
                    dgvReview.Columns["Customer"].Visible = false;
                    dgvReview.Columns["Total Blinds"].Visible = false;

                    dgvReview.Columns["Control Type"].Visible = false;
                    dgvReview.Columns["Hem"].Visible = false;
                    dgvReview.Columns["Track Colour"].Visible = false;
                    dgvReview.Columns["Pull Colour"].Visible = false;
                    dgvReview.Columns["Alpha Number"].Visible = false;

                    dgvReview.Columns["Barcode"].Visible = false;
                    dgvReview.Columns["Total Blinds"].Visible = false;
                    dgvReview.Columns["Cut Width"].Visible = false;
                    dgvReview.AllowUserToAddRows = false;
                    dgvReview.ReadOnly = true;
                    dgvReview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                }
                //if (dgvCBSelector.Columns.Count == 0)
                //{
                //    dgvCBSelector.Columns.Add("Department", "Department");
                //    dgvCBSelector.Columns.Add("CB Number", "CB Number");
                //    //  dtFinal.Columns.Add("Line Number", typeof(string)); // assign this in txt file only
                //    dgvCBSelector.Columns.Add("Quantity", "Quantity");
                //    dgvCBSelector.Columns.Add("Measured Width", "Measured Width");
                //    dgvCBSelector.Columns.Add("Measured Drop", "Measured Drop");
                //    dgvCBSelector.Columns.Add("Control Type", "Control Type");
                //    dgvCBSelector.Columns.Add("Hem", "Hem");
                //    dgvCBSelector.Columns.Add("Fabric Type", "Fabric Type");
                //    dgvCBSelector.Columns.Add("Fabric Colour", "Fabric Colour");
                //    dgvCBSelector.Columns.Add("Trim Type", "Trim Type");
                //    dgvCBSelector.Columns.Add("Track Colour", "Track Colour");
                //    dgvCBSelector.Columns.Add("Roll Width", "Roll Width");
                //    dgvCBSelector.Columns.Add("Pull Colour", "Pull Colour");
                //    //dtFinal.Columns.Add("Alpha Number", typeof(string)); //assign this is txt file only
                //    dgvCBSelector.Columns.Add("Barcode", "Barcode");
                //    //dtFinal.Columns.Add("Total Blinds", typeof(Int32)).DefaultValue = 0; //assign this is txt file only
                //    SelectButton = new DataGridViewButtonColumn();
                //    SelectButton.HeaderText = "Select";
                //    SelectButton.Text = "Select";
                //    SelectButton.UseColumnTextForButtonValue = true;
                //    SelectButton.Width = 80;
                //    dgvCBSelector.Columns.Add(SelectButton);
                //}

                
            }
            else
            {
                txtLoginName.Enabled = true;
                txtLoginName.Focus();
                cbTableNumber.Enabled = true;
                Login = false;
                btnLogin.Text = "Login";
                txtLoginName.Text = "";
                cbTableNumber.Text = "";
                btnConfig.Visible = true;
                gbSelection.Enabled = false;
                gbReview.Enabled = false;
                gbAction.Enabled = false;
                dgvReview.DataSource = null;
                dgvCBSelector.DataSource = null;
                dgvReview.Columns.Clear();
                dgvCBSelector.Columns.Clear();
                dgvReview.Visible = false;
                cbTableNumber.SelectedIndex = -1;
                FabricRollwidth.Clear();
                LatheType.Clear();
                KillLabelPrintApp();



                

            }
        }

        private  DataTable LoadData()
        {
            DataTable dtFinal = new DataTable();
            if (SequenceNumber.Count > 0)
            {

                SequenceNumber.Clear();
            }
            dtFinal.Columns.Add("Department", typeof(string));
            dtFinal.Columns.Add("CB Number", typeof(string));
            dtFinal.Columns.Add("Customer", typeof(string));
            dtFinal.Columns.Add("Line Number", typeof(string)); // assign this in txt file only
            dtFinal.Columns.Add("Quantity", typeof(string));
            dtFinal.Columns.Add("Measured Width", typeof(string));
            dtFinal.Columns.Add("Measured Drop", typeof(string));
            dtFinal.Columns.Add("Control Type", typeof(string));
            dtFinal.Columns.Add("Hem", typeof(string));
            dtFinal.Columns.Add("Fabric Type", typeof(string));
            dtFinal.Columns.Add("Fabric Colour", typeof(string));
            dtFinal.Columns.Add("Trim Type", typeof(string));
            dtFinal.Columns.Add("Control Side", typeof(string)); //Added on 8-10-2016
            dtFinal.Columns.Add("Track Colour", typeof(string));
            dtFinal.Columns.Add("Roll Width", typeof(string));
            dtFinal.Columns.Add("Pull Colour", typeof(string));
           // dtFinal.Columns.Add("Alpha Number", typeof(string)); //assign this is txt file only
            dtFinal.Columns.Add("Barcode", typeof(string));
            dtFinal.Columns.Add("Total Blinds", typeof(Int32)).DefaultValue = 0; //assign this is txt file only
            dtFinal.Columns.Add("Table Number", typeof(string)).DefaultValue = "";
            dtFinal.Columns.Add("User", typeof(string)).DefaultValue = "";
            dtFinal.Columns.Add("Date", typeof(string)).DefaultValue = "";
            dtFinal.Columns.Add("Cut Width", typeof(string));



            string query;
            string Con_Str = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + @newconfig.FilePath +
                                           ";Mode=ReadWrite;Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";

            using (OleDbConnection connection = new OleDbConnection(Con_Str))
            {

                connection.Open();
                query = "select " + newconfig.Columns + " from [" + newconfig.SheetNameValue + "$] WHERE TRIM([W/Order NO]) =  '" + (tbCBNumber.Text.TrimEnd()) + "' AND NOT TRIM([Department]) = ''";
                //"select " + newconfig.Columns + " from [" + newconfig.SheetNameValue + "$] WHERE [W/Order NO] =  '" + (tbCBNumber.Text.TrimEnd()) + "' AND NOT [Department] = '' "
                try
                {
                    OleDbCommand cm = new OleDbCommand(query, connection);
                    cm.CommandText = query;
                    OleDbDataReader rdr = (OleDbDataReader)cm.ExecuteReader();
                    DataTable dt = new DataTable();

                    dt.Load(rdr);
              
                    if (dt.Rows.Count > 0)
                    {
                        int LineCounter = 1;

                        int ToatalQty = Convert.ToInt32(dt.Compute("SUM(Qty)", string.Empty));


                        Int32 Totallines = dt.Rows.Count;
                        int i = 1;
                        foreach (DataRow dr in dt.Rows)
                        {


                            int totalblindLines = Convert.ToInt32(dr["Qty"].ToString());
                            for (int blinds = 1; blinds <= totalblindLines; blinds++)
                            {
                                int key = i;
                                int val = LineCounter;
                                if (SequenceNumber.ContainsKey(key))
                                {
                                    SequenceNumber[key].Add(val);

                                }
                                else
                                {
                                    List<int> list = new List<int>();
                                    list.Add(val);
                                    SequenceNumber[key] = list;

                                }

                                LineCounter++;

                            }

                            DataRow row = dtFinal.NewRow();
                            row["Department"] = dr["Department"].ToString().TrimEnd();
                            row["Customer"] = dr["Customer Name 1"].ToString().TrimEnd();
                            row["CB Number"] = dr["W/Order NO"].ToString().TrimEnd();
                            row["Line Number"] = i.ToString();
                            row["Quantity"] = dr["Qty"].ToString().TrimEnd();

                            //Change for Fin 36 and Motorised
                            if (Right(dr["Description"].ToString().TrimEnd(), 6) == "FIN 36" && dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd() == "Motorised")
                            {
                                

                                    row["Measured Width"] = (Convert.ToInt32(dr["Width"].ToString().TrimEnd()) - 5).ToString();

                                
                                
                            }
                            else
                            {
                                row["Measured Width"] = dr["Width"].ToString().TrimEnd();
                            }
                            



                            row["Measured Drop"] = dr["Drop"].ToString().TrimEnd();
                            if (dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd() != "")
                            {
                                row["Control Type"] = dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd();
                            }
                            else
                            {
                                row["Control Type"] = "Pin";
                            }
                            row["Hem"] = "";
                            row["Fabric Type"] = dr["Fabric"].ToString().TrimEnd();
                            row["Fabric Colour"] = dr["Colour"].ToString().TrimEnd();
                            //source.Substring(0, source.Length - 1)
                            string fab = dr["Fabric"].ToString().TrimEnd();

                            if (fab != "")
                            {
                                if (FabricRollwidth.ContainsKey(fab.Substring(0, fab.Length - 6).TrimEnd()))
                                {
                                    row["Roll Width"] = ((FabricRollwidth[fab.Substring(0, fab.Length - 6).TrimEnd()]).Substring(0, 4));

                                }
                                else
                                {
                                    row["Roll Width"] = (Left(dr["Fabric"].ToString().TrimEnd(), 6)).Substring(0, 4);
                                }
                            }
                            row["Trim Type"] = Right(dr["Description"].ToString().TrimEnd(), 6);
                            row["Track Colour"] = dr["Track Col/Roll Type/Batten Col"].ToString().TrimEnd();
                            if (dr["Pull Colour/Bottom Weight/Wand Len"].ToString().TrimEnd() != "")
                            {
                                row["Pull Colour"] = dr["Pull Colour/Bottom Weight/Wand Len"].ToString().TrimEnd();
                            }
                            else
                            {
                                // key = the key you are looking for
                                // value = the value you are looking for

                                string LookupKey = dr["Fabric"].ToString().TrimEnd();

                                if (LatheType.ContainsKey(LookupKey) && LatheType[LookupKey].Contains(dr["Colour"].ToString().TrimEnd()))
                                {
                                    row["Pull Colour"] = "PVC Lathe";

                                }
                                else
                                {
                                    row["Pull Colour"] = "Lathe";
                                }

                            }
                            row["Barcode"] = dr["Line No#"].ToString().TrimEnd();
                            row["Total Blinds"] = ToatalQty;
                            if (dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd() != "")
                            {
                                if (ControlTypevalues.ContainsKey(dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()))
                                {
                                    row["Cut Width"] = Convert.ToInt32(dr["Width"]) + ControlTypevalues[dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd().ToUpper()];
                                    // row["Control Type"] = dr["Bind# Type/# Panels/Rope/Operation"].ToString().TrimEnd();


                                }
                            }
                            else
                            {
                                row["Cut Width"] = Convert.ToInt32(dr["Width"]) - 30;
                            }
                            if (dr["Cntrl Side"].ToString().TrimEnd() != "")
                            {
                                row["Control Side"] = dr["Cntrl Side"].ToString().TrimEnd().ToCharArray()[0];// Added on 8-10-2016
                            }
                            else
                            {
                                row["Control Side"] = "N";

                            }

                            //row["Cut Width"]




                            if (File.Exists(newconfig.LogPath + "Log.xls"))
                            {
                                using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.LogPath + "Log.xls;Extended Properties='Excel 8.0;HDR=Yes'"))
                                {
                                    conn.Open();
                                    query = "select * from [LogData$] Where [CB Number] = '" + row["CB Number"].ToString() + "' AND [Barcode] = '" + row["Barcode"].ToString() + "'";
                                    OleDbCommand cm1 = new OleDbCommand(query, conn);
                                    cm1.CommandText = query;
                                    OleDbDataReader read = (OleDbDataReader)cm1.ExecuteReader();
                                    DataTable dt1 = new DataTable();

                                    dt1.Load(read);
                                    if (dt1.Rows.Count > 0)
                                    {
                                        row["Table Number"] = dt1.Rows[0]["Table Number"].ToString().TrimEnd();
                                        row["User"] = dt1.Rows[0]["Name"].ToString().TrimEnd();
                                        row["Date"] = dt1.Rows[0]["Datetime"].ToString().TrimEnd();
                                    }

                                }

                            }
                            dtFinal.Rows.Add(row);
                            // }
                            //  LineCounter++;
                            i++;

                        }
                
                    }
                //insert catch
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());

                }

                    return dtFinal;
                

            }

            }

        

        public static string Right(string param, int length)
        {
            //start at the index based on the lenght of the sting minus
            //the specified lenght and assign it a variable
            string result = param.Substring(param.Length - length, length);
            //return the result of the operation
            return result;
        }
        public static string Left(string mystring, int length)
        {
            var result = mystring.Substring(mystring.Length - Math.Min(length, mystring.Length));
            return result;
        }


        private void btnGetCB_Click(object sender, EventArgs e)
        {
            GetButtonClick();
        }


        private void GetButtonClick()
        {
              dgvCBSelector.DataSource = null;
              dgvCBSelector.AllowUserToAddRows = false;
            dgvCBSelector.Columns.Clear();
            if (tbCBNumber.Text != "")
            {
                dgvCBSelector.DataSource = LoadData();

                if (dgvCBSelector.Rows.Count >= 1)
                {
                    dgvCBSelector.Columns["Department"].Visible = false;
                    dgvCBSelector.Columns["Control Type"].Visible = false;
                    dgvCBSelector.Columns["Hem"].Visible = false;
                    dgvCBSelector.Columns["Track Colour"].Visible = false;
                    dgvCBSelector.Columns["Pull Colour"].Visible = false;
                    dgvCBSelector.Columns["Barcode"].Visible = false;
                    dgvCBSelector.Columns["Line Number"].Visible = false;
                    dgvCBSelector.Columns["Customer"].Visible = false;
                    dgvCBSelector.Columns["Total Blinds"].Visible = false;
                    dgvCBSelector.Columns["Cut Width"].Visible = false;
                    SelectButton = new DataGridViewButtonColumn();
                    SelectButton.HeaderText = "Select";
                    SelectButton.Text = "Select";
                    SelectButton.UseColumnTextForButtonValue = true;
                    SelectButton.Width = 80;
                    dgvCBSelector.Columns.Add(SelectButton);

                    dgvCBSelector.AutoResizeColumns();
                    //  RefreshDataGridColumns();  
                    tbCBNumber.Text = "";
                    dgvCBSelector.EditMode = DataGridViewEditMode.EditOnF2;
                    //dgvCBSelector.ReadOnly = true;
                    dgvCBSelector.Columns["CB Number"].ReadOnly = true;
                   
                    dgvCBSelector.Columns["Quantity"].ReadOnly = true;
                    dgvCBSelector.Columns["Measured Width"].ReadOnly = true;
                    dgvCBSelector.Columns["Measured Drop"].ReadOnly = true;
                    dgvCBSelector.Columns["Control Type"].ReadOnly = true;
                    dgvCBSelector.Columns["Fabric Type"].ReadOnly = true;
                    dgvCBSelector.Columns["Fabric Colour"].ReadOnly = true;
                    dgvCBSelector.Columns["Trim Type"].ReadOnly = true;
                    //dgvCBSelector.Columns["Fabric Colour"].ReadOnly = true;
               
                    dgvCBSelector.Columns["Table Number"].ReadOnly = true;
                    dgvCBSelector.Columns["User"].ReadOnly=true;
                    dgvCBSelector.Columns["Date"].ReadOnly = true;
                  
                    dgvCBSelector.Columns["Roll Width"].ReadOnly = false;
                   
                    AlphaCounter = 0;
                    dgvCBSelector.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                    dgvCBSelector.Columns["Control Side"].ReadOnly = true;  //added om 8-10-2016

                    foreach (DataGridViewRow row in dgvCBSelector.Rows)
                    {
                        if (row.Cells["User"].Value.ToString().TrimEnd() != "")
                        {
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                                //CurrentRow.DefaultCellStyle.BackColor = System.Drawing.Color.Yellow;
                        }
                    }
                }
                else
                {
                    dgvCBSelector.DataSource = null;

                    MessageBox.Show("Order not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }

            }
            else
            {
                MessageBox.Show("Enter a CB Number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            //dgvCBSelector.DataSource = Database.getCBDetails();

            

        }
        private void btnConfig_Click(object sender, EventArgs e)
        {
            Configurator config = new Configurator(newconfig,ConfigFileName);
            config.Show();

        }


        private void ReadConfigOptions()
        {

            XmlDocument xml = new XmlDataDocument();
            xml.Load(ConfigFileName);
            xnList = xml.SelectNodes("/ConfigDetails");

            foreach (XmlNode node in xnList)
            {
                newconfig.FilePath = node.SelectSingleNode("XLFilePath").InnerText;
                newconfig.LogPath = node.SelectSingleNode("LogFilePath").InnerText;
                newconfig.RollWidth = node.SelectSingleNode("RollwidhtPath").InnerText;
                newconfig.Lathe = node.SelectSingleNode("LathePath").InnerText;

                newconfig.CounterValue = Convert.ToInt16(node.SelectSingleNode("Counter").InnerText);
                newconfig.Columns = node.SelectSingleNode("SelectColumns").InnerText;
                newconfig.ToEmailAddress = node.SelectSingleNode("ToEmailAddress").InnerText;
                newconfig.SheetNameValue = node.SelectSingleNode("SheetName").InnerText;
                newconfig.SaveFilesPath = node.SelectSingleNode("SaveFilesPath").InnerText;
                newconfig.ftpSiteName = node.SelectSingleNode("FTPSiteName").InnerText;
                newconfig.Printer = node.SelectSingleNode("Printer").InnerText;
                newconfig.FTPSiteUsername = node.SelectSingleNode("FTPUName").InnerText;
                newconfig.FTPSitepassword = node.SelectSingleNode("FTPPassword").InnerText;
                newconfig.WidthDeduct = node.SelectSingleNode("WidthDeduction").InnerText;
               

            }




        }

        private void Form1_Load(object sender, EventArgs e)
        {


           
           
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(ConfigFileName))
            {
                Configurator config = new Configurator(newconfig, ConfigFileName);
                config.Show();

            }
            else
            {
                //Load the Config file
                ReadConfigOptions();
            }

           
        }

        private void tbColorSearch_TextChanged(object sender, EventArgs e)
        {
            if (dgvCBSelector.Rows.Count > 0)
            {
                ((DataTable)dgvCBSelector.DataSource).DefaultView.RowFilter = "Convert([" + cbsearch.Text + "],System.String) Like '*$ORNO*' ".Replace("$ORNO", tbColorSearch.Text);
                dgvCBSelector.AutoResizeColumns();
            }
        }

  
        private void dgvCBSelector_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvCBSelector.Columns[e.ColumnIndex] == SelectButton)
            {
                int LineNumber = Convert.ToInt32(dgvCBSelector.CurrentRow.Cells["Line Number"].Value.ToString());



                AlphaCounter++;

                if (!dgvReview.Visible)
                {
                    dgvReview.Visible = true;

                }
                
               

                int totalblindLines = Convert.ToInt32(dgvCBSelector.CurrentRow.Cells["Quantity"].Value.ToString());
                //int BlindNumbers = (LineNumber * totalblindLines) - (totalblindLines -1 );

                  for (int blinds = 1; blinds <= totalblindLines; blinds++)
                  {

                      int seqNumber = SequenceNumber[Convert.ToInt32(dgvCBSelector.CurrentRow.Cells["Line Number"].Value.ToString())][blinds - 1];

                      string alpha = Number2String(AlphaCounter,true);
                     // dgvReview.Rows.Add(dgvCBSelector.CurrentRow.Cells["Department"].Value.ToString(),dgvCBSelector.CurrentRow.Cells["Customer"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["CB Number"].Value.ToString(), LineNumber.ToString(), "1", dgvCBSelector.CurrentRow.Cells["Measured Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Measured Drop"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Control Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Hem"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Fabric Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Fabric Colour"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Trim Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Track Colour"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Roll Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Pull Colour"].Value.ToString(), Number2String(Convert.ToInt32(dgvCBSelector.CurrentRow.Cells["Line Number"].Value.ToString()),true), dgvCBSelector.CurrentRow.Cells["Barcode"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Total Blinds"].Value.ToString());
                      dgvReview.Rows.Add(dgvCBSelector.CurrentRow.Cells["Department"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Customer"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["CB Number"].Value.ToString(), seqNumber.ToString(), "1", dgvCBSelector.CurrentRow.Cells["Measured Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Measured Drop"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Control Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Hem"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Fabric Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Fabric Colour"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Trim Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Track Colour"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Roll Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Pull Colour"].Value.ToString(), Number2String(Convert.ToInt32(dgvCBSelector.CurrentRow.Cells["Line Number"].Value.ToString()), true), dgvCBSelector.CurrentRow.Cells["Barcode"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Total Blinds"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Cut Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Control Side"].Value.ToString());
                      LineNumber++;
                    //  BlindNumbers++;
                  }



                //foreach (DataGridViewCell cell in dgvCBSelector.CurrentRow.Cells)
                //{
                //    r.Cells[cell.ColumnIndex].Value = cell.Value;
                //}

                //this.dgvReview.Rows.Add(r);
                ////dgvReview.Rows.Add(l_NewDividerRow);
                  lbTotalBlinds.Text = "Blinds Selected: " + dgvReview.Rows.Count.ToString();

                dgvCBSelector.Rows.Remove(dgvCBSelector.CurrentRow);

            }



        }

        private void UpdateALL()
        {
            foreach (DataGridViewRow row in dgvCBSelector.Rows)
            {
                int LineNumber = Convert.ToInt32(row.Cells["Line Number"].Value.ToString());



                AlphaCounter++;

                if (!dgvReview.Visible)
                {
                    dgvReview.Visible = true;

                }



                int totalblindLines = Convert.ToInt32(row.Cells["Quantity"].Value.ToString());
                //int BlindNumbers = (LineNumber * totalblindLines) - (totalblindLines -1 );

                for (int blinds = 1; blinds <= totalblindLines; blinds++)
                {

                    int seqNumber = SequenceNumber[Convert.ToInt32(row.Cells["Line Number"].Value.ToString())][blinds - 1];

                    string alpha = Number2String(AlphaCounter, true);
                    // dgvReview.Rows.Add(dgvCBSelector.CurrentRow.Cells["Department"].Value.ToString(),dgvCBSelector.CurrentRow.Cells["Customer"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["CB Number"].Value.ToString(), LineNumber.ToString(), "1", dgvCBSelector.CurrentRow.Cells["Measured Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Measured Drop"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Control Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Hem"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Fabric Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Fabric Colour"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Trim Type"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Track Colour"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Roll Width"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Pull Colour"].Value.ToString(), Number2String(Convert.ToInt32(dgvCBSelector.CurrentRow.Cells["Line Number"].Value.ToString()),true), dgvCBSelector.CurrentRow.Cells["Barcode"].Value.ToString(), dgvCBSelector.CurrentRow.Cells["Total Blinds"].Value.ToString());
                    dgvReview.Rows.Add(row.Cells["Department"].Value.ToString(), row.Cells["Customer"].Value.ToString(), row.Cells["CB Number"].Value.ToString(), seqNumber.ToString(), "1", row.Cells["Measured Width"].Value.ToString(), row.Cells["Measured Drop"].Value.ToString(), row.Cells["Control Type"].Value.ToString(), row.Cells["Hem"].Value.ToString(), row.Cells["Fabric Type"].Value.ToString(), row.Cells["Fabric Colour"].Value.ToString(), row.Cells["Trim Type"].Value.ToString(), row.Cells["Track Colour"].Value.ToString(), row.Cells["Roll Width"].Value.ToString(), row.Cells["Pull Colour"].Value.ToString(), Number2String(Convert.ToInt32(row.Cells["Line Number"].Value.ToString()), true), row.Cells["Barcode"].Value.ToString(), row.Cells["Total Blinds"].Value.ToString());
                    LineNumber++;
                    //  BlindNumbers++;
                }



                //foreach (DataGridViewCell cell in dgvCBSelector.CurrentRow.Cells)
                //{
                //    r.Cells[cell.ColumnIndex].Value = cell.Value;
                //}

                //this.dgvReview.Rows.Add(r);
                ////dgvReview.Rows.Add(l_NewDividerRow);
             //   dgvCBSelector.Rows.Remove(row);
               
            }
            while (dgvCBSelector.Rows.Count > 0)
            {

                dgvCBSelector.Rows.RemoveAt(0);

            }
            lbTotalBlinds.Text = "Blinds Selected: " + dgvReview.Rows.Count.ToString();

        }



        private String Number2String(int number, bool isCaps)
        {

            Char c = (Char)((isCaps ? 65 : 97) + (number - 1));

            return c.ToString();

        }

        private void dgvReview_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvReview.Columns[e.ColumnIndex] == RemoveButton)
            {
                if (dgvReview.Rows.Count > 0)
                {
                    dgvReview.Rows.Remove(dgvReview.CurrentRow);
                }
                lbTotalBlinds.Text = "Blinds Selected: " + dgvReview.Rows.Count.ToString();
            }
          
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //TODO Create File/Log
            StringBuilder sb = new StringBuilder();
            StringBuilder labels = new StringBuilder();
            while (dgvReview.Rows.Count > 0)
            {

                sb.Append("START##\t" +  dgvReview.CurrentRow.Cells["Department"].Value.ToString().TrimEnd() + Environment.NewLine);
                sb.Append("ORDER NUMBER\t" + dgvReview.CurrentRow.Cells["CB Number"].Value.ToString().TrimEnd()+ Environment.NewLine);
                sb.Append("LINE NUMBER\t" + dgvReview.CurrentRow.Cells["Blind Number"].Value.ToString().TrimEnd() +"\t\t"+ Environment.NewLine);
                sb.Append("QUANTITY\t" + dgvReview.CurrentRow.Cells["Quantity"].Value.ToString().TrimEnd() + Environment.NewLine);
                sb.Append("MEASURED WIDTH\t" + dgvReview.CurrentRow.Cells["Measured Width"].Value.ToString().TrimEnd() + Environment.NewLine);
                sb.Append("MEASURED DROP\t" + dgvReview.CurrentRow.Cells["Measured Drop"].Value.ToString().TrimEnd() + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("CONTROL TYPE\t" + dgvReview.CurrentRow.Cells["Control Type"].Value.ToString().TrimEnd() + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("HEM\t" + Environment.NewLine);
                sb.Append("FABRIC TYPE\t" + dgvReview.CurrentRow.Cells["Roll Width"].Value.ToString().TrimEnd() + Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("FABRIC COLOUR\t" + Environment.NewLine);
                sb.Append("TRIM TYPE\t" + dgvReview.CurrentRow.Cells["Trim Type"].Value.ToString().TrimEnd() + Environment.NewLine);
                
                insertLog(dgvReview.CurrentRow.Cells["CB Number"].Value.ToString().TrimEnd(), dgvReview.CurrentRow.Cells["Barcode"].Value.ToString().TrimEnd(), cbTableNumber.Text, txtLoginName.Text, System.DateTime.Now.ToShortDateString());

                //TODO Send INFO for LABEL
                labels.Append("@" + dgvReview.CurrentRow.Cells["CB Number"].Value.ToString().TrimEnd());

                if (dgvReview.CurrentRow.Cells["Trim Type"].Value.ToString().TrimEnd() == "FIN 36" && dgvReview.CurrentRow.Cells["Control Type"].Value.ToString().TrimEnd() == "Motorised")
                {
                    labels.Append("@" + (Convert.ToInt32(dgvReview.CurrentRow.Cells["Measured Width"].Value.ToString().TrimEnd()) + 5).ToString());

                }
                else
                {
                    labels.Append("@" + dgvReview.CurrentRow.Cells["Measured Width"].Value.ToString().TrimEnd());
                }
                labels.Append("@" + dgvReview.CurrentRow.Cells["Measured Drop"].Value.ToString().TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Customer"].Value.ToString().TrimEnd());
                labels.Append("@"+  dgvReview.CurrentRow.Cells["Department"].Value.ToString().TrimEnd());
                if (dgvReview.CurrentRow.Cells["Track Colour"].Value.ToString().TrimEnd() == "OVEROLL")
                {
                    labels.Append("@O/R");
                }
                else
                {
                    labels.Append("@" + dgvReview.CurrentRow.Cells["Track Colour"].Value.ToString().TrimEnd());
                }
                labels.Append("@" + dgvReview.CurrentRow.Cells["Fabric Type"].Value.ToString().TrimEnd().Substring(0, dgvReview.CurrentRow.Cells["Fabric Type"].Value.ToString().TrimEnd().Length - 6).TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Fabric Colour"].Value.ToString().TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Trim Type"].Value.ToString().TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Pull Colour"].Value.ToString().TrimEnd() );//Added on 8-10-2016
               // labels.Append("@" + dgvReview.CurrentRow.Cells["Pull Colour"].Value.ToString().TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Alpha Number"].Value.ToString().TrimEnd() + "   " + dgvReview.CurrentRow.Cells["Cut Width"].Value.ToString().TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Barcode"].Value.ToString().TrimEnd());
                labels.Append("@" + dgvReview.CurrentRow.Cells["Control Side"].Value.ToString().TrimEnd() + " " + dgvReview.CurrentRow.Cells["Blind Number"].Value.ToString().TrimEnd()); //TODO: Suppy cut width here
                labels.Append("@" + dgvReview.CurrentRow.Cells["Total Blinds"].Value.ToString().TrimEnd());
                labels.Append("|");


                

                //sb.Append(dgvReview.CurrentRow.Cells["CB Number"].Value.ToString().TrimEnd() + "-" + dgvReview.CurrentRow.Cells["Line Number"].Value.ToString().TrimEnd() + ",");
                dgvReview.Rows.RemoveAt(0);

            }

            string labeldata = labels.ToString().TrimEnd('|');

            

            string filename = string.Format("{0:yyyy-MM-dd_hh-mm-ss}.txt", DateTime.Now);

            writefile(sb.ToString().TrimEnd(), newconfig.SaveFilesPath + filename, filename);

            //TODO:Call Print command for labels
           // StartLabelPrintApp(newconfig.Printer, labeldata);
            PrintLabels(newconfig.Printer + "|" + labeldata);
            lbTotalBlinds.Text = "." ;
        }

        private void insertLog(string cbNumber, string barCode, string tableNo, string uName, string datetime)
        {
            if (File.Exists(newconfig.LogPath + "Log.xls"))
            {
                using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source= " + @newconfig.LogPath + "Log.xls;Extended Properties='Excel 8.0;HDR=Yes'"))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand("Insert into [LogData$] ([CB Number],[Barcode],[Table Number],[Name],[Datetime]) values('" + cbNumber + "','" + barCode + "','" + tableNo + "','" + uName + "','"  + datetime + "')", conn);
                    cmd.ExecuteNonQuery();
                }
            }
        }


        private void writefile(string data, string filename, string file)
        {
            
            using (StreamWriter sw = new StreamWriter(filename))
            {
                sw.WriteLine(data);
               
            }
           
          
        }


        private void CreateData(DataRow dr, StringBuilder sb)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            while (dgvReview.Rows.Count > 0)
            {

                dgvReview.Rows.RemoveAt(0);

            }
            lbTotalBlinds.Text = ".";
        }

        private void PrintReport(string strNoCopy, string strPrinterName, string[] strParameterArray)
        {

            try
            {


                CrystalDecisions.Shared.ConnectionInfo crDbConnection = new CrystalDecisions.Shared.ConnectionInfo();
                int i = 0;


                CrystalDecisions.CrystalReports.Engine.ReportDocument oRpt = new CrystalDecisions.CrystalReports.Engine.ReportDocument();

                System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();

                System.Drawing.Printing.PaperSize pkSize = new System.Drawing.Printing.PaperSize();

                int rawKind = 0;

                oRpt.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)rawKind;
                oRpt.FileName = Application.StartupPath + "\\Printer Driver\\PrintLabel.rpt";
                oRpt.SetParameterValue("@CBNumber", strParameterArray[0]);
                oRpt.SetParameterValue("@Width", strParameterArray[1] + " mm");
                oRpt.SetParameterValue("@Drop", strParameterArray[2] + " mm");
                if (((strParameterArray[3]).ToString().Length > 23))
                {
                    
                    oRpt.SetParameterValue("@Customer", strParameterArray[3].ToString().Substring(0, 20));
                }
                else
                {
                    oRpt.SetParameterValue("@Customer", strParameterArray[3]);
                }

                if (((strParameterArray[4]).ToString().Length > 10))
                {
                    oRpt.SetParameterValue("@Department", strParameterArray[4].ToString().Substring(0, 10));
                }
                else
                {
                    oRpt.SetParameterValue("@Department", strParameterArray[4]);
                }

                oRpt.SetParameterValue("@Type", strParameterArray[5]);

                if (((strParameterArray[6]).ToString().Length > 12))
                {
                    oRpt.SetParameterValue("@Fabric", strParameterArray[6].ToString().Substring(0, 12));
                }
                else
                {
                    oRpt.SetParameterValue("@Fabric", strParameterArray[6]);
                }

                if (((strParameterArray[7]).ToString().Length > 12))
                {
                    oRpt.SetParameterValue("@Color", strParameterArray[7].ToString().Substring(0, 12));
                }
                else
                {
                    oRpt.SetParameterValue("@Color", strParameterArray[7]);
                }

                if (((strParameterArray[8]).ToString().Length > 6))
                {
                    oRpt.SetParameterValue("@ControlType", strParameterArray[8].ToString().Substring(0, 6));
                }
                else
                {
                    oRpt.SetParameterValue("@ControlType", strParameterArray[8]);
                }

                oRpt.SetParameterValue("@Lathe", strParameterArray[9]);
                oRpt.SetParameterValue("@Alpha", strParameterArray[10]);
                oRpt.SetParameterValue("@Barcode", strParameterArray[11]);
                oRpt.SetParameterValue("@LineNumber", strParameterArray[12]);
                oRpt.SetParameterValue("@Total", strParameterArray[13]);

                //oRpt.PrintOptions.PrinterName = "Auto Epson LX-300+ on DS22"
                oRpt.PrintOptions.PrinterName = strPrinterName;
                //DefaultPrinterName()
                i = 0;
                for (i = 1; i <= Convert.ToInt32(strNoCopy); i++)
                {
                    try
                    {
                        oRpt.PrintToPrinter(1, false, 0, 0);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void btnLabels_Click(object sender, EventArgs e)
        {
            //TODO Create File/Log
      
            StringBuilder labels = new StringBuilder();
            foreach (DataGridViewRow row in dgvReview.Rows)
            {
                
                labels.Append("@" + row.Cells["CB Number"].Value.ToString().TrimEnd());


                if (row.Cells["Trim Type"].Value.ToString().TrimEnd() == "FIN 36" && row.Cells["Control Type"].Value.ToString().TrimEnd() == "Motorised")
                {
                    labels.Append("@" + (Convert.ToInt32(row.Cells["Measured Width"].Value.ToString().TrimEnd()) + 5).ToString());

                }
                else
                {
                    labels.Append("@" + row.Cells["Measured Width"].Value.ToString().TrimEnd());
                }
                labels.Append("@" + row.Cells["Measured Drop"].Value.ToString().TrimEnd());
                labels.Append("@" + row.Cells["Customer"].Value.ToString().TrimEnd());
                labels.Append("@" + row.Cells["Department"].Value.ToString().TrimEnd());
                if (row.Cells["Track Colour"].Value.ToString().TrimEnd() == "OVEROLL")
                {
                    labels.Append("@O/R");
                }
                else
                {
                    labels.Append("@" + row.Cells["Track Colour"].Value.ToString().TrimEnd());
                }
                labels.Append("@" + row.Cells["Fabric Type"].Value.ToString().TrimEnd().Substring(0, row.Cells["Fabric Type"].Value.ToString().TrimEnd().Length - 6).TrimEnd());
                labels.Append("@" + row.Cells["Fabric Colour"].Value.ToString().TrimEnd());
                labels.Append("@" + row.Cells["Trim Type"].Value.ToString().TrimEnd());
               // labels.Append("@" + row.Cells["Pull Colour"].Value.ToString().TrimEnd() + " " + row.Cells["Control Side"].Value.ToString().TrimEnd());// Added on 08-10-2016

                labels.Append("@" + row.Cells["Pull Colour"].Value.ToString().TrimEnd());
                labels.Append("@" + row.Cells["Alpha Number"].Value.ToString().TrimEnd() + "   " + row.Cells["Cut Width"].Value.ToString().TrimEnd());
                labels.Append("@" + row.Cells["Barcode"].Value.ToString().TrimEnd());
                labels.Append("@" + row.Cells["Control Side"].Value.ToString().TrimEnd() + " " + row.Cells["Blind Number"].Value.ToString().TrimEnd()); //TODO: Supply Cut with here
                labels.Append("@" + row.Cells["Total Blinds"].Value.ToString().TrimEnd());
                labels.Append("|"); //Change the Next Set of Labels from Comma to Pipe

                 


                //sb.Append(dgvReview.CurrentRow.Cells["CB Number"].Value.ToString().TrimEnd() + "-" + dgvReview.CurrentRow.Cells["Line Number"].Value.ToString().TrimEnd() + ",");
                

            }

            string labeldata = labels.ToString().TrimEnd('|');

            int a;
           // StartLabelPrintApp(newconfig.Printer, labeldata);

            PrintLabels(newconfig.Printer + "|" + labeldata);

            //TODO:Call Print command for labels

        }

        private void PrintLabels(string strParameter)
        {
            string[] strpara = null;
            strpara = strParameter.Replace("|@","|").Split('|');

            //strpara = strParameter.Replace(",@", ",").Split(",");

        if ( strpara.GetUpperBound(0) == 0) 
            {
	        System.Environment.Exit(0);
            }
        else
        {

	    string strPrinterName = null;
	    strPrinterName = strpara[0];

	    for (int x = 1; x <= strpara.Length - 1; x++)
        {
		
            string[] strParameterArray = strpara[x].Split('@');

           // [] strParameterArray = strpara[x].Split("@");

		if (strParameterArray.GetUpperBound(0) == 0)
        {
			System.Environment.Exit(0);
		} 
        else 
        {
			PrintReport("1", strPrinterName, strParameterArray);
		}
	}

}

        }

        //Label Functions
        private void StartLabelPrintApp(string PrinterName,string LabelData)
        {
            Process myProcess = new Process();


            try
            {
                KillLabelPrintApp();

                myProcess.StartInfo.FileName = Application.StartupPath + "\\Printer Driver\\PrintLabel.exe";

                myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                myProcess.StartInfo.Arguments = PrinterName+ "," + LabelData;
               // myProcess.StartInfo.Verb = "runas";
                myProcess.Start();


            }
            catch (Exception e)
            {
                

                MessageBox.Show(e.ToString(), "Error");

            }

        }
        //Main

        public void KillLabelPrintApp()
        {

            try
            {

                if (Process.GetProcessesByName("PrintLabel").Length >= 1)
                {
                    Process[] pProcess = System.Diagnostics.Process.GetProcessesByName("PrintLabel");


                    foreach (Process p in pProcess)
                    {
                        p.Kill();

                    }

                    System.Threading.Thread.Sleep(100);

                }


            }
            catch (Exception ex)
            {
            }

        }





        private void tbCBNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (tbCBNumber.Text.Length > 0 && e.KeyCode.Equals(Keys.Enter))
            {
                GetButtonClick();

            }
        }

        private void cbsearch_TextChanged(object sender, EventArgs e)
        {
            if (cbsearch.SelectedIndex < 0)
            {
                cbsearch.Text = "Please, select search citeria";
            }
            else
            {
                cbsearch.Text = cbsearch.SelectedText;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            KillLabelPrintApp();

        }

        private void btnRollWidth_Click(object sender, EventArgs e)
        {
            if (dgvReview.Rows.Count > 0)
            {
                var t = new TextPrompt("Please enter the roll width:");
                t.ShowDialog();
                if (t.Value != "")
                {
                   //essageBox.Show(t.Value);

                    foreach (DataGridViewRow row in dgvReview.Rows)
                    {
                        row.Cells["Roll Width"].Value = t.Value;

                    }
                    dgvReview.Refresh();

                }
            }
            else
            {
                MessageBox.Show("No lines to update", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateALL();
        }
    }
}
