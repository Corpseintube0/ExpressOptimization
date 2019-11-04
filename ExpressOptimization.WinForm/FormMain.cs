using System;
using System.Windows.Forms;
using ExpressOptimization.Library;

namespace ExpressOptimization.WinForm
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private readonly DerivativeTaker _dt = new DerivativeTaker();

        private void buttonDerive_Click(object sender, EventArgs e)
        {
            textBoxResult.Text = _dt.Derivation(textBoxFunc.Text, "x");
        }

    }
}
