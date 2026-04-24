using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace GSTHD
{
    public struct WotHState
    {
        public string WotHText;
        public int ColourIndex;
        public List<GossipStone> Stones;


        public override string ToString() {

            string stoneString = "";
            foreach (GossipStone x in Stones) { 
                GossipStoneState ugh = x.GetState();
                if (ugh.ToString() != "False,,0,0")
                {
                    if (stoneString.Length > 0)
                    {
                        stoneString += ",";
                    }
                    stoneString += x.Name + "," + ugh.ToString();
                }
            }
            return $"{WotHText},{ColourIndex}\t{stoneString}";
        }
    }

    class WotH : PanelHint
    {
        public Settings Settings;

        public LabelExtended LabelPlace { get; set; }
        public List<GossipStone> listGossipStone = [];
        public string Name { get; set; }

        private Color[] Colors;
        private int ColorIndex;
        private int MinIndex;

        private int stoneCount;
        private int pathCount;

        public int PlacedOrder { get; set; }

        private Color GossipStoneBackColor;

        public WotH(Settings settings,
            string selectedPlace,
            int gossipStoneCount, string[] wothItemImageList, int gossipStoneSpacing,
            int pathGoalCount, string[] pathGoalImageList, int pathGoalSpacing,
            Point lastLabelLocation, TextBlock labelSettings, Size gossipStoneSize, Color gossipStoneBackColor, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool PathCycling, bool isMarkable)
        {
            Settings = settings;
            Name = selectedPlace;

            stoneCount = gossipStoneCount;
            pathCount = pathGoalCount;

            GossipStoneBackColor = gossipStoneBackColor;

            var labelStartX = 0;

            if (pathGoalCount > 0)
            {
                labelStartX += pathGoalCount * (gossipStoneSize.Width + pathGoalSpacing) - pathGoalSpacing;
            }
            int panelWidth = (int)labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

            var gossipStoneStartX = panelWidth - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

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

            if (wothItemImageList.Length > 0)
            {
                for (int i = 0; i < gossipStoneCount; i++)
                {
                    GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GossipStone" + i, 0, 0, wothItemImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, isMarkable:isMarkable);
                    newGossipStone.BackColor = new Avalonia.Media.Color(GossipStoneBackColor.A, GossipStoneBackColor.R, GossipStoneBackColor.G, GossipStoneBackColor.B);
                    Canvas.SetTop(newGossipStone, Canvas.GetTop(LabelPlace));
                    Canvas.SetLeft(newGossipStone, gossipStoneStartX + (newGossipStone.TrueWidth + gossipStoneSpacing) * i);
                    listGossipStone.Add(newGossipStone);
                }
            }

            if (pathGoalImageList != null && pathGoalImageList.Length > 0)
            {
                for (int i = 0; i < pathGoalCount; i++)
                {
                    GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GoalGossipStone" + i, 0, 0, pathGoalImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, PathCycling, isMarkable:isMarkable);
                    newGossipStone.BackColor = new Avalonia.Media.Color(GossipStoneBackColor.A, GossipStoneBackColor.R, GossipStoneBackColor.G, GossipStoneBackColor.B);
                    Canvas.SetTop(newGossipStone, Canvas.GetTop(LabelPlace));
                    Canvas.SetLeft(newGossipStone, (newGossipStone.TrueWidth + pathGoalSpacing) * i);
                    listGossipStone.Add(newGossipStone);
                }
            }

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
            foreach (var stone in listGossipStone)
            {
                stone.UpdateFromSettings();
            }
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

        public WotHState SaveState()
        {
            return new WotHState()
            {
                WotHText = Name,
                ColourIndex = ColorIndex,
                Stones = listGossipStone,
            };
        }

        public void RefreshLocation(int gossipStoneCount, string[] wothItemImageList, int gossipStoneSpacing,
                        int pathGoalCount, string[] pathGoalImageList, int pathGoalSpacing,
                        int LabelLastHeight, TextBlock labelSettings, Size gossipStoneSize, Color gossipStoneBackColor, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool PathCycling, bool isMarkable)
        {
            var labelStartX = 0;

            if (pathGoalCount > 0)
            {
                labelStartX += pathGoalCount * (gossipStoneSize.Width + pathGoalSpacing) - pathGoalSpacing;
            }
            int panelWidth = (int)labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

            var gossipStoneStartX = panelWidth - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

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
            LabelPlace.PointerPressed += Mouse_ClickDown;


            if (wothItemImageList.Length > 0)
            {
                for (int i = 0; i < gossipStoneCount; i++)
                {
                    var temp = listGossipStone.Where(g => g.Name == Name + "_GossipStone" + i).ToList();
                    if (temp.Any())
                    {
                        // modify old stone
                        GossipStone tempstone = temp.First();
                        tempstone.ImageNames = wothItemImageList;
                        tempstone.Width = gossipStoneSize.Width;
                        tempstone.Height = gossipStoneSize.Height;
                        tempstone.isScrollable = isScrollable;
                        tempstone.SizeMode = SizeMode;
                        tempstone.isBroadcastable = isBroadcastable;
                        tempstone.isMarkable = isMarkable;
                        tempstone.BackColor = new Avalonia.Media.Color(gossipStoneBackColor.A, gossipStoneBackColor.R,gossipStoneBackColor.G, gossipStoneBackColor.B);
                        Canvas.SetTop(tempstone, Canvas.GetTop(LabelPlace));
                        Canvas.SetLeft(tempstone, gossipStoneStartX + (tempstone.Width + gossipStoneSpacing) * i);
                        Debug.WriteLine($"old stone {i} is at {Canvas.GetLeft(tempstone)},{Canvas.GetTop(tempstone)}");
                    } else
                    {
                        // make new stone
                        GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GossipStone" + i, 0, 0, wothItemImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, isMarkable: isMarkable);
                        newGossipStone.BackColor = new Avalonia.Media.Color(gossipStoneBackColor.A, gossipStoneBackColor.R,gossipStoneBackColor.G, gossipStoneBackColor.B);

                        Canvas.SetTop(newGossipStone, Canvas.GetTop(LabelPlace));
                        Canvas.SetLeft(newGossipStone, gossipStoneStartX + (newGossipStone.Width + gossipStoneSpacing) * i);
                        Debug.WriteLine($"new stone {i} is at {Canvas.GetLeft(newGossipStone)},{Canvas.GetTop(newGossipStone)}");
                        listGossipStone.Add(newGossipStone);
                    }
                }
                if (gossipStoneCount < stoneCount)
                {
                    // kill old stones
                    for (int i = gossipStoneCount; i < stoneCount; i++)
                    {
                        Debug.WriteLine("killing " + i);
                        GossipStone temp = listGossipStone.First(g => g.Name == Name + "_GossipStone" + i);
                        listGossipStone.Remove(temp);
                        temp.TryToKill();
                        // temp.Dispose();
                    }
                }
            }
            stoneCount = gossipStoneCount;


            if (pathGoalImageList.Length > 0)
            {
                for (int i = 0; i < gossipStoneCount; i++)
                {
                    var temp = listGossipStone.Where(g => g.Name == Name + "_GoalGossipStone" + i).ToList();
                    if (temp.Any())
                    {
                        // modify old stone
                        GossipStone tempstone = temp.First();
                        tempstone.ImageNames = pathGoalImageList;
                        tempstone.Width = gossipStoneSize.Width;
                        tempstone.Height = gossipStoneSize.Height;
                        tempstone.isScrollable = isScrollable;
                        tempstone.SizeMode = SizeMode;
                        tempstone.isBroadcastable = isBroadcastable;
                        tempstone.isMarkable = isMarkable;
                        Canvas.SetTop(tempstone, Canvas.GetTop(LabelPlace));
                        Canvas.SetLeft(tempstone, (tempstone.Width + pathGoalSpacing) * i);
                        tempstone.BackColor = new Avalonia.Media.Color(gossipStoneBackColor.A, gossipStoneBackColor.R,gossipStoneBackColor.G, gossipStoneBackColor.B);
                        tempstone.UpdateImage();
                        //Debug.WriteLine($"old stone {i} is at {tempstone.Location}");
                    }
                    else
                    {
                        // make new stone
                        GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GoalGossipStone" + i, 0, 0, pathGoalImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, isMarkable: isMarkable);
                        newGossipStone.BackColor = new Avalonia.Media.Color(gossipStoneBackColor.A, gossipStoneBackColor.R,gossipStoneBackColor.G, gossipStoneBackColor.B);

                        Canvas.SetTop(newGossipStone, Canvas.GetTop(LabelPlace));
                        Canvas.SetLeft(newGossipStone, (newGossipStone.Width + pathGoalSpacing) * i);
                        //Debug.WriteLine($"new stone {i} is at {newGossipStone.Location}");
                        listGossipStone.Add(newGossipStone);
                    }
                }
                if (pathGoalCount < pathCount)
                {
                    // kill old stones
                    for (int i = pathGoalCount; i < pathCount; i++)
                    {
                        //Debug.WriteLine("killing " + i);
                        GossipStone temp = listGossipStone.Where(g => g.Name == Name + "_GoalGossipStone" + i).First();
                        listGossipStone.Remove(temp);
                        temp.TryToKill();
                        // temp.Dispose();
                    }
                }
            }
            pathCount = pathGoalCount;
        }
    }
}
