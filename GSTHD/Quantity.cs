using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;


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
        public int PlacedOrder { get; set; }

        public Quantity(Settings settings,
            string selectedPlace,
            int counterFontSize, int counterSpacing, string counterImageName,
            Size subBoxSize, Color subBackColor, 
            Point lastLabelLocation, TextBlock labelSettings, Size gossipStoneSize, bool isBroadcastable)
        {
            Settings = settings;
            Name = selectedPlace;

            var labelStartX = 0;

            int panelWidth = (int)labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;

            var gossipStoneStartX = panelWidth - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;

            LabelPlace = new LabelExtended
            {
                Name = Guid.NewGuid().ToString(),
                Text = selectedPlace,
                Foreground = labelSettings.Foreground,
                Background = labelSettings.Background,
                FontFamily = labelSettings.FontFamily,
                FontSize = labelSettings.FontSize,
                FontWeight = labelSettings.FontWeight,
                FontStyle =  labelSettings.FontStyle,
                Width = labelWidth,
                Height = labelSettings.Height,
                TextAlignment = TextAlignment.Left,
                Padding = new Thickness(2, 2)
                // AutoEllipsis = true,
            };
            Canvas.SetTop(LabelPlace, lastLabelLocation.Y + LabelPlace.Height);
            Canvas.SetLeft(LabelPlace, labelStartX);
            LabelPlace.PointerPressed += Mouse_ClickDown;



            ObjectPointCollectedItem tempOPCI = new ObjectPointCollectedItem
            {
                Size = gossipStoneSize,
                LabelFontSize = counterFontSize,
                ImageCollection = [counterImageName],
                CountPosition = new Size(6, counterFontSize - (counterFontSize/10)),
                LabelFontName = labelSettings.FontFamily.Name,
                LabelColor = Color.FromArgb(((ISolidColorBrush)labelSettings.Foreground).Color.A, ((ISolidColorBrush)labelSettings.Foreground).Color.R, ((ISolidColorBrush)labelSettings.Foreground).Color.G, ((ISolidColorBrush)labelSettings.Foreground).Color.B),
                hasSlash = true,
                BackColor = subBackColor,
                isMarkable = false,
                BackGroundColor = subBackColor,
                // note: quantity/woth/barren are NOT controls, so visibility might need to be passed from somewhere else
                Visible = true,
                X = gossipStoneStartX,
                Y = Canvas.GetTop(LabelPlace)
            };

            //Debug.WriteLine("making left CI");
            leftCounterCI = new CollectedItem(tempOPCI, Settings, isBroadcastable);

            //Debug.WriteLine(leftCounterCI.Location);

            // Debug.WriteLine("rmaking right CI");
            tempOPCI.hasSlash = false;
            tempOPCI.X = gossipStoneStartX + (leftCounterCI.TrueWidth + counterSpacing);
            rightCounterCI = new CollectedItem(tempOPCI, Settings, isBroadcastable);
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
            var currentcolour = Colors[ColorIndex];
            LabelPlace.Foreground = new SolidColorBrush(new Avalonia.Media.Color(currentcolour.A,currentcolour.R, currentcolour.G, currentcolour.B));
            leftCounterCI.SetColor(new Avalonia.Media.Color(currentcolour.A,currentcolour.R, currentcolour.G, currentcolour.B));
            rightCounterCI.SetColor(new Avalonia.Media.Color(currentcolour.A,currentcolour.R, currentcolour.G, currentcolour.B));
        }

        private void Mouse_ClickDown(object sender, PointerPressedEventArgs e)
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
                        Size subBoxSize, IBrush backColor,
                        int LabelLastHeight, TextBlock labelSettings, Size gossipStoneSize)
        {


            var labelStartX = 0;

            int panelWidth = (int)labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;

            var gossipStoneStartX = panelWidth - (gossipStoneSize.Width + counterSpacing + subBoxSize.Width) + counterSpacing;


            LabelPlace.Foreground = labelSettings.Foreground;
            LabelPlace.Background = labelSettings.Background;
            LabelPlace.FontFamily = labelSettings.FontFamily;
            LabelPlace.FontSize = labelSettings.FontSize;
            LabelPlace.FontWeight = labelSettings.FontWeight;
            LabelPlace.FontStyle = labelSettings.FontStyle;
            LabelPlace.Width = labelWidth;
            LabelPlace.Height = labelSettings.Height;
            Canvas.SetTop(LabelPlace, LabelLastHeight);
            Canvas.SetLeft(LabelPlace, labelStartX);


            leftCounterCI.TrueHeight = gossipStoneSize.Height;
            leftCounterCI.TrueWidth = gossipStoneSize.Width;
            leftCounterCI.ItemCount.FontFamily = LabelPlace.FontFamily;
            leftCounterCI.ItemCount.FontSize = LabelPlace.FontSize;
            leftCounterCI.ItemCount.Foreground = labelSettings.Foreground;
            leftCounterCI.ImageNames = [counterImage];
            leftCounterCI.BackColor = ((ISolidColorBrush)labelSettings.Foreground).Color;
            leftCounterCI.ItemCount.Background = backColor;
            Canvas.SetTop(leftCounterCI, LabelLastHeight);
            Canvas.SetLeft(leftCounterCI, gossipStoneStartX);


            rightCounterCI.TrueHeight = gossipStoneSize.Height;
            rightCounterCI.TrueWidth = gossipStoneSize.Width;
            rightCounterCI.ItemCount.FontFamily = LabelPlace.FontFamily;
            rightCounterCI.ItemCount.FontSize = LabelPlace.FontSize;
            rightCounterCI.ItemCount.Foreground = labelSettings.Foreground;
            rightCounterCI.ImageNames = [counterImage];
            rightCounterCI.BackColor = ((ISolidColorBrush)labelSettings.Foreground).Color;
            rightCounterCI.ItemCount.Background = backColor;
            Canvas.SetTop(rightCounterCI, LabelLastHeight);
            Canvas.SetLeft(rightCounterCI, gossipStoneStartX + (leftCounterCI.Width + counterSpacing));


        }
    }
}

