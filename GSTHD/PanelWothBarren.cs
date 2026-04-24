using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Color = System.Drawing.Color;

namespace GSTHD
{
    public enum HintPanelType
    {
        WotH = 0,
        Barren = 1,
        Quantity = 2,
        Mixed = 3
    }
    class PanelWothBarren : Canvas, UpdatableFromSettings, IAlternatableObject
    {
        Settings Settings;

        public Dictionary<string, string> KeycodesWithTag;

        public List<PanelHint> ListHints = new List<PanelHint>();
        public List<MixedSubPanels> ListSubs = new List<MixedSubPanels>();

        public TextBoxCustom textBoxCustom;
        public int GossipStoneCount { get; set; }
        public string[] ListImage_WothItemsOption { get; set; }
        public int PathGoalCount { get; set; }
        public string[] ListImage_GoalsOption { get; set; }
        public int CounterFontSize { get; set; }
        public int CounterSpacing { get; set; }
        private string CounterImage;
        public  Size subBoxSize { get; set; }
        public Size GossipStoneSize { get; set; }
        public int GossipStoneSpacing { get; set; }
        public Color GossipStoneBackColor { get; set; }
        public int PathGoalSpacing { get; set; }
        public int NbMaxRows { get; set; }
        public bool isScrollable { get; set; }
        public bool isBroadcastable { get; set; }
        public bool PathCycling { get; set; } = false;
        public bool isMarkable { get; set; } = true;
        public string OuterPathID { get; set; }
        public HintPanelType isWotH;
        PictureBoxSizeMode SizeMode;
        TextBlock LabelSettings;

        public PanelWothBarren(ObjectPanelWotH data, Settings settings, Dictionary<string, string> PlacesWithTag, Dictionary<string, string> keycodesWithTag)
        {
            Settings = settings;
            IsVisible = data.Visible;

            GossipStoneSize = data.GossipStoneSize;
            Background = new SolidColorBrush(new Avalonia.Media.Color(data.BackColor.A,  data.BackColor.R, data.BackColor.G, data.BackColor.B));
            SetTop(this, data.Y);
            SetLeft(this, data.X);
            this.Name = data.Name;
            this.Width = data.Width;
            Height = data.Height;
            this.GossipStoneCount = data.GossipStoneCount.HasValue ? data.GossipStoneCount.Value : settings.DefaultWothGossipStoneCount;
            this.PathGoalCount = data.PathGoalCount.HasValue ? data.PathGoalCount.Value : settings.DefaultPathGoalCount;
            this.GossipStoneSpacing = data.GossipStoneSpacing;
            this.GossipStoneBackColor = (data.GossipStoneBackColor != null) ? data.GossipStoneBackColor : data.BackColor;
            this.PathGoalSpacing = data.PathGoalSpacing;
            this.IsTabStop = false;
            this.isScrollable = data.IsScrollable;
            this.SizeMode = data.SizeMode;
            this.PathCycling = data.PathCycling;
            this.isBroadcastable = data.isBroadcastable;
            this.isWotH = HintPanelType.WotH;
            this.isMarkable = data.isMarkable;
            this.ClipToBounds =  true;
            if (data.IsScrollable)
                this.PointerWheelChanged += Panel_MouseWheel;

            ListImage_WothItemsOption = data.GossipStoneImageCollection ?? Settings.DefaultGossipStoneImages;
            ListImage_GoalsOption = data.PathGoalImageCollection ?? Settings.DefaultPathGoalImages;
            NbMaxRows = data.NbMaxRows;

            KeycodesWithTag = keycodesWithTag;
            OuterPathID = data.OuterPathID;

            LabelSettings = new TextBlock
            {
                Foreground = new SolidColorBrush(new Avalonia.Media.Color(data.LabelColor.A, data.LabelColor.R, data.LabelColor.G, data.LabelColor.B)),
                Background = new SolidColorBrush(new Avalonia.Media.Color(data.LabelBackColor.A, data.LabelBackColor.R, data.LabelBackColor.G, data.LabelBackColor.B)),
                FontFamily = data.LabelFontName,
                FontSize = data.LabelFontSize * 1.4,
                //TODO: fontsytle fixes
                // Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.Width,
                Height = data.LabelHeight,
            };

            textBoxCustom = new TextBoxCustom
            (
                Settings,
                PlacesWithTag,
                new Point(0, 0),
                data.TextBoxBackColor,
                new FontData(data.TextBoxFontName, data.TextBoxFontSize, data.TextBoxFontStyle),
                data.TextBoxName,
                new Size(data.Width, data.TextBoxHeight),
                data.TextBoxText,
                isWotH,
                (PathGoalCount > 0 || OuterPathID != null),
                KeycodesWithTag
            );
            textBoxCustom.KeyDown += textBoxCustom_KeyDown_WotH;
            textBoxCustom.PointerPressed += textBoxCustom_MouseClick;
            this.Children.Add(textBoxCustom);
        }

        public PanelWothBarren(ObjectPanelBarren data, Settings settings, Dictionary<string, string> PlacesWithTag)
        {
            Settings = settings;
            IsVisible = data.Visible;
        
            Background = new SolidColorBrush(new Avalonia.Media.Color(data.BackColor.A,  data.BackColor.R, data.BackColor.G, data.BackColor.B));
            SetTop(this, data.Y);
            SetLeft(this, data.X);
            this.Name = data.Name;
            this.Width = data.Width;
            Height = data.Height;
            this.IsTabStop = false;
            this.ClipToBounds =  true;
            this.isWotH = HintPanelType.Barren;
            if (data.IsScrollable)
                this.PointerWheelChanged += Panel_MouseWheel;
        
        
            NbMaxRows = data.NbMaxRows;
        
            LabelSettings = new TextBlock()
            {
                Foreground = new SolidColorBrush(new Avalonia.Media.Color(data.LabelColor.A, data.LabelColor.R, data.LabelColor.G, data.LabelColor.B)),
                Background = new SolidColorBrush(new Avalonia.Media.Color(data.LabelBackColor.A, data.LabelBackColor.R, data.LabelBackColor.G, data.LabelBackColor.B)),
                FontFamily = data.LabelFontName,
                FontSize = data.LabelFontSize * 1.4,
                //TODO: fontsytle fixes
                // Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.Width,
                Height = data.LabelHeight
            };
        
            textBoxCustom = new TextBoxCustom
                (
                    Settings,
                    PlacesWithTag,
                    new Point(0, 0),
                    data.TextBoxBackColor,
                    new FontData(data.TextBoxFontName, data.TextBoxFontSize, data.TextBoxFontStyle),
                    data.TextBoxName,
                    new Size(data.Width, data.TextBoxHeight),
                    data.TextBoxText,
                    isWotH
                );
            textBoxCustom.KeyDown += textBoxCustom_KeyDown_Barren;
            textBoxCustom.PointerPressed += textBoxCustom_MouseClick;
            this.Children.Add(textBoxCustom);
        }
        
        public PanelWothBarren(ObjectPanelQuantity data, Settings settings, Dictionary<string, string> PlacesWithTag)
        {
            Settings = settings;
            IsVisible = data.Visible;
        
            Background = new SolidColorBrush(new Avalonia.Media.Color(data.BackColor.A,  data.BackColor.R, data.BackColor.G, data.BackColor.B));
            SetTop(this, data.Y);
            SetLeft(this, data.X);
            this.Name = data.Name;
            this.Width = data.Width;
            Height = data.Height;
            this.IsTabStop = false;
            this.CounterFontSize = data.CounterFontSize;
            this.CounterSpacing = data.CounterSpacing;
            this.GossipStoneSize = data.CounterSize;
            this.isScrollable = data.IsScrollable;
            this.CounterImage = "dk64/blank.png";
            this.subBoxSize = data.SubTextBoxSize;
            this.isWotH = HintPanelType.Quantity;
            this.ClipToBounds =  true;
            if (data.IsScrollable)
                this.PointerWheelChanged += Panel_MouseWheel;
        
        
            NbMaxRows = data.NbMaxRows;
        
            LabelSettings = new TextBlock
            {
                Foreground = new SolidColorBrush(new Avalonia.Media.Color(data.LabelColor.A, data.LabelColor.R, data.LabelColor.G, data.LabelColor.B)),
                Background = new SolidColorBrush(new Avalonia.Media.Color(data.LabelBackColor.A, data.LabelBackColor.R, data.LabelBackColor.G, data.LabelBackColor.B)),
                FontFamily = data.LabelFontName,
                FontSize = data.LabelFontSize * 1.4,
                //TODO: fontsytle fixes
                // Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.Width,
                Height = data.LabelHeight
            };
        
            textBoxCustom = new TextBoxCustom
                (
                    Settings,
                    PlacesWithTag,
                    new Point(0, 0),
                    data.TextBoxBackColor,
                    new FontData(data.TextBoxFontName, data.TextBoxFontSize, data.TextBoxFontStyle),
                    data.TextBoxName,
                    new Size(data.Width, data.TextBoxHeight),
                    data.TextBoxText,
                    isWotH
                );
            textBoxCustom.KeyDown += textBoxCustom_KeyDown_Quantity;
            textBoxCustom.PointerPressed += textBoxCustom_MouseClick;
            this.Children.Add(textBoxCustom);
        }
        
        
        public PanelWothBarren(ObjectPanelMixed data, Settings settings, Dictionary<string, string> PlacesWithTag, Dictionary<string, string> keycodesWithTag)
        {
            // the common stuff
            Settings = settings;
            IsVisible = data.Visible;
            Background = new SolidColorBrush(new Avalonia.Media.Color(data.BackColor.A,  data.BackColor.R, data.BackColor.G, data.BackColor.B));
            SetTop(this, data.Y);
            SetLeft(this, data.X);
            this.Name = data.Name;
            this.Width = data.Width;
            Height = data.Height;
            this.IsTabStop = false;
            this.ClipToBounds =  true;
            this.isScrollable = data.IsScrollable;
            if (data.IsScrollable)
                this.PointerWheelChanged += Panel_MouseWheel;
        
            LabelSettings = new TextBlock
            {
                Foreground = new SolidColorBrush(Avalonia.Media.Color.FromUInt32(0)),
                Background = new SolidColorBrush(Avalonia.Media.Color.FromUInt32(0)),
                FontFamily = data.LabelFontName,
                FontSize = data.LabelFontSize * 1.4,
                //TODO: fontsytle fixes
                // Font = new Font(data.LabelFontName, data.LabelFontSize, data.LabelFontStyle),
                Width = data.Width,
                Height = data.LabelHeight
            };
        
            NbMaxRows = data.NbMaxRows;
            KeycodesWithTag = keycodesWithTag;
            this.CounterImage = "dk64/blank.png";
            isWotH = HintPanelType.Mixed;
        
            textBoxCustom = new TextBoxCustom
                (
                    Settings,
                    PlacesWithTag,
                    new Point(0, 0),
                    data.DefaultTextBoxBackColor,
                    new FontData(data.DefaultTextBoxFontName, data.DefaultTextBoxFontSize, data.DefaultTextBoxFontStyle),
                    data.DefaultTextBoxName,
                    new Size(data.Width, data.DefaultTextBoxHeight),
                    data.DefaultTextBoxText,
                    isWotH,
                    kpt:KeycodesWithTag
                );
            textBoxCustom.KeyDown += textBoxCustom_KeyDown_Mixed;
            textBoxCustom.PointerPressed += textBoxCustom_MouseClick;
            this.Children.Add(textBoxCustom);
        
            // drop subs into subs
            int badcount = 0;
            foreach (var sub in data.SubPanels)
            {
                sub.Order = badcount;
                ListSubs.Add(sub);
                badcount++;
            }
            textBoxCustom.ListSubs = ListSubs;
        }

        public void UpdateFromSettings()
        {
            foreach (var hint in ListHints)
            {
                hint.UpdateFromSettings();
            }
        }


        private void Panel_MouseWheel(object sender, PointerWheelEventArgs e)
        {
            var panel = (PanelWothBarren)sender;
            if (e.Delta.Y != 0)
            {
                // SuspendLayout();
                //really wish i didnt have to go through this foreach twice but thats just the order it has to be processed, unfortunately
                foreach (var element in panel.Children)
                {
                    if (element is GossipStone { hoveredOver: true })
                        return;
                    if (element is CollectedItem { hoveredOver: true })
                        return;
                    if (element is TextBoxCustom { SuggestionContainerIsFocus: true })
                        return;
                }
                int moveDirection;
                if (e.Delta.Y < 0) moveDirection = -15; else moveDirection = +15;
                foreach (Control element in panel.Children)
                {
                    if (element is TextBlock la)
                        SetTop(la, GetTop(la) + moveDirection);
                    if (element is GossipStone gs)
                        SetTop(gs, GetTop(gs) + moveDirection);
                    if (element is TextBoxCustom tb)
                        SetTop(tb, GetTop(tb) + moveDirection);
                    if (element is CollectedItem ci)
                        SetTop(ci, GetTop(ci) + moveDirection);
                }
                // ResumeLayout(false);
            }
            panel.SetSuggestionContainer();
        }
        
        public void SetSuggestionContainer()
        {
            textBoxCustom.SetSuggestionsContainerLocation(new Point((int)Canvas.GetLeft(this), (int)Canvas.GetTop(this)));
            textBoxCustom.SuggestionContainer.ZIndex = 3;
        }

        private void textBoxCustom_MouseClick(object? sender, PointerEventArgs e)
        {
            ((TextBoxCustom)sender).Text = string.Empty;
        }

        private void textBoxCustom_KeyDown_Barren(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                // e.SuppressKeyPress = true;
                var textbox = (TextBoxCustom)sender;
                if (ListHints.Count < NbMaxRows)
                {
                    AddBarren(textbox.Text);
                }
                textbox.Text = string.Empty;
            }
        }

        private void textBoxCustom_KeyDown_Quantity(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                // e.SuppressKeyPress = true;
                var textbox = (TextBoxCustom)sender;
                if (ListHints.Count < NbMaxRows)
                {
                    if (textbox.Text != string.Empty)
                    {
                        AddQuantity(textbox.Lines[2], textbox.Lines[1]);
                    }
                }
                textbox.Text = string.Empty;
            }
        }

        private void textBoxCustom_KeyDown_WotH(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                // e.SuppressKeyPress = true;

                var textbox = (TextBoxCustom)sender;
                if (ListHints.Count < NbMaxRows)
                {
                    if (textbox.Text != string.Empty)
                    {
                        AddWotH(textbox.Lines[2], textbox.Lines[1]);
                    }
                }
                textbox.Text = string.Empty;
            }
        }


        private void textBoxCustom_KeyDown_Mixed(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                // e.SuppressKeyPress = true;

                var textbox = (TextBoxCustom)sender;
                if (ListHints.Count < NbMaxRows)
                {
                    if (textbox.Text != string.Empty)
                    {
                        // check for keycode; if no keycode, play the error noise and wipe the text
                        // if keycode, check all in ListSub for a match and record their type
                        // switch case the type, then add the respective AddWoth/Barren/Quantity
                        
                        if (textbox.Lines[0] == "" && textbox.Lines[1] == "")
                        {
                            // SystemSounds.Beep.Play();
                            textbox.Text = "Missing all keycodes";
                            textBoxCustom.isErrorMessage = true;
                            return;
                        }
                        if (textbox.Lines[2] == "")
                        {
                            // SystemSounds.Beep.Play();
                            textbox.Text = "Missing hint text";
                            textBoxCustom.isErrorMessage = true;
                            return;
                        }
                        MixedSubPanels foundsub = null;
                        var foundkeycode = "";
                        var foundtext = "";
                        foreach (var sub in ListSubs)
                        {
                            if (sub.Keycode == textbox.Lines[0])
                            {
                                foundsub = sub;
                                foundkeycode = textbox.Lines[1];
                                foundtext = textbox.Lines[2];
                                break;
                            }
                        }

                        if (foundsub == null)
                        {
                            // SystemSounds.Beep.Play();
                            textbox.Text = "Invalid/Missing Hint Keycode";
                            textBoxCustom.isErrorMessage = true;
                            return;
                        }

                        switch (foundsub.Type)
                        {
                            case "none":
                                textbox.Text = string.Empty;
                                return;
                            case "WotH":
                                AddWotH(foundtext, foundkeycode, foundsub);
                                break;
                            case "Barren":
                                AddBarren(foundtext, foundsub);
                                break;
                            case "Quantity":
                                AddQuantity(foundtext, foundkeycode, foundsub);
                                break;
                        }
                       
                    }
                }
                textbox.Text = string.Empty;
            }
        }




        private void AddWotH(string text, string codestring="", MixedSubPanels Sub=null)
        {
            Dictionary<string, string> FoundKeycodes = new Dictionary<string, string> { };
            string selectedPlace;
            int usedPathGoalCount = PathGoalCount;
            string usedOuterPathID = OuterPathID;
            if (Sub != null)
            {
                usedPathGoalCount = (int)Sub.PathGoalCount;
                usedOuterPathID = null;
            }
            if ((codestring!="" || codestring != string.Empty) && (usedPathGoalCount > 0 || usedOuterPathID != null) && Settings.HintPathAutofill)
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
            if (Settings.EnableDuplicateWoth || ListHints.All(x => x.Name != selectedPlace))
            {
                var newlocation = (ListHints.Count <= 0) ? new Point(0, (int)-LabelSettings.Height) : new Point((int)GetLeft(ListHints.Last().LabelPlace), (int)GetTop(ListHints.Last().LabelPlace));
                WotH newWotH = null;
                if (isWotH == HintPanelType.Mixed)
                {
                    TextBlock tempLabel = new TextBlock()
                    {
                        Foreground = new SolidColorBrush(new Avalonia.Media.Color(Sub.LabelColor.A, Sub.LabelColor.R, Sub.LabelColor.G,Sub.LabelColor.B)),
                        Background = new SolidColorBrush(new Avalonia.Media.Color(Sub.LabelBackColor.A, Sub.LabelBackColor.R, Sub.LabelBackColor.G, Sub.LabelBackColor.B)),
                        FontFamily = LabelSettings.FontFamily,
                        FontSize = LabelSettings.FontSize,
                        FontWeight = LabelSettings.FontWeight,
                        FontStyle =  LabelSettings.FontStyle,
                        Width = LabelSettings.Width,
                        Height = LabelSettings.Height
                    };
                    
                    newWotH = new WotH(Settings, selectedPlace,
                            (Sub.GossipStoneCount ?? Settings.DefaultWothGossipStoneCount), (Sub.GossipStoneImageCollection ?? Settings.DefaultGossipStoneImages), Sub.GossipStoneSpacing,
                            (Sub.PathGoalCount ?? Settings.DefaultPathGoalCount), (Sub.PathGoalImageCollection ?? Settings.DefaultPathGoalImages), Sub.PathGoalSpacing,
                            newlocation, tempLabel, Sub.GossipStoneSize, Sub.GossipStoneBackColor, this.isScrollable, Sub.SizeMode, Sub.isBroadcastable, Sub.PathCycling, Sub.isMarkable);
                    newWotH.PlacedOrder = Sub.Order;
                } else
                {
                    newWotH = new WotH(Settings, selectedPlace,
                            GossipStoneCount, ListImage_WothItemsOption, GossipStoneSpacing,
                            PathGoalCount, ListImage_GoalsOption, PathGoalSpacing,
                            newlocation, LabelSettings, GossipStoneSize, this.GossipStoneBackColor, this.isScrollable, this.SizeMode, this.isBroadcastable, this.PathCycling, this.isMarkable);
                }

                ListHints.Add(newWotH);
                this.Children.Add(newWotH.LabelPlace);
                newWotH.LabelPlace.PointerPressed += LabelPlace_MouseClick;
                foreach (var gossipStone in newWotH.listGossipStone)
                {
                    this.Children.Add(gossipStone);
                }
                
                
                if (FoundKeycodes.Count > 0)
                {
                    if (usedPathGoalCount > 0)
                    {
                        GossipStone pathStone = null;
                        int y = 0;
                        foreach (var z in FoundKeycodes)
                        {
                            // spreads the paths evenly across multiple path stones (if applicable)
                            pathStone = newWotH.listGossipStone.Where(x => x.Name == newWotH.Name + $"_GoalGossipStone{y}").ToList()[0];
                            if (!pathStone.HeldImages.Contains(z.Value)) pathStone.HeldImages.Add(z.Value);
                            if (usedPathGoalCount > 1) y = (y + 1) % (usedPathGoalCount);
                            pathStone.HoldsImage = true;
                            pathStone.UpdateImage();
                        }
                    } else if (OuterPathID != null)
                    {
                        // GSTForms f = (GSTForms)this.FindForm();
                        // foreach (var z in FoundKeycodes)
                        // {
                        //     var search = f.Controls[0].Controls.OfType<Item>().Where(x => x.OuterPathID == $"{OuterPathID}_{z.Key.ToUpper()}").ToList();
                        //     if (search.Count > 0) search[0].SetState(1);
                        // }
                    }
                }
                SortAndReorderHints();
            }
        }

        private void AddBarren(string text, MixedSubPanels Sub = null)
        {
            var selectedPlace = text.ToUpper().Trim().Replace(",", "");
            
            // prevent dupes
            if ((ListHints.All(x => x.Name != selectedPlace)) || Sub != null)
            {
                Barren newBarren = null;
                if (isWotH == HintPanelType.Mixed)
                {
                    TextBlock tempLabel = new TextBlock()
                    {
                        Foreground = new SolidColorBrush(new Avalonia.Media.Color(Sub.LabelColor.A, Sub.LabelColor.R, Sub.LabelColor.G,Sub.LabelColor.B)),
                        Background = new SolidColorBrush(new Avalonia.Media.Color(Sub.LabelBackColor.A, Sub.LabelBackColor.R, Sub.LabelBackColor.G, Sub.LabelBackColor.B)),
                        FontFamily = LabelSettings.FontFamily,
                        FontSize = LabelSettings.FontSize,
                        FontWeight = LabelSettings.FontWeight,
                        FontStyle =  LabelSettings.FontStyle,
                        Width = LabelSettings.Width,
                        Height = LabelSettings.Height
                    };
                    var newlocation = (ListHints.Count <= 0) ? new Point(0, (int)-LabelSettings.Height) : new Point((int)Canvas.GetLeft(ListHints.Last().LabelPlace), (int)Canvas.GetTop(ListHints.Last().LabelPlace));
                    newBarren = new Barren(Settings, selectedPlace, newlocation, tempLabel);
                    newBarren.PlacedOrder = Sub.Order;
                }
                else
                {
                    var newlocation = (ListHints.Count <= 0) ? new Point(0, (int)-LabelSettings.Height) : new Point((int)Canvas.GetLeft(ListHints.Last().LabelPlace), (int)Canvas.GetTop(ListHints.Last().LabelPlace));
                    newBarren = new Barren(Settings, selectedPlace, newlocation, LabelSettings);
            
                }
            
                ListHints.Add(newBarren);
                this.Children.Add(newBarren.LabelPlace);
                newBarren.LabelPlace.PointerPressed += LabelPlace_MouseClick;
                SortAndReorderHints();
            }
        }

        private void AddQuantity(string text, string codestring = "", MixedSubPanels Sub=null)
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
            if (ListHints.All(x => x.Name != selectedPlace) || isWotH == HintPanelType.Mixed)
            {
                Quantity newQuan = null;
                if (isWotH == HintPanelType.Mixed)
                {
                    TextBlock tempLabel = new TextBlock()
                    {
                        Foreground = new SolidColorBrush(new Avalonia.Media.Color(Sub.LabelColor.A, Sub.LabelColor.R, Sub.LabelColor.G,Sub.LabelColor.B)),
                        Background = new SolidColorBrush(new Avalonia.Media.Color(Sub.LabelBackColor.A, Sub.LabelBackColor.R, Sub.LabelBackColor.G, Sub.LabelBackColor.B)),
                        FontFamily = LabelSettings.FontFamily,
                        FontSize = LabelSettings.FontSize,
                        FontWeight = LabelSettings.FontWeight,
                        FontStyle =  LabelSettings.FontStyle,
                        Width = LabelSettings.Width,
                        Height = LabelSettings.Height
                    };
                    var newlocation = (ListHints.Count <= 0) ? new Point(2, (int)-LabelSettings.Height) : new Point((int)Canvas.GetLeft(ListHints.Last().LabelPlace), (int)Canvas.GetTop(ListHints.Last().LabelPlace));
                    newQuan = new Quantity(Settings, selectedPlace,
                            Sub.CounterFontSize, Sub.CounterSpacing, CounterImage,
                            Sub.SubTextBoxSize, Color.FromArgb(((ISolidColorBrush)tempLabel.Background).Color.A, ((ISolidColorBrush)tempLabel.Background).Color.R, ((ISolidColorBrush)tempLabel.Background).Color.G, ((ISolidColorBrush)tempLabel.Background).Color.B),
                            newlocation, tempLabel, Sub.CounterSize, this.isBroadcastable);
                    newQuan.PlacedOrder = Sub.Order;
                } else
                {
                    var newlocation = (ListHints.Count <= 0) ? new Point(2, (int)-LabelSettings.Height) : new Point((int)Canvas.GetLeft(ListHints.Last().LabelPlace), (int)Canvas.GetTop(ListHints.Last().LabelPlace));
                    newQuan = new Quantity(Settings, selectedPlace,
                            CounterFontSize, CounterSpacing, CounterImage,
                            subBoxSize, Color.FromArgb(((ISolidColorBrush)LabelSettings.Background).Color.A, ((ISolidColorBrush)LabelSettings.Background).Color.R, ((ISolidColorBrush)LabelSettings.Background).Color.G, ((ISolidColorBrush)LabelSettings.Background).Color.B),
                            newlocation, LabelSettings, GossipStoneSize, this.isBroadcastable);
            
                }
            
                ListHints.Add(newQuan);
                this.Children.Add(newQuan.LabelPlace);
                newQuan.LabelPlace.PointerPressed += LabelPlace_MouseClick;
            
                this.Children.Add(newQuan.leftCounterCI);
                this.Children.Add(newQuan.leftCounterCI.ItemCount);
                this.Children.Add(newQuan.rightCounterCI);
                this.Children.Add(newQuan.rightCounterCI.ItemCount);
            
                SortAndReorderHints();
            
                if (foundin != 0)
                {
                    newQuan.rightCounterCI.SetState(foundin);
                }
            
            }
        }

        private void SortAndReorderHints()
        {
            // sort by their placedorder
            ListHints = ListHints.OrderBy(i => i.PlacedOrder).ToList();
            for (int i = 0; i < ListHints.Count; i++)
            {
                if (ListHints[i] is WotH w)
                {
                    var wothLabel = w.LabelPlace;
                    var newY = i * wothLabel.Height;
                    SetTop(wothLabel, newY);

                    for (int j = 0; j < w.listGossipStone.Count; j++)
                    {
                        SetTop(w.listGossipStone[j], newY);
                    }
                }
                else if (ListHints[i] is Quantity q)
                {
                    var wothLabel = q.LabelPlace;
                    var newY = i * wothLabel.Height;
                    SetTop(wothLabel, newY);
                    SetTop(q.leftCounterCI, newY);
                    SetTop(q.leftCounterCI.ItemCount, newY);
                    SetTop(q.rightCounterCI, newY);
                    SetTop(q.rightCounterCI.ItemCount, newY);
                }
                else if (ListHints[i] is Barren b)
                {
                    var wothLabel = b.LabelPlace;
                    SetTop(wothLabel, (i * wothLabel.Height));
                }
            }
            textBoxCustom.newLocation(new Point(0, (int)(GetTop(ListHints.Last().LabelPlace) + ListHints.Last().LabelPlace.Height)), new Point((int)Canvas.GetLeft(this), (int)Canvas.GetTop(this)));
        }

        private void LabelPlace_MouseClick(object sender, PointerEventArgs e)
        {
            if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton))
            {
                var label = (LabelExtended)sender;
                var hint = this.ListHints.Where(x => x.LabelPlace.Name == label.Name).ToList()[0];
                this.RemoveHint(hint);
            }
        }

        public void RemoveHint(PanelHint hint)
        {
            ListHints.Remove(hint);

            if (hint is WotH woth)
            {
                this.Children.Remove(woth.LabelPlace);
                foreach (var gossipStone in woth.listGossipStone)
                {
                    gossipStone.TryToKill();
                    this.Children.Remove(gossipStone);
                }

            } 
            else if (hint is Barren barren) {
                this.Children.Remove(barren.LabelPlace);
            }
            else if (hint is Quantity quantity) {
                this.Children.Remove(quantity.LabelPlace);
                this.Children.Remove(quantity.leftCounterCI);
                this.Children.Remove(quantity.leftCounterCI.ItemCount);
                this.Children.Remove(quantity.rightCounterCI);
                this.Children.Remove(quantity.rightCounterCI.ItemCount);
            }

            for (int i = 0; i < ListHints.Count; i++)
            {
                if (ListHints[i] is WotH w)
                {
                    var wothLabel = w.LabelPlace;
                    var newY = i * wothLabel.Height;
                    SetTop(wothLabel, newY);

                    for (int j = 0; j < w.listGossipStone.Count; j++)
                    {
                        SetTop(w.listGossipStone[j], newY);
                    }
                } else if (ListHints[i] is Quantity q)
                {
                    var wothLabel = q.LabelPlace;
                    var newY = i * wothLabel.Height;
                    SetTop(wothLabel, newY);
                    SetTop(q.leftCounterCI, newY);
                    SetTop(q.leftCounterCI.ItemCount, newY);
                    SetTop(q.rightCounterCI, newY);
                    SetTop(q.rightCounterCI.ItemCount, newY);
                }
                else if (ListHints[i] is Barren b)
                {
                    var wothLabel = b.LabelPlace;
                    SetTop(wothLabel, (i * wothLabel.Height));
                }
            }
            textBoxCustom.newLocation(new Point(0, (int)(ListHints.Count * LabelSettings.Height)), new Point((int)Canvas.GetLeft(this), (int)Canvas.GetTop(this)));
        }

        public string GetAlls()
        {
            // basically a getwoth/getbarn/getquan but attached the placedorder
            string thestring = "";
            foreach (var hint in ListHints)
            {
                if (thestring.Length > 0)
                {
                    thestring += "\n";
                }

                if (hint is WotH w)
                {
                    thestring += w.PlacedOrder + "," + w.SaveState().ToString();
                }
                if (hint is Barren b)
                {
                    thestring += b.PlacedOrder + "," + b.SaveState().ToString();
                }
                if (hint is Quantity q)
                {
                    thestring += q.PlacedOrder + "," + q.SaveState().ToString();
                }
            }




            return thestring;
        }

        public void SetAlls(string thestring)
        {
            string[] sections = thestring.Split('\n');
            foreach (string section in sections)
            {
                string[] pieces = section.Split(new char[] { ',' }, count:2);
                //pieces[0] is the placedorder for the section
                //pieces[1] is the text you dump into the respective function
                MixedSubPanels foundsub = ListSubs[int.Parse(pieces[0])];


                switch (foundsub.Type)
                {
                    case "none":
                        return;
                    case "WotH":
                        SetWoth2(pieces[1], foundsub);
                        break;
                    case "Barren":
                        SetBarren2(pieces[1], foundsub);
                        break;
                    case "Quantity":
                        SetQuantities2(pieces[1], foundsub);
                        break;
                }


            }
        }


        public List<WotHState> GetWotHs()
        {
            List<WotHState> thelist = new List<WotHState>();
            foreach (WotH x in ListHints)
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
                SetWoth2(section);
            }
        }

        private void SetWoth2(string section, MixedSubPanels sub = null)
        {
            // break into name & colour         and           stones
            string[] parts = section.Split('\t');

            // name = firstpart[0]
            // color = firstpart[1]
            string[] firstPart = parts[0].Split(',');

            // secondparts are explained below
            string[] secondPart = parts[1].Split(',');
            AddWotH(firstPart[0], Sub:sub);

            // find the woth we just made. the -1 is there so that if there are duplicates it tries the last one
            //Control foundWotH = this.Controls.Find(firstPart[0], true)[0];
            WotH thisWotH = (WotH)(ListHints.Where(x => x.Name == firstPart[0].Trim()).ToList()[-1]);

            thisWotH.SetColor(int.Parse(firstPart[1]));

            GossipStone foundStone = null;
            bool storedHoldsImage = false;
            List<string> storedHeldImageName = null;
            int storedImageIndex = 0;
            if (secondPart.Length > 4)
            {
                for (int i = 0; i < secondPart.Length; i++)
                {
                    switch (i % 5)
                    {
                        case 0:
                            // 0th is the name
                            foundStone = (GossipStone)(this.Children.First(o => o.Name == secondPart[i]));
                            break;
                        case 1:
                            // 1st is the bool
                            storedHoldsImage = bool.Parse(secondPart[i]);
                            break;
                        case 2:
                        {
                            // 2nd is the stored image
                            string[] images = (secondPart[i]).Split('|');
                            storedHeldImageName = images.ToList();
                            break;
                        }
                        case 3:
                            // 3rd is the stateindex
                            storedImageIndex = int.Parse(secondPart[i]);
                            break;
                        case 4:
                            // 4th is 
                            // also we have all 4 so go and set the state
                            foundStone.SetState(new GossipStoneState() { HoldsImage = storedHoldsImage, HeldImages = storedHeldImageName, ImageIndex = storedImageIndex, isMarked = (MarkedImageIndex)int.Parse(secondPart[i]) });
                            break;
                    }
                }
            }
        }


        public List<BarrenState> GetBarrens()
        {
            List<BarrenState> thelist = new List<BarrenState>();
            foreach (Barren x in ListHints)
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
                SetBarren2(section);
            }
        }
        
        private void SetBarren2(string section, MixedSubPanels sub=null)
        {
            // name = firstpart[0]
            // color = firstpart[1]
            string[] firstPart = section.Split(',');
        
            AddBarren(firstPart[0], Sub:sub);
        
            // find the woth we just made. the -1 is there so that if there are duplicates it tries the last one
            Barren thisBarren = (Barren)(ListHints.Where(x => x.Name == firstPart[0]).ToList()[-1]);
        
            thisBarren.SetColor(int.Parse(firstPart[1]));
        }
        
        public List<QuantityState> GetQuantities()
        {
            List<QuantityState> thelist = new List<QuantityState>();
            foreach (Quantity x in ListHints)
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
                SetQuantities2(section);
            }
        }
        
        private void SetQuantities2(string section, MixedSubPanels sub=null)
        {
            // name = firstpart[0]
            // color = firstpart[1]
            string[] firstPart = section.Split(',');
        
            AddQuantity(firstPart[0], Sub:sub);
        
            // find the woth we just made. the -1 is there so that if there are duplicates it tries the last one
            Quantity thisQuan = (Quantity)(ListHints.Where(x => x.Name == firstPart[0]).ToList()[-1]);
        
            thisQuan.SetColor(int.Parse(firstPart[1]));
            thisQuan.leftCounterCI.SetState(int.Parse(firstPart[2]));
            thisQuan.rightCounterCI.SetState(int.Parse(firstPart[3]));
        
        }


        public void SetVisible(bool visible)
        {
            IsVisible = visible;
        }

        public void SpecialtyImport(object ogPoint, string name, object value, int mult)
        {
            //var point = (ObjectPoint)ogPoint;
            // switch (name)
            // {
            //     case "TextBoxName":
            //         if (mult > 0) textBoxCustom.Name = value.ToString();
            //         else textBoxCustom.Name = (string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //     case "TextBoxBackColor":
            //         if (mult > 0)
            //         {
            //             // format: `1,2,3`
            //             var newrgb = value?.ToString().Split(',');
            //             Color tempColor;
            //             // if there isnt more than 1 response, assume its a word and not rgb
            //             if (newrgb.Length > 1) tempColor = Color.FromArgb(int.Parse(newrgb[0]), int.Parse(newrgb[1]), int.Parse(newrgb[2]));
            //             else tempColor = Color.FromName(value.ToString());
            //             textBoxCustom.BackColor = tempColor;
            //         }
            //         else textBoxCustom.BackColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //     case "TextBoxFontName":
            //         if (mult > 0) textBoxCustom.Font = new Font(value.ToString(), textBoxCustom.Font.Size, textBoxCustom.Font.Style);
            //         else textBoxCustom.Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), textBoxCustom.Font.Size, textBoxCustom.Font.Style);
            //         break;
            //     case "TextBoxFontSize":
            //         textBoxCustom.Font = new Font(textBoxCustom.Font.Name, textBoxCustom.Font.Size + (mult * int.Parse(value.ToString())), textBoxCustom.Font.Style);
            //         break;
            //     case "TextBoxFontStyle":
            //         if (mult > 0) textBoxCustom.Font = new Font(textBoxCustom.Font.FontFamily, textBoxCustom.Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
            //         else textBoxCustom.Font = new Font(textBoxCustom.Font.FontFamily, textBoxCustom.Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
            //         break;
            //     case "TextBoxHeight":
            //         textBoxCustom.TextBoxField.Size = new Size(textBoxCustom.TextBoxField.Size.Width, textBoxCustom.TextBoxField.Size.Height + (mult * int.Parse(value.ToString())));
            //         break;
            //     case "TextBoxText":
            //         if (mult > 0) textBoxCustom.Text = value.ToString();
            //         else textBoxCustom.Text = (string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //
            //     case "LabelColor":
            //         if (mult > 0)
            //         {
            //             // format: `1,2,3`
            //             var newrgb = value?.ToString().Split(',');
            //             Color tempColor;
            //             // if there isnt more than 1 response, assume its a word and not rgb
            //             if (newrgb.Length > 1) tempColor = Color.FromArgb(int.Parse(newrgb[0]), int.Parse(newrgb[1]), int.Parse(newrgb[2]));
            //             else tempColor = Color.FromName(value.ToString());
            //             LabelSettings.ForeColor = tempColor;
            //         }
            //         else LabelSettings.ForeColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //     case "LabelBackColor":
            //         if (mult > 0)
            //         {
            //             // format: `1,2,3`
            //             var newrgb = value?.ToString().Split(',');
            //             Color tempColor;
            //             // if there isnt more than 1 response, assume its a word and not rgb
            //             if (newrgb.Length > 1) tempColor = Color.FromArgb(int.Parse(newrgb[0]), int.Parse(newrgb[1]), int.Parse(newrgb[2]));
            //             else tempColor = Color.FromName(value.ToString());
            //             LabelSettings.BackColor = tempColor;
            //         }
            //         else LabelSettings.BackColor = (Color)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
            //         break;
            //     case "LabelFontName":
            //         if (mult > 0) LabelSettings.Font = new Font(value.ToString(), LabelSettings.Font.Size, LabelSettings.Font.Style);
            //         else LabelSettings.Font = new Font((string)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null), LabelSettings.Font.Size, LabelSettings.Font.Style);
            //         break;
            //     case "LabelFontSize":
            //         LabelSettings.Font = new Font(LabelSettings.Font.Name, LabelSettings.Font.Size + (mult* int.Parse(value.ToString())), LabelSettings.Font.Style);
            //         break;
            //     case "LabelFontStyle":
            //         if (mult > 0) LabelSettings.Font = new Font(LabelSettings.Font.FontFamily, LabelSettings.Font.Size, (FontStyle)Enum.Parse(typeof(FontStyle), value.ToString()));
            //         else LabelSettings.Font = new Font(LabelSettings.Font.FontFamily, LabelSettings.Font.Size, (FontStyle)ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null));
            //         break;
            //     case "LabelHeight":
            //         LabelSettings.Size = new Size(LabelSettings.Size.Width, LabelSettings.Size.Height + (mult * int.Parse(value.ToString())));
            //         break;
            //     case "LabelWidth":
            //     case "TextBoxWidth":
            //     case "PathGoalSize":
            //     case "CounterImage":
            //         // not a real property
            //         break;
            //     case "Width":
            //         // handled differently
            //         this.Width = this.Width + (mult * int.Parse(value.ToString()));
            //         LabelSettings.Size = new Size(this.Width, LabelSettings.Size.Height);
            //         textBoxCustom.Size = new Size(this.Width, textBoxCustom.Size.Height);
            //         textBoxCustom.TextBoxField.Size = new Size(this.Width, textBoxCustom.TextBoxField.Size.Height);
            //         break;
            //     default:
            //         throw new NotImplementedException($"Could not perform Hint Panel Specialty Import for property \"{name}\", as it has not yet been implemented. Go pester JXJacob to go fix it.");
            // }
        }

        public void ConfirmAlternates()
        {
            RefreshCells();
        }



        public void RefreshCells()
        {
            int tempcount = 0;

            foreach (var Hint in ListHints)
            {
                if (Hint is WotH woth)
                {
                    if (woth.PlacedOrder == -1) woth.RefreshLocation(GossipStoneCount, ListImage_WothItemsOption ?? Settings.DefaultGossipStoneImages, GossipStoneSpacing,
                            PathGoalCount, ListImage_GoalsOption ?? Settings.DefaultPathGoalImages, PathGoalSpacing,
                            (int)(tempcount * LabelSettings.Height), LabelSettings, GossipStoneSize, this.GossipStoneBackColor, this.isScrollable, this.SizeMode, this.isBroadcastable, this.PathCycling, this.isMarkable);
                    tempcount++;
                    foreach (var stone in woth.listGossipStone)
                    {
                        if (!this.Children.Contains(stone)) this.Children.Add(stone);
                    }
                }
                else if (Hint is Barren barr)
                {
                    if (barr.PlacedOrder == -1) barr.RefreshLocation((int)(tempcount * LabelSettings.Height), LabelSettings);
                    tempcount++;
                }
                else if (Hint is Quantity quan)
                {
                    if (quan.PlacedOrder == -1) quan.RefreshLocation(CounterFontSize, CounterSpacing, CounterImage,
                        subBoxSize, LabelSettings.Background,
                        (int)(tempcount * LabelSettings.Height), LabelSettings, GossipStoneSize);
                    tempcount++;
                }
            }
            if (ListHints.Count != 0) textBoxCustom.newLocation(new Point(0, (int)(GetTop(ListHints.Last().LabelPlace) + ListHints.Last().LabelPlace.Height)), new Point((int)Canvas.GetLeft(this), (int)Canvas.GetTop(this)));


        }
    }
}
