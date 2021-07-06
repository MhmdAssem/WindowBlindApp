namespace TestFab
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.gbLoginPanel = new System.Windows.Forms.GroupBox();
            this.btnConfig = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.cbTableNumber = new System.Windows.Forms.ComboBox();
            this.txtLoginName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gbSelection = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.cbsearch = new System.Windows.Forms.ComboBox();
            this.btnGetCB = new System.Windows.Forms.Button();
            this.tbColorSearch = new System.Windows.Forms.TextBox();
            this.tbCBNumber = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dgvCBSelector = new System.Windows.Forms.DataGridView();
            this.gbReview = new System.Windows.Forms.GroupBox();
            this.dgvReview = new System.Windows.Forms.DataGridView();
            this.gbAction = new System.Windows.Forms.GroupBox();
            this.lbTotalBlinds = new System.Windows.Forms.Label();
            this.btnRollWidth = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCreateFile = new System.Windows.Forms.Button();
            this.btnLabels = new System.Windows.Forms.Button();
            this.gbLoginPanel.SuspendLayout();
            this.gbSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCBSelector)).BeginInit();
            this.gbReview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReview)).BeginInit();
            this.gbAction.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbLoginPanel
            // 
            this.gbLoginPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLoginPanel.Controls.Add(this.btnConfig);
            this.gbLoginPanel.Controls.Add(this.btnLogin);
            this.gbLoginPanel.Controls.Add(this.cbTableNumber);
            this.gbLoginPanel.Controls.Add(this.txtLoginName);
            this.gbLoginPanel.Controls.Add(this.label2);
            this.gbLoginPanel.Controls.Add(this.label1);
            this.gbLoginPanel.Location = new System.Drawing.Point(12, 12);
            this.gbLoginPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbLoginPanel.Name = "gbLoginPanel";
            this.gbLoginPanel.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbLoginPanel.Size = new System.Drawing.Size(1451, 159);
            this.gbLoginPanel.TabIndex = 0;
            this.gbLoginPanel.TabStop = false;
            this.gbLoginPanel.Text = "Login";
            // 
            // btnConfig
            // 
            this.btnConfig.Location = new System.Drawing.Point(1235, 106);
            this.btnConfig.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnConfig.Name = "btnConfig";
            this.btnConfig.Size = new System.Drawing.Size(173, 38);
            this.btnConfig.TabIndex = 5;
            this.btnConfig.Text = "Configration";
            this.btnConfig.UseVisualStyleBackColor = true;
            this.btnConfig.Click += new System.EventHandler(this.btnConfig_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(996, 106);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(173, 38);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // cbTableNumber
            // 
            this.cbTableNumber.AutoCompleteCustomSource.AddRange(new string[] {
            "B4",
            "B5",
            "B6"});
            this.cbTableNumber.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.cbTableNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTableNumber.FormattingEnabled = true;
            this.cbTableNumber.Items.AddRange(new object[] {
            "B4",
            "B5",
            "B6"});
            this.cbTableNumber.Location = new System.Drawing.Point(757, 39);
            this.cbTableNumber.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbTableNumber.Name = "cbTableNumber";
            this.cbTableNumber.Size = new System.Drawing.Size(244, 24);
            this.cbTableNumber.TabIndex = 3;
            // 
            // txtLoginName
            // 
            this.txtLoginName.Location = new System.Drawing.Point(123, 39);
            this.txtLoginName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.txtLoginName.Name = "txtLoginName";
            this.txtLoginName.Size = new System.Drawing.Size(319, 22);
            this.txtLoginName.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(609, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "Table Number";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(56, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // gbSelection
            // 
            this.gbSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSelection.Controls.Add(this.button2);
            this.gbSelection.Controls.Add(this.button1);
            this.gbSelection.Controls.Add(this.cbsearch);
            this.gbSelection.Controls.Add(this.btnGetCB);
            this.gbSelection.Controls.Add(this.tbColorSearch);
            this.gbSelection.Controls.Add(this.tbCBNumber);
            this.gbSelection.Controls.Add(this.label3);
            this.gbSelection.Controls.Add(this.dgvCBSelector);
            this.gbSelection.Enabled = false;
            this.gbSelection.Location = new System.Drawing.Point(12, 187);
            this.gbSelection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbSelection.Name = "gbSelection";
            this.gbSelection.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbSelection.Size = new System.Drawing.Size(1451, 343);
            this.gbSelection.TabIndex = 1;
            this.gbSelection.TabStop = false;
            this.gbSelection.Text = "CB Selector";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1242, 24);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(166, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "View Original Document";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(639, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(143, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Select All";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbsearch
            // 
            this.cbsearch.AccessibleDescription = "";
            this.cbsearch.FormattingEnabled = true;
            this.cbsearch.Items.AddRange(new object[] {
            "Fabric Type",
            "Fabric Colour"});
            this.cbsearch.Location = new System.Drawing.Point(823, 23);
            this.cbsearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cbsearch.Name = "cbsearch";
            this.cbsearch.Size = new System.Drawing.Size(207, 24);
            this.cbsearch.TabIndex = 5;
            this.cbsearch.TextChanged += new System.EventHandler(this.cbsearch_TextChanged);
            // 
            // btnGetCB
            // 
            this.btnGetCB.Location = new System.Drawing.Point(475, 24);
            this.btnGetCB.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnGetCB.Name = "btnGetCB";
            this.btnGetCB.Size = new System.Drawing.Size(143, 23);
            this.btnGetCB.TabIndex = 4;
            this.btnGetCB.Text = "Get CB Details";
            this.btnGetCB.UseVisualStyleBackColor = true;
            this.btnGetCB.Click += new System.EventHandler(this.btnGetCB_Click);
            // 
            // tbColorSearch
            // 
            this.tbColorSearch.Location = new System.Drawing.Point(1041, 23);
            this.tbColorSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbColorSearch.Name = "tbColorSearch";
            this.tbColorSearch.Size = new System.Drawing.Size(173, 22);
            this.tbColorSearch.TabIndex = 3;
            this.tbColorSearch.TextChanged += new System.EventHandler(this.tbColorSearch_TextChanged);
            // 
            // tbCBNumber
            // 
            this.tbCBNumber.Location = new System.Drawing.Point(152, 25);
            this.tbCBNumber.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbCBNumber.Name = "tbCBNumber";
            this.tbCBNumber.Size = new System.Drawing.Size(289, 22);
            this.tbCBNumber.TabIndex = 2;
            this.tbCBNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbCBNumber_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 17);
            this.label3.TabIndex = 1;
            this.label3.Text = "Scan CB Number";
            // 
            // dgvCBSelector
            // 
            this.dgvCBSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCBSelector.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCBSelector.Location = new System.Drawing.Point(5, 66);
            this.dgvCBSelector.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvCBSelector.Name = "dgvCBSelector";
            this.dgvCBSelector.RowTemplate.Height = 24;
            this.dgvCBSelector.Size = new System.Drawing.Size(1437, 270);
            this.dgvCBSelector.TabIndex = 0;
            this.dgvCBSelector.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCBSelector_CellContentClick);
            // 
            // gbReview
            // 
            this.gbReview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbReview.Controls.Add(this.dgvReview);
            this.gbReview.Enabled = false;
            this.gbReview.Location = new System.Drawing.Point(12, 537);
            this.gbReview.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbReview.Name = "gbReview";
            this.gbReview.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbReview.Size = new System.Drawing.Size(1451, 343);
            this.gbReview.TabIndex = 2;
            this.gbReview.TabStop = false;
            this.gbReview.Text = "CB Review";
            // 
            // dgvReview
            // 
            this.dgvReview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvReview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReview.Location = new System.Drawing.Point(9, 21);
            this.dgvReview.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dgvReview.Name = "dgvReview";
            this.dgvReview.RowTemplate.Height = 24;
            this.dgvReview.Size = new System.Drawing.Size(1437, 316);
            this.dgvReview.TabIndex = 5;
            this.dgvReview.Visible = false;
            this.dgvReview.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvReview_CellContentClick);
            // 
            // gbAction
            // 
            this.gbAction.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbAction.Controls.Add(this.lbTotalBlinds);
            this.gbAction.Controls.Add(this.btnRollWidth);
            this.gbAction.Controls.Add(this.btnClear);
            this.gbAction.Controls.Add(this.btnCreateFile);
            this.gbAction.Controls.Add(this.btnLabels);
            this.gbAction.Enabled = false;
            this.gbAction.Location = new System.Drawing.Point(12, 889);
            this.gbAction.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbAction.Name = "gbAction";
            this.gbAction.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbAction.Size = new System.Drawing.Size(1451, 57);
            this.gbAction.TabIndex = 3;
            this.gbAction.TabStop = false;
            this.gbAction.Text = "Actions";
            // 
            // lbTotalBlinds
            // 
            this.lbTotalBlinds.AutoSize = true;
            this.lbTotalBlinds.Location = new System.Drawing.Point(1003, 24);
            this.lbTotalBlinds.Name = "lbTotalBlinds";
            this.lbTotalBlinds.Size = new System.Drawing.Size(12, 17);
            this.lbTotalBlinds.TabIndex = 4;
            this.lbTotalBlinds.Text = ".";
            // 
            // btnRollWidth
            // 
            this.btnRollWidth.Location = new System.Drawing.Point(748, 21);
            this.btnRollWidth.Name = "btnRollWidth";
            this.btnRollWidth.Size = new System.Drawing.Size(143, 23);
            this.btnRollWidth.TabIndex = 3;
            this.btnRollWidth.Text = "Update Roll Width";
            this.btnRollWidth.UseVisualStyleBackColor = true;
            this.btnRollWidth.Click += new System.EventHandler(this.btnRollWidth_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(558, 21);
            this.btnClear.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(143, 23);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCreateFile
            // 
            this.btnCreateFile.Location = new System.Drawing.Point(360, 21);
            this.btnCreateFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCreateFile.Name = "btnCreateFile";
            this.btnCreateFile.Size = new System.Drawing.Size(143, 23);
            this.btnCreateFile.TabIndex = 1;
            this.btnCreateFile.Text = "Create File/Labels";
            this.btnCreateFile.UseVisualStyleBackColor = true;
            this.btnCreateFile.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnLabels
            // 
            this.btnLabels.Location = new System.Drawing.Point(152, 21);
            this.btnLabels.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLabels.Name = "btnLabels";
            this.btnLabels.Size = new System.Drawing.Size(143, 23);
            this.btnLabels.TabIndex = 0;
            this.btnLabels.Text = "Print Labels Only";
            this.btnLabels.UseVisualStyleBackColor = true;
            this.btnLabels.Click += new System.EventHandler(this.btnLabels_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1543, 964);
            this.Controls.Add(this.gbAction);
            this.Controls.Add(this.gbReview);
            this.Controls.Add(this.gbSelection);
            this.Controls.Add(this.gbLoginPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Fabric Cutting Planner V2.1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.gbLoginPanel.ResumeLayout(false);
            this.gbLoginPanel.PerformLayout();
            this.gbSelection.ResumeLayout(false);
            this.gbSelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCBSelector)).EndInit();
            this.gbReview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvReview)).EndInit();
            this.gbAction.ResumeLayout(false);
            this.gbAction.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLoginPanel;
        private System.Windows.Forms.ComboBox cbTableNumber;
        private System.Windows.Forms.TextBox txtLoginName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConfig;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.GroupBox gbSelection;
        private System.Windows.Forms.GroupBox gbReview;
        private System.Windows.Forms.GroupBox gbAction;
        private System.Windows.Forms.TextBox tbColorSearch;
        private System.Windows.Forms.TextBox tbCBNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dgvCBSelector;
        private System.Windows.Forms.Button btnGetCB;
        private System.Windows.Forms.DataGridView dgvReview;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnCreateFile;
        private System.Windows.Forms.Button btnLabels;
        private System.Windows.Forms.ComboBox cbsearch;
        private System.Windows.Forms.Button btnRollWidth;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lbTotalBlinds;
        private System.Windows.Forms.Button button2;
    }
}

