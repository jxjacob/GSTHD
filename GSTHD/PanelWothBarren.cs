using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    class PanelWothBarren : Panel, UpdatableFromSettings
    {
        Settings Settings;

        public Dictionary<string, string> KeycodesWithTag;

        public List<WotH> ListWotH = new List<WotH>();
        public List<Barren> ListBarren = new List<Barren>();
        public List<Quantity> ListQuantity = new List<Quantity>();

        public TextBoxCustom textBoxCustom;
        private int GossipStoneCount;
        private string[] ListImage_WothItemsOption;
        private int PathGoalCount;
        private string[] ListImage_GoalsOption;
        private int CounterFontSize;
        private int CounterSpacing;
        private string CounterImage;
        private Size subBoxSize;
        private Size GossipStoneSize;
        int GossipStoneSpacing;
        int PathGoalSpacing;
        int NbMaxRows;
        bool isScrollable;
        bool isBroadcastable;
        bool PathCycling = false;
        string OuterPathID;
        // 0 = WotH, 1 = Barren, 2 = Quantity
        public int isWotH;
        PictureBoxSizeMode SizeMode;
        Label LabelSettings = new Label();

        public PanelWothBarren(ObjectPanelWotH data, Settings settings)
        {
            Settings = settings;
            Visible = data.Visible;

            GossipStoneSize = data.GossipStoneSize;
            this.BackColor = data.BackColor;
            this.Location = new Point(data.X, data.Y);
            this.Name = data.Name;
            this.Size = new Size(data.Width, data.Height);
            this.GossipStoneCount = data.GossipStoneCount.HasValue ? data.GossipStoneCount.Value : settings.DefaultWothGossipStoneCount;
            this.PathGoalCount = data.PathGoalCount.HasValue ? data.PathGoalCount.Value : settings.DefaultPathGoalCount;
            this.GossipStoneSpacing = data.GossipStoneSpacing;
            this.PathGoalSpacing = data.PathGoalSpacing;
            this.TabStop = false;
            this.isScrollable = data.IsScrollable;
            this.SizeMode = data.SizeMode;
            this.PathCycling = data.PathCycling;
            this.isBroadcastable = data.isBroadcastable;
            this.isWotH = 0;
            if (data.IsScrollable)
                this.MouseWheel += Panel_MouseWheel;


        }

        public PanelWothBarren(ObjectPanelBarren data, Settings settings)
        {
            Settings = settings;

            this.BackColor = data.BackColor;
            this.Location = new Point(data.X, data.Y);
            this.Name = data.Name;
            this.Size = new Size(data.Width, data.Height);
            this.TabStop = false;
            this.isWotH = 1;
            if (data.IsScrollable)
                this.MouseWheel += Panel_MouseWheel;
        }

        public PanelWothBarren(ObjectPanelQuantity data, Settings settings)
        {
            Settings = settings;

            this.BackColor = data.BackColor;
            this.Location = new Point(data.X, data.Y);
            this.Name = data.Name;
            this.Size = new Size(data.Width, data.Height);
            this.TabStop = false;
            this.CounterFontSize = data.CounterFontSize;
            this.CounterSpacing = data.CounterSpacing;
            this.GossipStoneSize = data.CounterSize;
            this.CounterImage = data.CounterImage;
            this.subBoxSize = data.SubTextBoxSize;
            this.isWotH = 2;
            if (data.IsScrollable)
                this.MouseWheel += Panel_MouseWheel;
        }

        public void UpdateFromSettings()
        {
            foreach (var woth in ListWotH)
            {
                woth.UpdateFromSettings();
            }
            foreach (var barren in ListBarren)
            {
                barren.UpdateFromSettings();
            }
            foreach (var barren in ListQuantity)
            {
                barren.UpdateFromSettings();
            }
        }


        private void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            var panel = (Panel)sender;
            if (e.Delta != 0)
            {
                //really wish i didnt have to go through this foreach twice but thats just the order it has to be processed, unfortunately
                foreach (var element in panel.Controls)
                {
                    if (element is GossipStone gs)
                        if (gs.hoveredOver) return;
                    if (element is CollectedItem ci)
                        if (ci.hoveredOver) return;
                }
                int moveDirection;
                if (e.Delta < 0) moveDirection = -15; else moveDirection = +15;
                foreach (var element in panel.Controls)
                {
                    if (element is Label la)
                        la.Location = new Point(la.Location.X, la.Location.Y + moveDirection);
                    if (element is GossipStone gs)
                        gs.Location = new Point(gs.Location.X, gs.Location.Y + moveDirection);
                    if (element is TextBox tb)
                        tb.Location = new Point(tb.Location.X, tb.Location.Y + moveDirection);
                    if (element is CollectedItem ci)
                        ci.Location = new Point(ci.Location.X, ci.Location.Y + moveDirection);
                }
            }
            ((PanelWothBarren)panel).SetSuggestionContainer();
        }

        public void PanelWoth(Dictionary<string, string> PlacesWithTag, Dictionary<string, string> keycodesWithTag, ObjectPanelWotH data)
        {
            ListImage_WothItemsOption = data.GossipStoneImageCollection ?? Settings.DefaultGossipStoneImages;
            ListImage_GoalsOption = data.PathGoalImageCollection ?? Settings.DefaultPathGoalImages;
            NbMaxRows = data.NbMaxRows;

            KeycodesWithTag = keycodesWithTag;
            OuterPathID = data.OuterPathID;

            LabelSettings = new Label
            {
                ForeColor = data.LabelColor,
                BackColor = data.LabelBackColor,
                Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.Width,
                Height = data.LabelHeight,
            };

            textBoxCustom = new TextBoxCustom
            (
                Settings,
                PlacesWithTag,
                new Point(0, 0),
                data.TextBoxBackColor,
                new Font(data.TextBoxFontName, data.TextBoxFontSize, data.TextBoxFontStyle),
                data.TextBoxName,
                new Size(data.Width, data.TextBoxHeight),
                data.TextBoxText,
                (PathGoalCount > 0 || OuterPathID != null)
            );
            textBoxCustom.TextBoxField.KeyDown += textBoxCustom_KeyDown_WotH;
            textBoxCustom.TextBoxField.MouseClick += textBoxCustom_MouseClick;
            this.Controls.Add(textBoxCustom.TextBoxField);
        }

        public void PanelBarren(Dictionary<string, string> PlacesWithTag, ObjectPanelBarren data)
        {
            NbMaxRows = data.NbMaxRows;

            LabelSettings = new Label
            {
                ForeColor = data.LabelColor,
                BackColor = data.LabelBackColor,
                Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.LabelWidth,
                Height = data.LabelHeight
            };

            textBoxCustom = new TextBoxCustom
                (
                    Settings,
                    PlacesWithTag,
                    new Point(0, 0),
                    data.TextBoxBackColor,
                    new Font(data.TextBoxFontName, data.TextBoxFontSize, data.TextBoxFontStyle),
                    data.TextBoxName,
                    new Size(data.TextBoxWidth, data.TextBoxHeight),
                    data.TextBoxText
                );
            textBoxCustom.TextBoxField.KeyDown += textBoxCustom_KeyDown_Barren;
            textBoxCustom.TextBoxField.MouseClick += textBoxCustom_MouseClick;
            this.Controls.Add(textBoxCustom.TextBoxField);
        }

        public void PanelQuantity(Dictionary<string, string> PlacesWithTag, ObjectPanelQuantity data)
        {
            NbMaxRows = data.NbMaxRows;

            LabelSettings = new Label
            {
                ForeColor = data.LabelColor,
                BackColor = data.LabelBackColor,
                Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.LabelWidth,
                Height = data.LabelHeight
            };

            textBoxCustom = new TextBoxCustom
                (
                    Settings,
                    PlacesWithTag,
                    new Point(0, 0),
                    data.TextBoxBackColor,
                    new Font(data.TextBoxFontName, data.TextBoxFontSize, data.TextBoxFontStyle),
                    data.TextBoxName,
                    new Size(data.TextBoxWidth, data.TextBoxHeight),
                    data.TextBoxText,
                    true
                );
            textBoxCustom.TextBoxField.KeyDown += textBoxCustom_KeyDown_Quantity;
            textBoxCustom.TextBoxField.MouseClick += textBoxCustom_MouseClick;
            this.Controls.Add(textBoxCustom.TextBoxField);
        }

        public void SetSuggestionContainer()
        {
            textBoxCustom.SetSuggestionsContainerLocation(this.Location);
            textBoxCustom.SuggestionContainer.BringToFront();
        }

        private void textBoxCustom_MouseClick(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).Text = string.Empty;
        }

        private void textBoxCustom_KeyDown_Barren(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                var textbox = (TextBox)sender;
                if (ListBarren.Count < NbMaxRows)
                {
                    AddBarren(textbox.Text);
                }
                textbox.Text = string.Empty;
            }
        }

        private void textBoxCustom_KeyDown_Quantity(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                var textbox = (TextBox)sender;
                if (ListQuantity.Count < NbMaxRows)
                {
                    if (textbox.Text != string.Empty)
                    {
                        if (textbox.Lines.Length > 1)
                        {
                            AddQuantity(textbox.Lines[1], textbox.Lines[0]);
                        }
                        else AddQuantity(textbox.Text);
                    }
                }
                textbox.Text = string.Empty;
            }
        }

        private void textBoxCustom_KeyDown_WotH(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                var textbox = (TextBox)sender;
                if (ListWotH.Count < NbMaxRows)
                {
                    if (textbox.Text != string.Empty)
                    {
                        if (textbox.Lines.Length > 1)
                        {
                            AddWotH(textbox.Lines[1], textbox.Lines[0]);
                        } else AddWotH(textbox.Text);
                    }
                }
                textbox.Text = string.Empty;
            }
        }

        private void AddWotH(string text, string codestring="")
        {
            Dictionary<string, string> FoundKeycodes = new Dictionary<string, string> { };
            string selectedPlace;
            if ((codestring!="" || codestring != string.Empty) && (PathGoalCount > 0 || OuterPathID != null) && Settings.HintPathAutofill)
            {
                // if theres a letter that isnt a code, bail
                foreach (char x in codestring)
                {
                    if (KeycodesWithTag.ContainsKey(x.ToString()))
                    {
                        if (!FoundKeycodes.ContainsKey(x.ToString())) FoundKeycodes.Add(x.ToString(), KeycodesWithTag[x.ToString()]);
                    } else if (!Settings.HintPathAutofillAggressive)
                    {
                        FoundKeycodes.Clear();
                        break;
                    }
                }

                // if there is no lookup, then assume the code is a misinterpit and add it back
                if (FoundKeycodes.Count > 0)
                {
                    selectedPlace = text.ToUpper().Trim().Replace(",", "");
                } else
                {
                    selectedPlace = (codestring + " " + text).ToUpper().Trim().Replace(",", "");

                }

            } else
            {
                selectedPlace = (codestring + " " + text).ToUpper().Trim().Replace(",", "");
            }

            // add woth if duplicates are allowed or if there aren't any duplicates
            if (Settings.EnableDuplicateWoth || !ListWotH.Any(x => x.Name == selectedPlace))
            {
                var newlocation = (ListWotH.Count <= 0) ? new Point(0, -LabelSettings.Height) : ListWotH.Last().LabelPlace.Location;
                WotH newWotH = new WotH(Settings, selectedPlace,
                        GossipStoneCount, ListImage_WothItemsOption, GossipStoneSpacing,
                        PathGoalCount, ListImage_GoalsOption, PathGoalSpacing,
                        newlocation, LabelSettings, GossipStoneSize, this.isScrollable, this.SizeMode, this.isBroadcastable, this.PathCycling);
                
                ListWotH.Add(newWotH);
                this.Controls.Add(newWotH.LabelPlace);
                newWotH.LabelPlace.MouseClick += LabelPlace_MouseClick_WotH;
                foreach (var gossipStone in newWotH.listGossipStone)
                {
                    this.Controls.Add(gossipStone);
                }
                
                
                if (FoundKeycodes.Count > 0)
                {
                    if (PathGoalCount > 0)
                    {
                        GossipStone pathStone = null;
                        int y = 0;
                        foreach (var z in FoundKeycodes)
                        {
                            // spreads the paths evenly across multiple path stones (if applicable)
                            pathStone = newWotH.listGossipStone.Where(x => x.Name == newWotH.Name + $"_GoalGossipStone{y}").ToList()[0];
                            if (!pathStone.HeldImages.Contains(z.Value)) pathStone.HeldImages.Add(z.Value);
                            if (PathGoalCount > 1) y = (y + 1) % (PathGoalCount);
                            pathStone.HoldsImage = true;
                            pathStone.UpdateImage();
                        }
                    } else if (OuterPathID != null)
                    {
                        GSTForms f = (GSTForms)this.FindForm();
                        foreach (var z in FoundKeycodes)
                        {
                            var search = f.Controls[0].Controls.OfType<Item>().Where(x => x.OuterPathID == $"{OuterPathID}_{z.Key.ToUpper()}").ToList();
                            if (search.Count > 0) search[0].SetState(1);
                        }
                    }
                }
                //Move TextBoxCustom
                textBoxCustom.newLocation(new Point(0, newWotH.LabelPlace.Location.Y + newWotH.LabelPlace.Height), this.Location);
            }
        }

        private void AddBarren(string text)
        {
            var selectedPlace = text.ToUpper().Trim().Replace(",", "");
            // prevent dupes
            if (!ListBarren.Any(x => x.Name == selectedPlace))
            {
                var newlocation = (ListBarren.Count <= 0) ? new Point(0, -LabelSettings.Height) : ListBarren.Last().LabelPlace.Location;
                Barren newBarren = newBarren = new Barren(Settings, selectedPlace, newlocation, LabelSettings);
                
                ListBarren.Add(newBarren);
                this.Controls.Add(newBarren.LabelPlace);
                newBarren.LabelPlace.MouseClick += LabelPlace_MouseClick_Barren;
                textBoxCustom.newLocation(new Point(0, newBarren.LabelPlace.Location.Y + newBarren.LabelPlace.Height), this.Location);
            }
        }

        private void AddQuantity(string text, string codestring = "")
        {
            string selectedPlace;
            int foundin = 0;
            if ((codestring != "" || codestring != string.Empty) && Settings.HintPathAutofill)
            {
                // if theres a letter that isnt a code, bail

                try
                {
                    foundin = int.Parse(codestring);
                } catch {
                
                }

                // if there is no lookup, then assume the code is a misinterpit and add it back
                if (foundin != 0)
                {
                    selectedPlace = text.ToUpper().Trim().Replace(",", "");
                }
                else
                {
                    selectedPlace = (codestring + " " + text).ToUpper().Trim().Replace(",", "");

                }

            }
            else
            {
                selectedPlace = (codestring + " " + text).ToUpper().Trim().Replace(",", "");
            }
            // prevent dupes
            if (!ListQuantity.Any(x => x.Name == selectedPlace))
            {
                var newlocation = (ListQuantity.Count <= 0) ? new Point(2, -LabelSettings.Height) : ListQuantity.Last().LabelPlace.Location;
                Quantity newQuan = new Quantity(Settings, selectedPlace,
                        CounterFontSize, CounterSpacing, CounterImage,
                        subBoxSize, LabelSettings.BackColor,
                        newlocation, LabelSettings, GossipStoneSize, this.isBroadcastable);
            
                ListQuantity.Add(newQuan);
                this.Controls.Add(newQuan.LabelPlace);
                newQuan.LabelPlace.MouseClick += LabelPlace_MouseClick_Quantity;

                this.Controls.Add(newQuan.leftCounterCI);
                this.Controls.Add(newQuan.rightCounterCI);

                textBoxCustom.newLocation(new Point(0, newQuan.LabelPlace.Location.Y + newQuan.LabelPlace.Height), this.Location);

                if (foundin != 0)
                {
                    newQuan.rightCounterCI.SetState(foundin);
                }

            }

        }

        private void LabelPlace_MouseClick_Barren(object sender, MouseEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton))
            {
                var label = (Label)sender;
                var barren = this.ListBarren.Where(x => x.LabelPlace.Name == label.Name).ToList()[0];
                this.RemoveBarren(barren);
            }
        }

        private void LabelPlace_MouseClick_Quantity(object sender, MouseEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton))
            {
                var label = (Label)sender;
                var barren = this.ListQuantity.Where(x => x.LabelPlace.Name == label.Name).ToList()[0];
                this.RemoveQuantity(barren);
            }
        }

        private void LabelPlace_MouseClick_WotH(object sender, MouseEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton))
            {
                var label = (Label)sender;
                var woth = this.ListWotH.Where(x => x.LabelPlace.Name == label.Name).ToList()[0];
                this.RemoveWotH(woth);
            }
        }

        public void RemoveWotH(WotH woth)
        {
            ListWotH.Remove(woth);

            this.Controls.Remove(woth.LabelPlace);
            foreach (var gossipStone in woth.listGossipStone)
            {
                gossipStone.TryToKill();
                this.Controls.Remove(gossipStone);
            }

            for (int i = 0; i < ListWotH.Count; i++)
            {
                var wothLabel = ListWotH[i].LabelPlace;
                var newY = i * wothLabel.Height;
                wothLabel.Location = new Point(wothLabel.Left, newY);

                for (int j = 0; j < ListWotH[i].listGossipStone.Count; j++)
                {
                    var newX = ListWotH[i].listGossipStone[j].Location.X;
                    ListWotH[i].listGossipStone[j].Location = new Point(newX, newY);
                }
            }
            textBoxCustom.newLocation(new Point(0, ListWotH.Count * woth.LabelPlace.Height), this.Location);
        }

        public void RemoveBarren(Barren barren)
        {
            ListBarren.Remove(barren);

            this.Controls.Remove(barren.LabelPlace);

            for (int i = 0; i < ListBarren.Count; i++)
            {
                var wothLabel = ListBarren[i].LabelPlace;
                wothLabel.Location = new Point(0, (i * wothLabel.Height));
            }
            textBoxCustom.newLocation(new Point(0, ListBarren.Count * barren.LabelPlace.Height), this.Location);
        }

        public void RemoveQuantity(Quantity quantity)
        {
            ListQuantity.Remove(quantity);

            this.Controls.Remove(quantity.LabelPlace);
            this.Controls.Remove(quantity.leftCounterCI);
            this.Controls.Remove(quantity.rightCounterCI);

            for (int i = 0; i < ListQuantity.Count; i++)
            {
                var wothLabel = ListQuantity[i].LabelPlace;
                var newY = i * wothLabel.Height;
                wothLabel.Location = new Point(0, newY);
                ListQuantity[i].leftCounterCI.Location = new Point(ListQuantity[i].leftCounterCI.Location.X, newY);
                ListQuantity[i].rightCounterCI.Location = new Point(ListQuantity[i].rightCounterCI.Location.X, newY);
            }
            textBoxCustom.newLocation(new Point(0, ListQuantity.Count * quantity.LabelPlace.Height), this.Location);
        }

        public List<WotHState> GetWotHs()
        {
            List<WotHState> thelist = new List<WotHState>();
            foreach (WotH x in ListWotH)
            {
                thelist.Add(x.SaveState());
            }
            return thelist;
        }

        public void SetWotH(string thestring)
        {
            string[] sections = thestring.Split('\n');
            foreach (string section in sections)
            {
                // break into name & colour         and           stones
                string[] parts = section.Split('\t');

                // name = firstpart[0]
                // color = firstpart[1]
                string[] firstPart = parts[0].Split(',');

                // secondparts are explained below
                string[] secondPart = parts[1].Split(',');
                AddWotH(firstPart[0]);

                // find the woth we just made
                //Control foundWotH = this.Controls.Find(firstPart[0], true)[0];
                WotH thisWotH = this.ListWotH.Where(x => x.Name == firstPart[0].Trim()).ToList()[0];

                thisWotH.SetColor(int.Parse(firstPart[1]));

                GossipStone foundStone = null;
                bool storedHoldsImage = false;
                List<string> storedHeldImageName = null;
                int storedImageIndex = 0;
                if (secondPart.Length > 4)
                {
                    for (int i = 0; i < secondPart.Length; i++)
                    {
                        if (i % 5 == 0)
                        {
                            // 0th is the name
                            foundStone = (GossipStone)(this.Controls.Find(secondPart[i], true)[0]);
                        }
                        else if (i % 5 == 1)
                        {
                            // 1st is the bool
                            storedHoldsImage = Boolean.Parse(secondPart[i]);
                        }
                        else if (i % 5 == 2)
                        {
                            // 2nd is the stored image
                            string[] images = (secondPart[i]).Split('|');
                            storedHeldImageName = images.ToList();
                        }
                        else if (i % 5 == 3)
                        {
                            // 3rd is the stateindex
                            storedImageIndex = int.Parse(secondPart[i]);
                        } else if (i % 5 == 4)
                        {
                            // 4th is 
                            // also we have all 4 so go and set the state
                            foundStone.SetState(new GossipStoneState() { HoldsImage = storedHoldsImage, HeldImages = storedHeldImageName, ImageIndex = storedImageIndex, isMarked = (MarkedImageIndex)int.Parse(secondPart[i]) });
                        }
                    }
                }


            }
        }


        public List<BarrenState> GetBarrens()
        {
            List<BarrenState> thelist = new List<BarrenState>();
            foreach (Barren x in ListBarren)
            {
                thelist.Add(x.SaveState());
            }
            return thelist;
        }

        public void SetBarren(string thestring)
        {
            string[] sections = thestring.Split('\n');
            foreach (string section in sections)
            {
                // name = firstpart[0]
                // color = firstpart[1]
                string[] firstPart = section.Split(',');

                AddBarren(firstPart[0]);

                // find the woth we just made
                Barren thisBarren = this.ListBarren.Where(x => x.Name == firstPart[0]).ToList()[0];

                thisBarren.SetColor(int.Parse(firstPart[1]));



            }
        }

        public List<QuantityState> GetQuantities()
        {
            List<QuantityState> thelist = new List<QuantityState>();
            foreach (Quantity x in ListQuantity)
            {
                thelist.Add(x.SaveState());
            }
            return thelist;
        }

        public void SetQuantities(string thestring)
        {
            string[] sections = thestring.Split('\n');
            foreach (string section in sections)
            {
                // name = firstpart[0]
                // color = firstpart[1]
                string[] firstPart = section.Split(',');

                AddQuantity(firstPart[0]);

                // find the woth we just made
                Quantity thisQuan = this.ListQuantity.Where(x => x.Name == firstPart[0]).ToList()[0];

                thisQuan.SetColor(int.Parse(firstPart[1]));
                thisQuan.leftCounterCI.SetState(int.Parse(firstPart[2]));
                thisQuan.rightCounterCI.SetState(int.Parse(firstPart[3]));




            }
        }
    }
}
