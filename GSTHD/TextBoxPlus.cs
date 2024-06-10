using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    internal class TextBoxPlus : TextBox, IAlternatableObject
    {
        Settings settings;

        public bool isBroadcastable { get; set; }

        public TextBoxPlus(ObjectPointTextbox data, Settings settings, bool isOnBroadcast = false)
        {
            this.settings = settings;
            Visible = data.Visible;

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
            TextAlign = data.TextAlignment;
            // for reasons unknown to man and machine, height=19 doesnt actually work with autosize=false
            // i WILL find who is responsible for this bug and lobotomize them
            AutoSize = (Height == 19 && BorderStyle == BorderStyle.FixedSingle);

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

        public void SetVisible(bool visible)
        {
            Visible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            switch (name)
            {
                case "FontName":
                    if (mult > 0) Font = new Font(value.ToString(), Font.Size, Font.Style);
                    else Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), Font.Size, Font.Style);
                    break;
                case "FontSize":
                    if (mult > 0) Font = new Font(Font.Name, int.Parse(value.ToString()), Font.Style);
                    else Font = new Font(Font.Name, (int)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), Font.Style);
                    break;
                case "FontStyle":
                    if (mult > 0) Font = new Font(Font.FontFamily, Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
                    else Font = new Font(Font.FontFamily, Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                case "FontColor":
                    if (mult > 0) ForeColor = Color.FromName(value.ToString());
                    else ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Textbox Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }
    }
}
