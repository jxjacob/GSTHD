using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace GSTHD
{
    public struct BarrenState
    {
        public string BarrenName;
        public int ColourIndex;

        public override string ToString() => $"{BarrenName},{ColourIndex}";
    }

    class Barren : PanelHint
    {
        public Settings Settings;

        public LabelExtended LabelPlace { get; set; }
        public string Name { get; set; }

        private Color[] Colors;
        private int ColorIndex;
        public int PlacedOrder { get; set; }

        public Barren(Settings settings, string selectedPlace, Point lastLabelLocation, TextBlock labelSettings)
        {
            Settings = settings;
            Name = selectedPlace;

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
                Width = labelSettings.Width,
                Height = labelSettings.Height,
                TextAlignment = TextAlignment.Left,
                Padding = new Thickness(2, 2),
                // AutoEllipsis = true,
                // UseMnemonic = false
            };
            Canvas.SetTop(LabelPlace, lastLabelLocation.Y + LabelPlace.Height);
            LabelPlace.PointerPressed += Mouse_ClickDown;

            Colors = new Color[Settings.DefaultBarrenColors.Length];
            for (int i = 0; i < Settings.DefaultBarrenColors.Length; i++)
            {
                Colors[i] = Color.FromName(Settings.DefaultBarrenColors[i]);
            }
            ColorIndex = 0;
            UpdateFromSettings();
        }

        public void UpdateFromSettings()
        {
            UpdateColor();
        }

        public void SetColor(int color)
        {
            ColorIndex = color;
            UpdateColor();
        }

        public void UpdateColor()
        {
            var currentcolour = Colors[Settings.EnableBarrenColors ? ColorIndex : 0];
            LabelPlace.Foreground = new SolidColorBrush(new Avalonia.Media.Color(currentcolour.A,currentcolour.R, currentcolour.G, currentcolour.B));
        }

        private void Mouse_ClickDown(object sender, PointerEventArgs e)
        {
            if (!Settings.EnableBarrenColors)
                return;

            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton))
            {
                if (ColorIndex < Colors.Length - 1) ColorIndex++;
                else if (Settings.WraparoundItems) ColorIndex = 0;
            }
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton))
            {
                if (ColorIndex > 0) ColorIndex--;
                else if (Settings.WraparoundItems) ColorIndex = Colors.Length - 1;
            }
            UpdateColor();
        }

        public BarrenState SaveState()
        {
            return new BarrenState()
            {
               BarrenName = Name,
               ColourIndex = ColorIndex,
            };
        }

        public void RefreshLocation(int LabelLastHeight, TextBlock labelSettings)
        {
            LabelPlace.Foreground = labelSettings.Foreground;
            LabelPlace.Background = labelSettings.Background;
            LabelPlace.FontFamily = labelSettings.FontFamily;
            LabelPlace.FontSize = labelSettings.FontSize;
            LabelPlace.FontWeight = labelSettings.FontWeight;
            LabelPlace.FontStyle = labelSettings.FontStyle;
            LabelPlace.Width = labelSettings.Width;
            LabelPlace.Height = labelSettings.Height;

            Canvas.SetTop(LabelPlace, LabelLastHeight);
        }
    }
}
