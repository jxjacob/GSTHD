using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

    class WotH
    {
        public Settings Settings;

        public LabelExtended LabelPlace;
        public List<GossipStone> listGossipStone = new List<GossipStone>();
        public string Name;

        private Color[] Colors;
        private int ColorIndex;
        private int MinIndex;

        private int stoneCount;
        private int pathCount;

        private Color GossipStoneBackColor;

        public WotH(Settings settings,
            string selectedPlace,
            int gossipStoneCount, string[] wothItemImageList, int gossipStoneSpacing,
            int pathGoalCount, string[] pathGoalImageList, int pathGoalSpacing,
            Point lastLabelLocation, Label labelSettings, Size gossipStoneSize, Color gossipStoneBackColor, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool PathCycling, bool isMarkable)
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
            int panelWidth = labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

            var gossipStoneStartX = panelWidth - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

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

            if (wothItemImageList.Length > 0)
            {
                for (int i = 0; i < gossipStoneCount; i++)
                {
                    GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GossipStone" + i, 0, 0, wothItemImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, isMarkable:isMarkable);
                    newGossipStone.BackColor = GossipStoneBackColor;
                    newGossipStone.Location =
                        new Point(gossipStoneStartX + (newGossipStone.Width + gossipStoneSpacing) * i, LabelPlace.Location.Y);
                    listGossipStone.Add(newGossipStone);
                }
            }

            if (pathGoalImageList != null && pathGoalImageList.Length > 0)
            {
                for (int i = 0; i < pathGoalCount; i++)
                {
                    GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GoalGossipStone" + i, 0, 0, pathGoalImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, PathCycling, isMarkable:isMarkable);
                    newGossipStone.BackColor = GossipStoneBackColor;
                    newGossipStone.Location =
                        new Point((newGossipStone.Width + pathGoalSpacing) * i, LabelPlace.Location.Y);
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
            LabelPlace.ForeColor = Colors[ColorIndex];
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
                        int LabelLastHeight, Label labelSettings, Size gossipStoneSize, Color gossipStoneBackColor, bool isScrollable, PictureBoxSizeMode SizeMode, bool isBroadcastable, bool PathCycling, bool isMarkable)
        {
            var labelStartX = 0;

            if (pathGoalCount > 0)
            {
                labelStartX += pathGoalCount * (gossipStoneSize.Width + pathGoalSpacing) - pathGoalSpacing;
            }
            int panelWidth = labelSettings.Width;
            int labelWidth = panelWidth - labelStartX - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

            var gossipStoneStartX = panelWidth - gossipStoneCount * (gossipStoneSize.Width + gossipStoneSpacing) + gossipStoneSpacing;

            LabelPlace.ForeColor = labelSettings.ForeColor;
            LabelPlace.BackColor = labelSettings.BackColor;
            LabelPlace.Font = labelSettings.Font;
            LabelPlace.Width = labelWidth;
            LabelPlace.Height = labelSettings.Height;
            LabelPlace.Location = new Point(labelStartX, LabelLastHeight);
            LabelPlace.MouseDown += new MouseEventHandler(Mouse_ClickDown);


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
                        tempstone.Size = gossipStoneSize;
                        tempstone.isScrollable = isScrollable;
                        tempstone.SizeMode = SizeMode;
                        tempstone.isBroadcastable = isBroadcastable;
                        tempstone.isMarkable = isMarkable;
                        tempstone.BackColor = gossipStoneBackColor;
                        tempstone.Location = new Point(gossipStoneStartX + (tempstone.Width + gossipStoneSpacing) * i, LabelPlace.Location.Y);
                        Debug.WriteLine($"old stone {i} is at {tempstone.Location}");
                    } else
                    {
                        // make new stone
                        GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GossipStone" + i, 0, 0, wothItemImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, isMarkable: isMarkable);
                        newGossipStone.BackColor = gossipStoneBackColor;
                        newGossipStone.Location =
                            new Point(gossipStoneStartX + (newGossipStone.Width + gossipStoneSpacing) * i, LabelPlace.Location.Y);
                        Debug.WriteLine($"new stone {i} is at {newGossipStone.Location}");
                        listGossipStone.Add(newGossipStone);
                    }
                }
                if (gossipStoneCount < stoneCount)
                {
                    // kill old stones
                    for (int i = gossipStoneCount; i < stoneCount; i++)
                    {
                        Debug.WriteLine("killing " + i);
                        GossipStone temp = listGossipStone.Where(g => g.Name == Name + "_GossipStone" + i).First();
                        listGossipStone.Remove(temp);
                        temp.TryToKill();
                        temp.Dispose();
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
                        tempstone.Size = gossipStoneSize;
                        tempstone.isScrollable = isScrollable;
                        tempstone.SizeMode = SizeMode;
                        tempstone.isBroadcastable = isBroadcastable;
                        tempstone.isMarkable = isMarkable;
                        tempstone.Location = new Point((tempstone.Width + pathGoalSpacing) * i, LabelPlace.Location.Y);
                        tempstone.BackColor = gossipStoneBackColor;
                        tempstone.UpdateImage();
                        //Debug.WriteLine($"old stone {i} is at {tempstone.Location}");
                    }
                    else
                    {
                        // make new stone
                        GossipStone newGossipStone = new GossipStone(Settings, true, Name + "_GoalGossipStone" + i, 0, 0, pathGoalImageList, gossipStoneSize, isScrollable, SizeMode, isBroadcastable, isMarkable: isMarkable);
                        newGossipStone.BackColor = gossipStoneBackColor;
                        newGossipStone.Location =
                            new Point((newGossipStone.Width + pathGoalSpacing) * i, LabelPlace.Location.Y);
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
                        temp.Dispose();
                    }
                }
            }
            pathCount = pathGoalCount;
        }
    }
}
