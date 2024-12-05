using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    public partial class progresswindow : Form
    {
        public progresswindow(string windowtext, int barMax)
        {
            InitializeComponent();
            waitinglabel.Text = windowtext;
            connectProgress.Maximum = barMax;
        }

        private void progresswindow_Load(object sender, EventArgs e)
        {

        }

        public void incBar()
        {
            connectProgress.Value += 1;
        }

    }
}
