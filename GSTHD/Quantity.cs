using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GSTHD
{
    public struct QuantityState
    {
        public string WotHText;
        public int ColourIndex;
        public int CICount;
        public string BoxText;


        public override string ToString()
        {
            return $"{WotHText},{ColourIndex},{CICount},{BoxText}";
        }
    }

    class Quantity
    {
        public Settings Settings;

        public LabelExtended LabelPlace;
        public CollectedItem counterCI;
        public TextBox textBox;
        public string Name;

        private Color[] Colors;
        private int ColorIndex;
        private int MinIndex;

        public Quantity(Settings settings,
            string selectedPlace,
            int counterFontSize, int counterSpacing, string counterImageName,
            Size subBoxSize, int subFontSize, Color subBackColor, Color subFontColor,
            Point lastLabelLocation, Label labelSettings, Size gossipStoneSize, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool PathCycling)
        {
            Settings = settings;
            Name = selectedPlace;

            var labelStartX = 0;

            int panelWidth = labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;

            var gossipStoneStartX = panelWidth - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;

            LabelPlace = new LabelExtended
            {
                Name = Guid.NewGuid().ToString(),
                Text = selectedPlace,
                ForeColor = labelSettings.ForeColor,
                BackColor = labelSettings.BackColor,
                Font = labelSettings.Font,
                Width = labelWidth,
                Height = labelSettings.Height,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
            };
            LabelPlace.Location = new Point(labelStartX, lastLabelLocation.Y + LabelPlace.Height);
            LabelPlace.MouseDown += new MouseEventHandler(Mouse_ClickDown);



            ObjectPointCollectedItem tempOPCI = new ObjectPointCollectedItem();
            tempOPCI.Size = gossipStoneSize;
            tempOPCI.LabelFontSize = counterFontSize;
            tempOPCI.ImageCollection = new[] { counterImageName };
            tempOPCI.CountPosition =  new Size(6, 10);
            tempOPCI.LabelFontName = LabelPlace.Font.Name;
            tempOPCI.LabelColor = Color.White;
            tempOPCI.hasSlash = true;
            tempOPCI.BackColor = subBackColor;

            Debug.WriteLine("making an CI");
            CollectedItem newCI = new CollectedItem(tempOPCI, Settings, isBroadcastable);
            newCI.Location =
                new Point(gossipStoneStartX + (newCI.Width + counterSpacing)*0, LabelPlace.Location.Y);
            Debug.WriteLine(newCI.Location);
            counterCI = newCI;

            Debug.WriteLine("rmaking a label");
            TextBox newTextBox = new TextBox()
            {
                BackColor = subBackColor,
                Name = selectedPlace + "_box",
                Font = labelSettings.Font,
                ForeColor = subFontColor,
                Size = subBoxSize,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5, 10, 5, 5),
                Margin = new Padding(5, 10, 5, 5)
            };
            newTextBox.Location =
                new Point(gossipStoneStartX + (newCI.Width + counterSpacing), LabelPlace.Location.Y);
            Debug.WriteLine(newTextBox.Location);
            textBox = newTextBox;


            Colors = new Color[Settings.DefaultWothColors.Length + 1];
            for (int i = 0; i < Settings.DefaultWothColors.Length; i++)
            {
                Colors[i + 1] = Color.FromName(Settings.DefaultWothColors[i]);
            }
            ColorIndex = Settings.DefaultWothColorIndex + 1;
            UpdateFromSettings();
        }

        ~Quantity()
        {
            Debug.WriteLine("Quantity " + this.Name + " being killed");
        }

        public void UpdateFromSettings()
        {
            MinIndex = Settings.EnableLastWoth ? 0 : 1;
            Colors[0] = Color.FromKnownColor(Settings.LastWothColor);
            UpdateColor();
        }

        public void SetColor(int color)
        {
            ColorIndex = color;
            UpdateColor();
        }

        public void UpdateColor()
        {
            LabelPlace.ForeColor = Colors[ColorIndex];
        }

        private void Mouse_ClickDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ColorIndex < Colors.Length - 1)
            {
                ColorIndex++;
            }
            else if (e.Button == MouseButtons.Right && ColorIndex > MinIndex)
            {
                ColorIndex--;
            }
            UpdateColor();
        }

        public QuantityState SaveState()
        {
            return new QuantityState()
            {
                WotHText = Name,
                ColourIndex = ColorIndex,
                CICount = counterCI.CollectedItems,
                BoxText = textBox.Text
            };
        }
    }
}

