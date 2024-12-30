using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ImageList;


namespace GSTHD
{
    public struct QuantityState
    {
        public string WotHText;
        public int ColourIndex;
        public int CILCount;
        public int CIRCount;


        public override string ToString()
        {
            return $"{WotHText},{ColourIndex},{CILCount},{CIRCount}";
        }
    }

    class Quantity : PanelHint
    {
        public Settings Settings;

        public LabelExtended LabelPlace { get; set; }
        public CollectedItem leftCounterCI;
        public CollectedItem rightCounterCI;
        public string Name { get; set; }

        private Color[] Colors;
        private int ColorIndex;
        private int MinIndex;

        public Quantity(Settings settings,
            string selectedPlace,
            int counterFontSize, int counterSpacing, string counterImageName,
            Size subBoxSize, Color subBackColor, 
            Point lastLabelLocation, Label labelSettings, Size gossipStoneSize, bool isBroadcastable)
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



            ObjectPointCollectedItem tempOPCI = new ObjectPointCollectedItem
            {
                Size = gossipStoneSize,
                LabelFontSize = counterFontSize,
                ImageCollection = new[] { counterImageName },
                CountPosition = new Size(6, counterFontSize - (counterFontSize/10)),
                LabelFontName = LabelPlace.Font.Name,
                LabelColor = labelSettings.ForeColor,
                hasSlash = true,
                BackColor = subBackColor,
                BackGroundColor = subBackColor,
                // note: quantity/woth/barren are NOT controls, so visibility might need to be passed from somewhere else
                Visible = true,
            };

            //Debug.WriteLine("making left CI");
            leftCounterCI = new CollectedItem(tempOPCI, Settings, isBroadcastable)
            {
                Location = new Point(gossipStoneStartX, LabelPlace.Location.Y)
            };

            //Debug.WriteLine(leftCounterCI.Location);

            // Debug.WriteLine("rmaking right CI");
            tempOPCI.hasSlash = false;
            rightCounterCI = new CollectedItem(tempOPCI, Settings, isBroadcastable)
            {
                Location = new Point(gossipStoneStartX + (leftCounterCI.Width + counterSpacing), LabelPlace.Location.Y)
            };

            //Debug.WriteLine(rightCounterCI.Location);


            Colors = new Color[Settings.DefaultWothColors.Length + 1];
            for (int i = 0; i < Settings.DefaultWothColors.Length; i++)
            {
                Colors[i + 1] = Color.FromName(Settings.DefaultWothColors[i]);
            }
            ColorIndex = Settings.DefaultWothColorIndex + 1;
            UpdateFromSettings();
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
            leftCounterCI.SetColor(Colors[ColorIndex]);
            rightCounterCI.SetColor(Colors[ColorIndex]);
        }

        private void Mouse_ClickDown(object sender, MouseEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton))
            {
                if (ColorIndex < Colors.Length - 1) ColorIndex++;
                else if (Settings.WraparoundItems) ColorIndex = MinIndex;
            }
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton))
            {
                if (ColorIndex > MinIndex) ColorIndex--;
                else if (Settings.WraparoundItems) ColorIndex = Colors.Length - 1;
            }
            UpdateColor();
        }

        public QuantityState SaveState()
        {
            return new QuantityState()
            {
                WotHText = Name,
                ColourIndex = ColorIndex,
                CILCount = leftCounterCI.CollectedItems,
                CIRCount = rightCounterCI.CollectedItems
            };
        }

        public void RefreshLocation(int counterFontSize, int counterSpacing, string counterImage,
                        Size subBoxSize, Color ackColor,
                        int LabelLastHeight, Label labelSettings, Size gossipStoneSize)
        {


            var labelStartX = 0;

            int panelWidth = labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;

            var gossipStoneStartX = panelWidth - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;


            LabelPlace.ForeColor = labelSettings.ForeColor;
            LabelPlace.BackColor = labelSettings.BackColor;
            LabelPlace.Font = labelSettings.Font;
            LabelPlace.Width = labelWidth;
            LabelPlace.Height = labelSettings.Height;

            LabelPlace.Location = new Point(labelStartX, LabelLastHeight);


            leftCounterCI.Size = gossipStoneSize;
            leftCounterCI.ItemCount.Font = new Font(LabelPlace.Font.Name, counterFontSize);
            leftCounterCI.ItemCount.ForeColor = labelSettings.ForeColor;
            leftCounterCI.ImageNames = new[] { counterImage };
            leftCounterCI.BackColor = ackColor;
            leftCounterCI.ItemCount.BackColor = ackColor;
            leftCounterCI.Location = new Point(gossipStoneStartX, LabelPlace.Location.Y);


            rightCounterCI.Size = gossipStoneSize;
            rightCounterCI.ItemCount.Font = new Font(LabelPlace.Font.Name, counterFontSize);
            rightCounterCI.ItemCount.ForeColor = labelSettings.ForeColor;
            rightCounterCI.ImageNames = new[] { counterImage };
            rightCounterCI.BackColor = ackColor;
            rightCounterCI.ItemCount.BackColor = ackColor;
            rightCounterCI.Location = new Point(gossipStoneStartX + (leftCounterCI.Width + counterSpacing), LabelPlace.Location.Y);


        }
    }
}

