namespace TestFab
{
    partial class Configurator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.configPanel = new System.Windows.Forms.Panel();
            this.txtWidthDeduction = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbPrinterName = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLathe = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRollwidthSheet = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lbColumns = new System.Windows.Forms.Label();
            this.txtSavePath = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtSheetName = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtLogFilePath = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtEmailAddress = new System.Windows.Forms.TextBox();
            this.txtXlFilePath = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnFinish = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.configPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // configPanel
            // 
            this.configPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.configPanel.Controls.Add(this.txtWidthDeduction);
            this.configPanel.Controls.Add(this.label6);
            this.configPanel.Controls.Add(this.cbPrinterName);
            this.configPanel.Controls.Add(this.label3);
            this.configPanel.Controls.Add(this.txtLathe);
            this.configPanel.Controls.Add(this.label2);
            this.configPanel.Controls.Add(this.txtRollwidthSheet);
            this.configPanel.Controls.Add(this.label1);
            this.configPanel.Controls.Add(this.btnCancel);
            this.configPanel.Controls.Add(this.lbColumns);
            this.configPanel.Controls.Add(this.txtSavePath);
            this.configPanel.Controls.Add(this.label10);
            this.configPanel.Controls.Add(this.txtSheetName);
            this.configPanel.Controls.Add(this.label9);
            this.configPanel.Controls.Add(this.txtLogFilePath);
            this.configPanel.Controls.Add(this.label8);
            this.configPanel.Controls.Add(this.txtEmailAddress);
            this.configPanel.Controls.Add(this.txtXlFilePath);
            this.configPanel.Controls.Add(this.label7);
            this.configPanel.Controls.Add(this.label5);
            this.configPanel.Controls.Add(this.label4);
            this.configPanel.Controls.Add(this.btnFinish);
            this.configPanel.Controls.Add(this.dataGridView1);
            this.configPanel.Location = new System.Drawing.Point(1, 9);
            this.configPanel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.configPanel.Name = "configPanel";
            this.configPanel.Size = new System.Drawing.Size(813, 414);
            this.configPanel.TabIndex = 11;
            // 
            // txtWidthDeduction
            // 
            this.txtWidthDeduction.Location = new System.Drawing.Point(211, 336);
            this.txtWidthDeduction.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtWidthDeduction.Name = "txtWidthDeduction";
            this.txtWidthDeduction.Size = new System.Drawing.Size(236, 20);
            this.txtWidthDeduction.TabIndex = 38;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 336);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(146, 13);
            this.label6.TabIndex = 37;
            this.label6.Text = "Please width deduction sheet";
            // 
            // cbPrinterName
            // 
            this.cbPrinterName.FormattingEnabled = true;
            this.cbPrinterName.Location = new System.Drawing.Point(212, 301);
            this.cbPrinterName.Name = "cbPrinterName";
            this.cbPrinterName.Size = new System.Drawing.Size(235, 21);
            this.cbPrinterName.TabIndex = 36;
            this.cbPrinterName.SelectedIndexChanged += new System.EventHandler(this.cbPrinterName_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 304);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(122, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "Please Select the printer";
            // 
            // txtLathe
            // 
            this.txtLathe.Location = new System.Drawing.Point(212, 144);
            this.txtLathe.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtLathe.Name = "txtLathe";
            this.txtLathe.Size = new System.Drawing.Size(236, 20);
            this.txtLathe.TabIndex = 34;
            this.txtLathe.Leave += new System.EventHandler(this.txtLathe_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 146);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(170, 13);
            this.label2.TabIndex = 33;
            this.label2.Text = "Please Enter the file path for Lathe";
            // 
            // txtRollwidthSheet
            // 
            this.txtRollwidthSheet.Location = new System.Drawing.Point(212, 103);
            this.txtRollwidthSheet.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtRollwidthSheet.Name = "txtRollwidthSheet";
            this.txtRollwidthSheet.Size = new System.Drawing.Size(236, 20);
            this.txtRollwidthSheet.TabIndex = 32;
            this.txtRollwidthSheet.Leave += new System.EventHandler(this.txtRollwidthSheet_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 105);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(186, 13);
            this.label1.TabIndex = 31;
            this.label1.Text = "Please Enter the file path for Rollwidht";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(147, 366);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(106, 32);
            this.btnCancel.TabIndex = 30;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lbColumns
            // 
            this.lbColumns.AutoSize = true;
            this.lbColumns.Location = new System.Drawing.Point(19, 9);
            this.lbColumns.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbColumns.Name = "lbColumns";
            this.lbColumns.Size = new System.Drawing.Size(41, 13);
            this.lbColumns.TabIndex = 29;
            this.lbColumns.Text = "label14";
            // 
            // txtSavePath
            // 
            this.txtSavePath.Location = new System.Drawing.Point(211, 192);
            this.txtSavePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtSavePath.Name = "txtSavePath";
            this.txtSavePath.Size = new System.Drawing.Size(236, 20);
            this.txtSavePath.TabIndex = 14;
            this.txtSavePath.Leave += new System.EventHandler(this.txtSavePath_Leave);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(19, 194);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(179, 13);
            this.label10.TabIndex = 13;
            this.label10.Text = "Please Enter Path to Save input files";
            // 
            // txtSheetName
            // 
            this.txtSheetName.Location = new System.Drawing.Point(211, 68);
            this.txtSheetName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtSheetName.Name = "txtSheetName";
            this.txtSheetName.Size = new System.Drawing.Size(236, 20);
            this.txtSheetName.TabIndex = 7;
            this.txtSheetName.Leave += new System.EventHandler(this.txtSheetName_Leave);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 71);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(163, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "Please Enter the XL Sheet Name";
            // 
            // txtLogFilePath
            // 
            this.txtLogFilePath.Location = new System.Drawing.Point(211, 228);
            this.txtLogFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtLogFilePath.Name = "txtLogFilePath";
            this.txtLogFilePath.Size = new System.Drawing.Size(236, 20);
            this.txtLogFilePath.TabIndex = 15;
            this.txtLogFilePath.Leave += new System.EventHandler(this.txtLogFilePath_Leave);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 231);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(150, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Please Enter the Log File Path";
            // 
            // txtEmailAddress
            // 
            this.txtEmailAddress.Location = new System.Drawing.Point(211, 270);
            this.txtEmailAddress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtEmailAddress.Name = "txtEmailAddress";
            this.txtEmailAddress.Size = new System.Drawing.Size(236, 20);
            this.txtEmailAddress.TabIndex = 17;
            this.txtEmailAddress.Leave += new System.EventHandler(this.txtEmailAddress_Leave);
            // 
            // txtXlFilePath
            // 
            this.txtXlFilePath.Location = new System.Drawing.Point(211, 29);
            this.txtXlFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtXlFilePath.Name = "txtXlFilePath";
            this.txtXlFilePath.Size = new System.Drawing.Size(236, 20);
            this.txtXlFilePath.TabIndex = 6;
            this.txtXlFilePath.TextChanged += new System.EventHandler(this.txtXlFilePath_TextChanged);
            this.txtXlFilePath.Leave += new System.EventHandler(this.txtXlFilePath_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 270);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(183, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Please Enter the error Emails Address";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 32);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(145, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Please Enter the XL File Path";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(620, 20);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Select Column Values";
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(270, 366);
            this.btnFinish.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnFinish.Name = "btnFinish";
            this.btnFinish.Size = new System.Drawing.Size(106, 32);
            this.btnFinish.TabIndex = 20;
            this.btnFinish.Text = "Finish";
            this.btnFinish.UseVisualStyleBackColor = true;
            this.btnFinish.Click += new System.EventHandler(this.btnFinish_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(459, 37);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(338, 350);
            this.dataGridView1.TabIndex = 8;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView1_CellFormatting);
            // 
            // Configurator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 446);
            this.Controls.Add(this.configPanel);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Configurator";
            this.Text = "Configurator";
            this.Load += new System.EventHandler(this.Configurator_Load);
            this.Shown += new System.EventHandler(this.Configurator_Shown);
            this.configPanel.ResumeLayout(false);
            this.configPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel configPanel;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lbColumns;
        private System.Windows.Forms.TextBox txtSavePath;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSheetName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtLogFilePath;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtEmailAddress;
        private System.Windows.Forms.TextBox txtXlFilePath;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnFinish;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox txtLathe;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRollwidthSheet;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbPrinterName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtWidthDeduction;
        private System.Windows.Forms.Label label6;
    }
}