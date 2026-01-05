using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    public class LabelOneTimeThing : Label
    {
        // overload class JUST for song display
        protected override void OnPaint(PaintEventArgs e)
        {
            var flags = TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, flags);
        }
    }
    public class LabelExtended : Label, IAlternatableObject
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            var flags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis | TextFormatFlags.NoPrefix;
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, flags);
        }
        public void SetVisible(bool visible)
        {
            this.Visible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (GenericLabel)ogPoint;
            switch (name)
            {
                case "FontName":
                    if (mult > 0) Font = new Font(value.ToString(), Font.Size, Font.Style);
                    else Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), Font.Size, Font.Style);
                    break;
                case "FontSize":
                    Font = new Font(Font.Name, Font.Size + (mult * int.Parse(value.ToString())), Font.Style);
                    break;
                case "FontStyle":
                    if (mult > 0) Font = new Font(Font.FontFamily, Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
                    else Font = new Font(Font.FontFamily, Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Label Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates() { }
    }
    
}
