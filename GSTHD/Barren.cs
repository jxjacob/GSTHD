using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    public struct BarrenState
    {
        public string BarrenName;
        public int ColourIndex;

        public override string ToString() => $"{BarrenName},{ColourIndex}";
    }

    class Barren
    {
        public Settings Settings;

        public Label LabelPlace;
        public string Name;

        private Color[] Colors;
        private int ColorIndex;

        public Barren(Settings settings, string selectedPlace, Point lastLabelLocation, Label labelSettings)
        {
            Settings = settings;
            Name = selectedPlace;

            LabelPlace = new Label
            {
                Name = Guid.NewGuid().ToString(),
                Text = selectedPlace,
                ForeColor = labelSettings.ForeColor,
                BackColor = labelSettings.BackColor,
                Font = labelSettings.Font,
                Width = labelSettings.Width,
                Height = labelSettings.Height,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                UseMnemonic = false
            };
            LabelPlace.Location = new Point(0, lastLabelLocation.Y + LabelPlace.Height);
            LabelPlace.MouseDown += new MouseEventHandler(Mouse_ClickDown);

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
            LabelPlace.ForeColor = Colors[Settings.EnableBarrenColors ? ColorIndex : 0];
        }

        private void Mouse_ClickDown(object sender, MouseEventArgs e)
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
    }
}
