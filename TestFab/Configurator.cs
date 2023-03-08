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

namespace TestFab
{
    public partial class Configurator : Form
    {
        Config _newconfig = new Config();
        static string ConfigFileName;
        DataGridViewButtonColumn selectButton;

        public Configurator(Config newConfig,string _ConfigFileName)
        {
            _newconfig = newConfig;
            ConfigFileName = _ConfigFileName;
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        public void SetConfigOptions()
        {
            if (_newconfig.FilePath != null && _newconfig.SheetNameValue != null)
            {
                txtXlFilePath.Text = _newconfig.FilePath;
                txtLogFilePath.Text = _newconfig.LogPath;
                txtLathe.Text = _newconfig.Lathe;
                txtRollwidthSheet.Text = _newconfig.RollWidth;
                cbPrinterName.Text = _newconfig.Printer;
               // txtCounter.Text = _newconfig.CounterValue.ToString();
                txtEmailAddress.Text = _newconfig.ToEmailAddress;
                txtSheetName.Text = _newconfig.SheetNameValue;
                txtSavePath.Text = _newconfig.SaveFilesPath;
                txtWidthDeduction.Text = _newconfig.WidthDeduct;

               // txtFTPSite.Text = _newconfig.ftpSiteName;
              //  txtFTPUsername.Text = _newconfig.FTPSiteUsername;
              //  txtFTPPassword.Text = _newconfig.FTPSitepassword;

                if (_newconfig.Columns != "")
                {
                    if (dataGridView1.Rows.Count <= 0)
                    {
                        LoadColumnName();
                    }
                    dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
                    dataGridView1.AutoResizeColumns();
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    lbColumns.Text = "Columns Selected " + _newconfig.Columns.Replace("[", "").Replace("]", "");
                }

            }

           
            configPanel.Visible = true;


        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            //(txtRollwidthSheet.Text = "")
            if ( (txtEmailAddress.Text == "") || (txtLogFilePath.Text == "") || (txtRollwidthSheet.Text =="") || (txtLathe.Text =="") || (txtSavePath.Text == "") || (txtSheetName.Text == "") || (txtXlFilePath.Text == "") || (txtWidthDeduction.Text == ""))
            {
                MessageBox.Show("Please Fill all the Details", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else
            {
                WriteXML();
                this.Dispose();
                //workingPanel.Visible = true;
                //configPanel.Visible = false;
                //textBox1.Focus();
            }
        }

        private void WriteXML()
        {
            XmlTextWriter xw = new XmlTextWriter(ConfigFileName, System.Text.Encoding.UTF8);
            xw.Formatting = Formatting.Indented;
            xw.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            xw.WriteStartElement("ConfigDetails");
            xw.WriteStartElement("XLFilePath");
            xw.WriteString(_newconfig.FilePath);
            xw.WriteEndElement();

            xw.WriteStartElement("LogFilePath");
            xw.WriteString(_newconfig.LogPath);
            xw.WriteEndElement();

            xw.WriteStartElement("RollwidhtPath");
            xw.WriteString(_newconfig.RollWidth);
            xw.WriteEndElement();

            xw.WriteStartElement("LathePath");
            xw.WriteString(_newconfig.Lathe);
            xw.WriteEndElement();


            xw.WriteStartElement("Counter");
            xw.WriteString(_newconfig.CounterValue.ToString());
            xw.WriteEndElement();

            xw.WriteStartElement("SelectColumns");
            xw.WriteString(_newconfig.Columns);
            xw.WriteEndElement();

            xw.WriteStartElement("ToEmailAddress");
            xw.WriteString(_newconfig.ToEmailAddress);
            xw.WriteEndElement();

            xw.WriteStartElement("SheetName");
            xw.WriteString(_newconfig.SheetNameValue);
            xw.WriteEndElement();


            xw.WriteStartElement("SaveFilesPath");
            xw.WriteString(_newconfig.SaveFilesPath);
            xw.WriteEndElement();

            xw.WriteStartElement("FTPSiteName");
            xw.WriteString(_newconfig.ftpSiteName);
            xw.WriteEndElement();

            xw.WriteStartElement("Printer");
            xw.WriteString(_newconfig.Printer);
            xw.WriteEndElement();

            xw.WriteStartElement("WidthDeduction");
            xw.WriteString(_newconfig.WidthDeduct);
            xw.WriteEndElement();

            xw.WriteStartElement("FTPUName");
            xw.WriteString(_newconfig.FTPSiteUsername);
            xw.WriteEndElement();

            xw.WriteStartElement("FTPPassword");
            xw.WriteString(_newconfig.FTPSitepassword);
            xw.WriteEndElement();


            xw.WriteEndElement();

            xw.Close();
        }

        private void txtXlFilePath_Leave(object sender, EventArgs e)
        {
            if (txtXlFilePath.Text != "")
            {
                //FileName = @txtXlFilePath.Text;
                if (System.IO.File.Exists(@txtXlFilePath.Text))
                {
                    _newconfig.FilePath = @txtXlFilePath.Text;

                    txtSheetName.Focus();

                }
                else
                {
                    MessageBox.Show("File Not Found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            else
            {
                MessageBox.Show("Xl File path cannot be blank", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtXlFilePath.Focus();

            }
        }

        private void txtSheetName_Leave(object sender, EventArgs e)
        {
            if (txtSheetName.Text != "")
            {

                if (dataGridView1.Rows.Count <= 0)
                {
                    LoadColumnName();
                }
            }
            else
            {
                MessageBox.Show("Sheet Name cannot be blank", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtSheetName.Focus();

            }

        }

        private void LoadColumnName()
        {
            string Con_Str;
            int i = 0;
            Con_Str = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _newconfig.FilePath +
                                        ";Mode=ReadWrite;Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"";
            _newconfig.SheetNameValue = txtSheetName.Text;

            OleDbConnection oleDb = new OleDbConnection(Con_Str);

            oleDb.Open();
            //  OleDbCommand cm = new OleDbCommand("select * from [Sheet1$] WHERE FON=" + Convert.ToDouble(textBox1.Text), connection);
            object[] arrRestrict;
            //ctbsodump
            arrRestrict = new object[] { null, null, txtSheetName.Text + "$", null };
            DataTable tblDbSchema = oleDb.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, arrRestrict);
            oleDb.Close();

            for (i = 0; i <= tblDbSchema.Rows.Count - 1; i++)
            {
                tblDbSchema.Rows[i]["Column_Name"] = tblDbSchema.Rows[i]["Column_Name"].ToString().TrimEnd();
            }

            dataGridView1.DataSource = tblDbSchema;


            for (i = 0; i <= dataGridView1.Columns.Count - 1; i++)
            {
                dataGridView1.Columns[i].Visible = false;
            }

            dataGridView1.Columns["Column_Name"].Visible = true;

            // insert edit button into datagridview
            dataGridView1.ColumnHeadersHeight = 50;
            selectButton = new DataGridViewButtonColumn();
            selectButton.HeaderText = "Select";
            selectButton.Text = "Select";
            selectButton.UseColumnTextForButtonValue = true;
            selectButton.Width = 80;
            dataGridView1.Columns.Add(selectButton);
            dataGridView1.AllowUserToAddRows = false;

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex] == selectButton)
            {
                if (dataGridView1.CurrentRow.DefaultCellStyle.BackColor != System.Drawing.Color.Yellow)
                {
                    dataGridView1.CurrentRow.DefaultCellStyle.BackColor = System.Drawing.Color.Yellow;

                    _newconfig.Columns = (_newconfig.Columns + "," + "[" + dataGridView1.CurrentRow.Cells["Column_Name"].Value.ToString() + "]");
                    //+ "," + newconfig.Columns

                    _newconfig.Columns = _newconfig.Columns.TrimEnd(',');
                    _newconfig.Columns = _newconfig.Columns.TrimStart(',');
                    lbColumns.Text = "Columns Selected " + _newconfig.Columns.Replace("[", "").Replace("]", "");
                }
                else
                {
                    dataGridView1.CurrentRow.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                    _newconfig.Columns = _newconfig.Columns.Replace(",[" + dataGridView1.CurrentRow.Cells["Column_Name"].Value.ToString() + "]", "");
                    _newconfig.Columns = _newconfig.Columns.TrimEnd(',');
                    _newconfig.Columns = _newconfig.Columns.TrimStart(',');
                    lbColumns.Text = "Columns Selected " + _newconfig.Columns.Replace("[", "").Replace("]", "");
                }


            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int i;
            //dataGridView1.CurrentRow.DefaultCellStyle.BackColor = Color.Yellow;
            if (_newconfig.Columns != null)
            {
                for (i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    if (((_newconfig.Columns.Replace("[", "").Replace("]", "")).Contains(dataGridView1.Rows[i].Cells["Column_Name"].Value.ToString())))
                    {
                        // '  this.dataGridView1.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.Yellow;

                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;

                    }
                }
            }
        }

        private void Configurator_Load(object sender, EventArgs e)
        {
           
        }

        private void Configurator_Shown(object sender, EventArgs e)
        {
            SetConfigOptions();
            LoadPrinters();
        }

        private void LoadPrinters()
        {
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
              //  MessageBox.Show(printer);
                cbPrinterName.Items.Add(printer);
            }
        }

        private void txtSavePath_Leave(object sender, EventArgs e)
        {
            if (txtSavePath.Text != "")
            {
                if (System.IO.Directory.Exists(@txtSavePath.Text))
                {


                    _newconfig.SaveFilesPath = txtSavePath.Text;
                }
                else
                {
                    MessageBox.Show("File path dont exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please provide the path to save input files", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void txtLogFilePath_Leave(object sender, EventArgs e)
        {
            if (txtLogFilePath.Text != "")
            {

                if (System.IO.Directory.Exists(@txtLogFilePath.Text))
                {

                    _newconfig.LogPath = txtLogFilePath.Text;
                }
                else
                {
                    MessageBox.Show("File path dont exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Log file path cannot be Blank", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void txtEmailAddress_Leave(object sender, EventArgs e)
        {
            if (txtEmailAddress.Text != "")
            {
                if (Email.isValid(txtEmailAddress.Text))
                {
                    _newconfig.ToEmailAddress = txtEmailAddress.Text;
                }
                else
                {
                    MessageBox.Show("Email address is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {

                MessageBox.Show("Email address is required to send errors", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtRollwidthSheet_Leave(object sender, EventArgs e)
        {
            if (txtRollwidthSheet.Text != "")
            {

                if (System.IO.File.Exists(@txtRollwidthSheet.Text))
                {

                    _newconfig.RollWidth = txtRollwidthSheet.Text;
                }
                else
                {
                    MessageBox.Show("File  dont exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("file path cannot be Blank", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtLathe_Leave(object sender, EventArgs e)
        {
            if (txtLathe.Text != "")
            {

                if (System.IO.File.Exists(@txtLathe.Text))
                {

                    _newconfig.Lathe = txtLathe.Text;
                }
                else
                {
                    MessageBox.Show("File  dont exists", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("file path cannot be Blank", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbPrinterName_SelectedIndexChanged(object sender, EventArgs e)
        {
            _newconfig.Printer = cbPrinterName.Text;
        }

        private void txtXlFilePath_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
