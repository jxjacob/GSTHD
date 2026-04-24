using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace GSTHD
{
    internal class TextBoxPlus : TextBox, IAlternatableObject
    {
        protected override Type StyleKeyOverride => typeof(TextBox);
        
        Settings settings;

        public bool isBroadcastable { get; set; }

        public TextBoxPlus(ObjectPointTextbox data, Settings settings, bool isOnBroadcast = false)
        {
            this.settings = settings;
            IsVisible = data.Visible;
            
            Text = data.Text;
            Background = new SolidColorBrush(new Avalonia.Media.Color(data.BackColor.A,  data.BackColor.R, data.BackColor.G, data.BackColor.B));
            Name = data.Name;
            FontFamily = data.FontName;
            FontSize = data.FontSize * 1.4;
            FontStyleFix(data.FontStyle);
            Foreground = new SolidColorBrush(new Avalonia.Media.Color(data.FontColor.A,  data.FontColor.R, data.FontColor.G, data.FontColor.B));
            Width = data.Width;
            Height = data.Height;
            MinHeight = 0;
            MinWidth = 0;
            CornerRadius = new CornerRadius(0);
            Canvas.SetTop(this, data.Y);
            Canvas.SetLeft(this, data.X);
            BorderBrush = new SolidColorBrush(Avalonia.Media.Color.FromRgb(100,100,100));
            BorderThickness = new Thickness(1);
            // BorderStyle = data.BorderStyle;
            Padding = new Thickness(3, 3, 1, 1);
            //Margin = new Thickness(5, 10, 5, 5);
            isBroadcastable = !isOnBroadcast && data.isBroadcastable;
            TextAlignment = LegacyConverters.ConvetTA(data.TextAlignment);
            ZIndex += 1;

            TextChanged += BoxPlus_TextChanged;
        }


        public void BoxPlus_TextChanged(object? sender, EventArgs e)
        {
            //lol
            Push();
        }

        public void Push()
        {
            // if (isBroadcastable && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            // {
            //     ((Label)Application.OpenForms["GSTHD_DK64 Broadcast View"].Controls.Find(this.Name, true)[0]).Text = this.Text;
            // }
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
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
                // TODO: restore
                // case "FontColor":
                //     if (mult > 0) ForeColor = Color.FromName(value.ToString());
                //     else ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                //     break;
                default:
                    throw new NotImplementedException($"Could not perform Textbox Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            }
        }

        public void ConfirmAlternates() { }
    }
}
