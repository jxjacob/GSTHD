using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    internal class TextBoxPlus : TextBox
    {
        Settings settings;

        public bool isBroadcastable;

        public TextBoxPlus(ObjectPointTextbox data, Settings settings, bool isOnBroadcast = false)
        {
            this.settings = settings;

            Text = data.Text;
            BackColor = data.BackColor;
            Name = data.Name;
            Font = new Font(data.FontName, data.FontSize, data.FontStyle);
            ForeColor = data.FontColor;
            Size = new Size(data.Width, data.Height);
            Location = new Point(data.X, data.Y);
            BorderStyle = data.BorderStyle;
            Padding = new Padding(5, 10, 5, 5);
            Margin = new Padding(5, 10, 5, 5);
            isBroadcastable = data.isBroadcastable;

            this.TextChanged += BoxPlus_TextChanged;
        }


        public void BoxPlus_TextChanged(object sender, EventArgs e)
        {
            //lol
            Push();
        }

        public void Push()
        {
            if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            {
                ((Label)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).Text = this.Text;
            }
        }
    }
}
