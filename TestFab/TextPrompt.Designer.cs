namespace TestFab
{
    partial class TextPrompt
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
            this.tbText = new System.Windows.Forms.TextBox();
            this.BtnSubmitText = new System.Windows.Forms.Button();
            this.lblPromptText = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbText
            // 
            this.tbText.Location = new System.Drawing.Point(15, 62);
            this.tbText.Name = "tbText";
            this.tbText.Size = new System.Drawing.Size(146, 22);
            this.tbText.TabIndex = 0;
            this.tbText.TextChanged += new System.EventHandler(this.tbText_TextChanged);
            // 
            // BtnSubmitText
            // 
            this.BtnSubmitText.Location = new System.Drawing.Point(15, 102);
            this.BtnSubmitText.Name = "BtnSubmitText";
            this.BtnSubmitText.Size = new System.Drawing.Size(75, 23);
            this.BtnSubmitText.TabIndex = 1;
            this.BtnSubmitText.Text = "Submit";
            this.BtnSubmitText.UseVisualStyleBackColor = true;
            this.BtnSubmitText.Click += new System.EventHandler(this.BtnSubmitText_Click);
            // 
            // lblPromptText
            // 
            this.lblPromptText.AutoSize = true;
            this.lblPromptText.Location = new System.Drawing.Point(12, 29);
            this.lblPromptText.Name = "lblPromptText";
            this.lblPromptText.Size = new System.Drawing.Size(46, 17);
            this.lblPromptText.TabIndex = 2;
            this.lblPromptText.Text = "label1";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(140, 102);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // TextPrompt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 157);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblPromptText);
            this.Controls.Add(this.BtnSubmitText);
            this.Controls.Add(this.tbText);
            this.Name = "TextPrompt";
            this.Text = "Roll Width";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbText;
        private System.Windows.Forms.Button BtnSubmitText;
        private System.Windows.Forms.Label lblPromptText;
        private System.Windows.Forms.Button btnCancel;
    }
}