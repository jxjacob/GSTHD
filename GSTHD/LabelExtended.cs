using System;
using Avalonia.Controls;
using Avalonia.Media;
using Color = Avalonia.Media.Color;


namespace GSTHD
{
    
    public class LabelOneTimeThing : TextBlock
    {
        protected override Type StyleKeyOverride => typeof(TextBlock);
        // overload class JUST for song display
        // protected override void OnPaint(PaintEventArgs e)
        // {
        //     var flags = TextFormatFlags.NoPrefix;
        //     TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, flags);
        // }
    }
    public class LabelExtended : TextBlock, IAlternatableObject
    {
        protected override Type StyleKeyOverride => typeof(TextBlock);
        
        // protected override void OnPaint(render e)
        // {
        //     var flags = TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.WordEllipsis | TextFormatFlags.NoPrefix;
        //     TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, ForeColor, flags);
        // }
        public LabelExtended()
        {
            
        }
        
        public LabelExtended(GenericLabel data)
        {
            Name = data.Name;
            IsVisible = data.Visible;
            Text = data.Text;
            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            FontFamily = data.FontName;
            FontSize = data.FontSize * 1.4;
            FontStyleFix(data.FontStyle);
            Foreground = new SolidColorBrush(Color.Parse(data.Color));
            // AutoSize = (data.TextAlignment == ContentAlignment.TopLeft);
            TextAlignment = LegacyConverters.ConvetTA(data.TextAlignment);
            // Width = data.Width;
        }
        
        
        public void SetVisible(bool visible)
        {
            this.IsVisible = visible;
        }

        private void FontStyleFix(FontStyle style)
        {
            // sucks that this is manual but what you gonna do?
            switch (style)
            {
                case GSTHD.FontStyle.Bold:
                    FontWeight = FontWeight.Bold;
                    FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
                case GSTHD.FontStyle.Italic:
                    FontWeight = FontWeight.Normal;
                    FontStyle = Avalonia.Media.FontStyle.Italic;
                    break;
                case GSTHD.FontStyle.Regular:
                    FontWeight = FontWeight.Normal;
                    FontStyle = Avalonia.Media.FontStyle.Normal;
                    break;
            }
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            var point = (GenericLabel)ogPoint;
            switch (name)
            {
                case "FontName":
                    if (mult > 0) FontFamily = value.ToString();
                    else FontFamily = (string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                    break;
                case "FontSize":
                    FontSize += (mult * int.Parse(value.ToString()));
                    break;
                case "FontStyle":
                    if (mult > 0) FontStyleFix(Enum.Parse<FontStyle>(value.ToString()));
                    else FontStyleFix((FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
                    break;
                default:
                    throw new NotImplementedException($"Could not perform Label Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates() { }
    }
    
}
