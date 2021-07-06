using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestFab
{
    public partial class TextPrompt : Form
    {
        public string Value
        {
            get { return tbText.Text.Trim(); }
        }

        public TextPrompt(string promptInstructions)
        {
            InitializeComponent();

            lblPromptText.Text = promptInstructions;
        }

        private void BtnSubmitText_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void TextPrompt_Load(object sender, EventArgs e)
        {
            CenterToParent();
        }

        private void tbText_TextChanged(object sender, EventArgs e)
        {
            string tString = tbText.Text;
            if (tString.Trim() == "") return;
            for (int i = 0; i < tString.Length; i++)
            {
                if (!char.IsNumber(tString[i]))
                {
                    MessageBox.Show("Please enter a valid number");
                    tbText.Text = "";
                    return;
                }

            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            tbText.Text = "";

            Close();
        }
    }
}
