using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace GSTHD
{
    public interface UpdatableFromSettings
    {
        void UpdateFromSettings();
    }

    public class Layout
    {
        public List<GenericLabel> ListLabels { get; }  = [];
        public List<ObjectPointTextbox> ListTextBoxes { get; } = [];
        public List<ObjectPointGrid> ListTextBoxGrids { get; } = [];
        public List<ObjectPoint> ListItems { get; } = [];
        public List<ObjectPointGrid> ListItemGrids { get; } = [];
        public List<ObjectPointSong> ListSongs { get; } = [];
        public List<ObjectPoint> ListDoubleItems { get; } = [];
        public List<ObjectPointCollectedItem> ListCollectedItems { get; } = [];
        public List<ObjectPointMedallion> ListMedallions { get; } = [];
        public List<ObjectPoint> ListGuaranteedHints { get; } = [];
        public List<ObjectPoint> ListGossipStones { get; } = [];
        public List<ObjectPointGrid> ListGossipStoneGrids { get; } = [];
        public List<AutoFillTextBox> ListSometimesHints { get; } = [];
        public List<ObjectPanelWotH> ListPanelWotH { get; } = [];
        public List<ObjectPanelBarren> ListPanelBarren { get; } = [];
        public List<ObjectPanelQuantity> ListPanelQuantity { get; } = [];
        public List<ObjectPanelMixed> ListPanelMixed { get; } = [];
        public List<ObjectPanelSpoiler> ListPanelSpoiler { get; } = [];
        public List<ObjectPanelNowPlaying> ListPanelNowPlaying { get; } = [];

        public List<AlternateSettings> ListAlternates { get; } = [];

        public List<UpdatableFromSettings> ListUpdatables = [];


        List<IAlternatableObject> ControlsToBeUpdated = [];

        public AppSettings App_Settings = new();

        private GSTForms hostForm;
        
        // gonna lose my shit why does this work HERE????
        public static readonly Dictionary<MarkedImageIndex, AdvancedImage> Insufferable = new()
        {
            { MarkedImageIndex.mark_check, new AdvancedImage((Uri?)null) },
            { MarkedImageIndex.mark_x, new AdvancedImage((Uri?)null)},
            { MarkedImageIndex.mark_question, new AdvancedImage((Uri?)null) },
            { MarkedImageIndex.mark_star, new AdvancedImage((Uri?)null) },
        };
        

        public void UpdateFromSettings()
        {
            foreach (var updatable in ListUpdatables)
            {
                updatable.UpdateFromSettings();
            }
        }

        public void LoadLayout(Panel panelLayout, Settings settings, SortedSet<string> listSometimesHintsSuggestions, Dictionary<string, string> listPlacesWithTag, Dictionary<string, string> listKeycodesWithTag, GSTForms form)
        {
            hostForm = form;
            bool isOnBroadcast = (hostForm.Title == "GSTHD Broadcast View");

            foreach (var ffs in Insufferable)
            {
                ffs.Value.Source = $"Resources/{ffs.Key}.png";
            }


            ListUpdatables.Clear();
            if (settings.ActiveLayout == string.Empty) return;
            JObject json_layout;
            try
            {
                if (!isOnBroadcast)
                {
                    json_layout = JObject.Parse(File.ReadAllText(@"" + settings.ActiveLayout));
                    if (!json_layout.ContainsKey("AppSize"))
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                            "Layout file " + settings.ActiveLayout + " does not appear to contain any GSTHD layout data.\nReverting to dk64.json.",
                            ButtonEnum.Ok);
                        box.ShowAsync();
                        // set settings to dk64.json
                        settings.ActiveLayout = "Layouts\\dk64.json";
                        settings.Write();
                        // force reload
                        form.Reset(null);
                        return;
                    }
                } else
                {
                    var thepath = Settings.SinglePathReformat(
                        $"{Path.GetFileName(Path.GetDirectoryName(settings.ActiveLayout))}\\{settings.ActiveLayoutBroadcastFile}");
                    json_layout = JObject.Parse(File.ReadAllText(thepath));
                }
            } catch (JsonReaderException)
            {
                if (isOnBroadcast)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                        "File " + settings.ActiveLayoutBroadcastFile.ToString() + " appears to contian incorrect JSON formatting.\nClosing broadcast view.",
                        ButtonEnum.Ok);
                    box.ShowAsync();
                    ((BroadcastWindow)hostForm).Close();
                } else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                        "File " + settings.ActiveLayout.ToString() + " appears to contian incorrect JSON formatting.\nReverting to dk64.json.",
                        ButtonEnum.Ok);
                    box.ShowAsync();
                    // if this IS dk64.json and its having issues, force close so it doesnt loop
                    if (settings.ActiveLayout == "layouts\\dk64.json") ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).TryShutdown();
                    // set settings to dk64.json
                    settings.ActiveLayout = "layouts\\dk64.json";
                    settings.Write();
                }
                // force reload
                form.Reset(null);
                return;
            } catch (FileNotFoundException)
            {
                if (isOnBroadcast)
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                        "File " + settings.ActiveLayoutBroadcastFile.ToString() + " could not be found.\nClosing broadcast view.",
                        ButtonEnum.Ok);
                    box.ShowAsync();
                    ((BroadcastWindow)hostForm).Close();
                }
                else
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                        "File " + settings.ActiveLayout.ToString() + " could not be found.\nReverting to dk64.json.",
                        ButtonEnum.Ok);
                    box.ShowAsync();
                    // set settings to dk64.json
                    settings.ActiveLayout = "layouts\\dk64.json";
                    settings.Write();
                }
                // force reload
                form.Reset(null);
                return;
            }
                
            foreach (var category in json_layout)
            {
                switch (category.Key)
                {
                    case "AppSize":
                    {
                        App_Settings = JsonConvert.DeserializeObject<AppSettings>(category.Value.ToString());
                        break;
                    }
                    case "Alternates":
                    {
                        foreach (var element in category.Value)
                        {
                            ListAlternates.Add(JsonConvert.DeserializeObject<AlternateSettings>(element.ToString()));
                        }

                        break;
                    }
                    case "Labels":
                    {
                        foreach (var element in category.Value)
                        {
                            ListLabels.Add(JsonConvert.DeserializeObject<GenericLabel>(element.ToString()));
                        }

                        break;
                    }
                    case "TextBoxes":
                    {
                        foreach (var element in category.Value)
                        {
                            ListTextBoxes.Add(JsonConvert.DeserializeObject<ObjectPointTextbox>(element.ToString()));
                        }

                        break;
                    }
                    case "TextBoxGrids":
                    {
                        foreach (var element in category.Value)
                        {
                            ListTextBoxGrids.Add(JsonConvert.DeserializeObject<ObjectPointGrid>(element.ToString()));
                        }

                        break;
                    }
                    case "Items":
                    {
                        foreach (var element in category.Value)
                        {
                            ListItems.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }

                        break;
                    }
                    case "ItemGrids":
                    {
                        foreach (var element in category.Value)
                        {
                            ListItemGrids.Add(JsonConvert.DeserializeObject<ObjectPointGrid>(element.ToString()));
                        }

                        break;
                    }
                    case "Songs":
                    {
                        foreach (var element in category.Value)
                        {
                            ListSongs.Add(JsonConvert.DeserializeObject<ObjectPointSong>(element.ToString()));
                        }

                        break;
                    }
                    case "DoubleItems":
                    {
                        foreach (var element in category.Value)
                        {
                            ListDoubleItems.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }

                        break;
                    }
                    case "CollectedItems":
                    {
                        foreach (var element in category.Value)
                        {
                            ListCollectedItems.Add(JsonConvert.DeserializeObject<ObjectPointCollectedItem>(element.ToString()));
                        }

                        break;
                    }
                    case "Medallions":
                    {
                        foreach (var element in category.Value)
                        {
                            ListMedallions.Add(JsonConvert.DeserializeObject<ObjectPointMedallion>(element.ToString()));
                        }

                        break;
                    }
                    case "GuaranteedHints":
                    {
                        foreach (var element in category.Value)
                        {
                            ListGuaranteedHints.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }

                        break;
                    }
                    case "GossipStones":
                    {
                        foreach (var element in category.Value)
                        {
                            ListGossipStones.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }

                        break;
                    }
                    case "GossipStoneGrids":
                    {
                        foreach (var element in category.Value)
                        {
                            ListGossipStoneGrids.Add(JsonConvert.DeserializeObject<ObjectPointGrid>(element.ToString()));
                        }

                        break;
                    }
                    case "SometimesHints":
                    {
                        foreach (var element in category.Value)
                        {
                            ListSometimesHints.Add(JsonConvert.DeserializeObject<AutoFillTextBox>(element.ToString()));
                        }

                        break;
                    }
                    case "PanelWoth":
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelWotH.Add(JsonConvert.DeserializeObject<ObjectPanelWotH>(element.ToString()));
                        }

                        break;
                    }
                    case "PanelBarren":
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelBarren.Add(JsonConvert.DeserializeObject<ObjectPanelBarren>(element.ToString()));
                        }

                        break;
                    }
                    case "PanelQuantity":
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelQuantity.Add(JsonConvert.DeserializeObject<ObjectPanelQuantity>(element.ToString()));
                        }

                        break;
                    }
                    case "PanelMixed":
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelMixed.Add(JsonConvert.DeserializeObject<ObjectPanelMixed>(element.ToString()));
                        }

                        break;
                    }
                    case "PanelSpoiler":
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelSpoiler.Add(JsonConvert.DeserializeObject<ObjectPanelSpoiler>(element.ToString()));
                        }

                        break;
                    }
                    case "PanelNowPlaying":
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelNowPlaying.Add(JsonConvert.DeserializeObject<ObjectPanelNowPlaying>(element.ToString()));
                        }

                        break;
                    }
                }
            }

            //Debug.WriteLine(App_Settings.BroadcastFile);
            if (App_Settings.BroadcastFile != String.Empty && App_Settings.BroadcastFile != null && !isOnBroadcast)
            {
                settings.ActiveLayoutBroadcastFile = App_Settings.BroadcastFile;
            } else
            {
                if (!isOnBroadcast) settings.ActiveLayoutBroadcastFile = null;
            }

            if (App_Settings.PlacesFile != String.Empty && App_Settings.PlacesFile != null && !isOnBroadcast)
            {
                settings.ActivePlaces = App_Settings.PlacesFile;
                ((MainWindow)form).LoadPlaces();
            }

            if (settings.EnabledMarks == null)
            {
                settings.EnabledMarks = [Settings.MarkModeOption.Checkmark];
            }

            settings.Write();

            panelLayout.Width = App_Settings.Width;
            panelLayout.Height = App_Settings.Height;
            if (App_Settings.BackgroundColor.HasValue)
                // surely this can be done better
                hostForm.SetLayoutColor(new Avalonia.Media.Color(App_Settings.BackgroundColor.Value.A, App_Settings.BackgroundColor.Value.R, App_Settings.BackgroundColor.Value.G, App_Settings.BackgroundColor.Value.B));
            else
                hostForm.SetLayoutColor(Avalonia.Media.Color.FromRgb(33, 33, 33));

            if (App_Settings.DefaultSongMarkerImages != null)
            {
                settings.DefaultSongMarkerImages = App_Settings.DefaultSongMarkerImages;
            }
            if (App_Settings.DefaultGossipStoneImages != null)
            {
                settings.DefaultGossipStoneImages = App_Settings.DefaultGossipStoneImages;
            }
            if (App_Settings.DefaultPathGoalImages != null)
            {
                settings.DefaultPathGoalImages = App_Settings.DefaultPathGoalImages;
            }
            if (App_Settings.DefaultPathGoalCount.HasValue)
            {
                settings.DefaultPathGoalCount = App_Settings.DefaultPathGoalCount.Value;
            }
            if (App_Settings.DefaultWothGossipStoneCount.HasValue)
            {
                settings.DefaultWothGossipStoneCount = App_Settings.DefaultWothGossipStoneCount.Value;
            }
            if (App_Settings.WothColors != null)
            {
                settings.DefaultWothColors = App_Settings.WothColors;
            }
            if (App_Settings.BarrenColors != null)
            {
                settings.DefaultBarrenColors = App_Settings.BarrenColors;
            }
            if (App_Settings.DefaultWothColorIndex.HasValue)
            {
                settings.DefaultWothColorIndex = App_Settings.DefaultWothColorIndex.Value;
            }
            
            if (App_Settings.DefaultDungeonNames != null)
            {
                if (App_Settings.DefaultDungeonNames.TextCollection != null)
                    settings.DefaultDungeonNames.TextCollection = App_Settings.DefaultDungeonNames.TextCollection;
                if (App_Settings.DefaultDungeonNames.DefaultValue.HasValue)
                    settings.DefaultDungeonNames.DefaultValue = App_Settings.DefaultDungeonNames.DefaultValue;
                if (App_Settings.DefaultDungeonNames.Wraparound.HasValue)
                    settings.DefaultDungeonNames.Wraparound = App_Settings.DefaultDungeonNames.Wraparound;
                if (App_Settings.DefaultDungeonNames.FontName != null)
                    settings.DefaultDungeonNames.FontName = App_Settings.DefaultDungeonNames.FontName;
                if (App_Settings.DefaultDungeonNames.FontSize.HasValue)
                    settings.DefaultDungeonNames.FontSize = App_Settings.DefaultDungeonNames.FontSize;
                if (App_Settings.DefaultDungeonNames.FontStyle.HasValue)
                    settings.DefaultDungeonNames.FontStyle = App_Settings.DefaultDungeonNames.FontStyle;
            }

            if (!isOnBroadcast)
            {
                var theMenu = ((MainWindow)hostForm).MenuBar;
                theMenu.ClearAlternates();
                if (ListAlternates.Count > 0)
                {
                    foreach(var item in ListAlternates)
                    {
                        // just in case anyone gets cheeky and sets it in here
                        item.Enabled = false;
                        //Debug.WriteLine(item.Name);
                        if (item.Group != string.Empty && item.Collection != string.Empty)
                        {
                            // if theres no collection, make that first
                            if (!theMenu.CheckAlternatesForSubmenu(item.Collection))
                            {
                                theMenu.AddCollectionToAlternates(item.Collection);

                            }
                            // if that collection is missing the group, then add that
                            if (!theMenu.CheckAlternatesForSubmenu(item.Group, item.Collection))
                            {

                                // add group into collection
                                theMenu.AddGroupToAlternates(item.Group, item.Collection);

                                // add "Disabled" to menugroup first
                                theMenu.AddToAlternatesGroupInCollection(item.Collection, item.Group, string.Empty);
                            }
                            // add the group item into the colleciton
                            theMenu.AddToAlternatesGroupInCollection(item.Collection, item.Group, item.Name);

                        } else if (item.Group != string.Empty)
                        {
                            // add mutually-exclusive toggles
                            if (!theMenu.CheckAlternatesForSubmenu(item.Group))
                            {
                                theMenu.AddGroupToAlternates(item.Group);
                                // add "Disabled" to menugroup first
                                theMenu.AddToAlternatesGroup(item.Group, string.Empty);
                            }
                            theMenu.AddToAlternatesGroup(item.Group, item.Name);
                        } else if (item.Collection != string.Empty) {
                            // put the toggles in a folder somewhere
                            if (!theMenu.CheckAlternatesForSubmenu(item.Collection))
                            {
                                theMenu.AddCollectionToAlternates(item.Collection);
                            }
                            theMenu.AddToAlternatesCollection(item.Collection, item.Name);
                        } else
                        {
                            // make single toggle
                            theMenu.AddToggleToAlternates(item.Name);
                        }
                    }

                }
                else
                {
                    theMenu.AddEmptyAlternatesOption();
                }
            }
                
            if (ListAlternates.Count > 0)
            {
                foreach (var item in ListAlternates)
                {
                    // yes, this is gonna add several duplicate copies of conditional data, but at this point I dont care
                    // check if it has a Conditional
                    if (item.ConditionalChanges != null)
                    {
                        // if it does, give the Alts its conditioining with a copy of its conditionals with the names revsered and isOrganic set to FALSE
                        foreach (SubAltSettings cond in item.ConditionalChanges)
                        {
                            if (!cond.isOrganic) continue;

                            foreach (string name in cond.Names)
                            {
                                // find Alt with name
                                AlternateSettings targetAlt = ListAlternates.Find(newitem => newitem.Name == name);
                                // make new string[] with all names, but with THIS name taken out and item.name swapped in
                                string[] namecopy = (string[])cond.Names.Clone();
                                for (var i = 0; i < namecopy.Length; i++)
                                {
                                    if (namecopy[i] == name) { namecopy[i] = item.Name; break; }
                                }
                                // give it the copy condtional with isOrganic false 
                                if (targetAlt.ConditionalChanges == null) targetAlt.ConditionalChanges = [];
                                targetAlt.ConditionalChanges.Add(new SubAltSettings() { Names = namecopy, Changes = cond.Changes, isOrganic = false });
                            }
                        }

                    }
                }
            }

            
            
            
            
            
            
            if (ListLabels.Count > 0)
            {
                foreach (var item in ListLabels)
                {
                    panelLayout.Children.Add(new LabelExtended(item));
                }
            }
            
            if (ListTextBoxes.Count > 0)
            {
                foreach (var box in ListTextBoxes)
                {
                        
                    if (isOnBroadcast)
                    {
                        // // converts boxes to labels
                        // ContentAlignment ca;
                        // switch (box.TextAlignment) {
                        //     case HorizontalAlignment.Center:
                        //         ca = ContentAlignment.TopCenter;
                        //         break;
                        //     case HorizontalAlignment.Right:
                        //         ca = ContentAlignment.TopRight;
                        //         break;
                        //     case HorizontalAlignment.Left:
                        //     default:
                        //         ca = ContentAlignment.TopLeft;
                        //         break;
                        // }
                        // panelLayout.Controls.Add(new LabelExtended()
                        // {
                        //     Name = box.Name,
                        //     Visible= box.Visible,
                        //     Text = box.Text,
                        //     Left = box.X,
                        //     Top = box.Y,
                        //     Font = new Font(new FontFamily(box.FontName), box.FontSize, box.FontStyle),
                        //     ForeColor = box.FontColor,
                        //     BackColor = Color.Transparent,
                        //     AutoSize = (ca == ContentAlignment.TopLeft),
                        //     TextAlign = ca,
                        //     Width = box.Width,
                        // });
                    } else
                    {
                        panelLayout.Children.Add(new TextBoxPlus(box, settings, isOnBroadcast));
                    }
                        
                }
            }
            
            if (ListTextBoxGrids.Count > 0)
            {
                foreach (var item in ListTextBoxGrids)
                {
                    int namenum = 0;
                    for (int j = 0; j < item.Rows; j++)
                    {
                        for (int i = 0; i < item.Columns; i++)
                        {
                            ObjectPointTextbox temp = new ObjectPointTextbox()
                            {
                                Text = item.Text,
                                Visible = item.Visible,
                                BackColor = item.BackColor,
                                Name = item.Name + namenum,
                                FontName = item.FontName,
                                FontSize = item.FontSize,
                                FontStyle = item.FontStyle,
                                FontColor = item.FontColor,
                                Width = item.Width,
                                Height = item.Height,
                                X = item.X + i * (item.Width + item.Spacing.Width),
                                Y = item.Y + j * (item.Height + item.Spacing.Height),
                                BorderStyle = item.BorderStyle,
                                isBroadcastable = item.isBroadcastable,
                                TextAlignment = item.TextAlignment
                            };
                            panelLayout.Children.Add(new TextBoxPlus(temp, settings, isOnBroadcast));
                            namenum++;
                        }
                    }
                        
                }
            }
            
            if (ListItems.Count > 0)
            {
                foreach (var item in ListItems)
                {
                    panelLayout.Children.Add(new Item(item, settings, isOnBroadcast));
                }
            }
            
            if (ListItemGrids.Count > 0)
            {
                foreach (var item in ListItemGrids)
                {
                        
                    int namenum = 0;
                    for (int j = 0; j < item.Rows; j++)
                    {
                        for (int i = 0; i < item.Columns; i++)
                        {
                            var gs = new ObjectPoint()
                            {
                                Name = item.Name + namenum,
                                X = item.X + i * (item.Size.Width + item.Spacing.Width),
                                Y = item.Y + j * (item.Size.Height + item.Spacing.Height),
                                Size = item.Size,
                                ImageCollection = item.ImageCollection,
                                TinyImageCollection = item.TinyImageCollection,
                                Visible = item.Visible,
                                SizeMode = item.SizeMode,
                                isBroadcastable = item.isBroadcastable,
                                isDraggable = item.isDraggable,
                                AutoName = item.AutoName,
                                BackColor = item.BackColor,
                                OuterPathID = (item.OuterPathID != null) ? $"{namenum}{item.OuterPathID}" : null,
                                isMarkable = item.isMarkable,
                                LinkedItem = item.LinkedItem
                            };
                            panelLayout.Children.Add(new Item(gs, settings, isOnBroadcast));
                            namenum++;
                        }
                    }
                        
                }
            }
            
            if (ListSongs.Count > 0)
            {
                foreach (var song in ListSongs)
                {
                    var s = new Song(song, settings, isOnBroadcast);
                    panelLayout.Children.Add(s);
                    panelLayout.Children.Add(s.SongMarker);
                    ListUpdatables.Add(s);
                }
            }
            
            if (ListDoubleItems.Count > 0)
            {
                foreach (var doubleItem in ListDoubleItems)
                {
                    panelLayout.Children.Add(new DoubleItem(doubleItem, settings, isOnBroadcast));
                }
            }
            
            if (ListCollectedItems.Count > 0)
            {
                foreach (var item in ListCollectedItems)
                {
                    var ci = new CollectedItem(item, settings, isOnBroadcast);
                    panelLayout.Children.Add(ci);
                    panelLayout.Children.Add(ci.ItemCount);
                }
            }
            
            if (ListMedallions.Count > 0)
            {
                foreach (var medallion in ListMedallions)
                {
                    var element = new Medallion(medallion, settings,  App_Settings.BackgroundColor, isOnBroadcast);
                    panelLayout.Children.Add(element);
                    panelLayout.Children.Add(element.SelectedDungeon);
                    element.SetSelectedDungeonLocation();
                }
            }
            
            if (ListGuaranteedHints.Count > 0)
            {
                foreach (var item in ListGuaranteedHints)
                {
                    panelLayout.Children.Add(new GuaranteedHint(item, settings, isOnBroadcast, false));
                }
            }
            
            if (ListGossipStones.Count > 0)
            {
                foreach (var item in ListGossipStones)
                {
                    var g = new GossipStone(item, settings, isOnBroadcast);
                    panelLayout.Children.Add(g);
                    ListUpdatables.Add(g);
                }
            }
            
            if (ListGossipStoneGrids.Count > 0)
            {
                foreach (var item in ListGossipStoneGrids)
                {
                    int namenum = 0;
                    for (int j = 0; j < item.Rows; j++)
                    {
                        for (int i = 0; i < item.Columns; i++)
                        {
                            var gs = new ObjectPoint()
                            {
                                Name = item.Name + namenum,
                                X = item.X + i * (item.Size.Width + item.Spacing.Width),
                                Y = item.Y + j * (item.Size.Height + item.Spacing.Height),
                                Size = item.Size,
                                ImageCollection = item.ImageCollection,
                                TinyImageCollection = item.TinyImageCollection,
                                Visible = item.Visible,
                                SizeMode = item.SizeMode,
                                isBroadcastable = item.isBroadcastable,
                                isMarkable = item.isMarkable,
                            };
                            var g = new GossipStone(gs, settings, isOnBroadcast);
                            panelLayout.Children.Add(g);
                            ListUpdatables.Add(g);
                            namenum++;
                        }
                    }
                        
                }
            }
            
            // if (ListSometimesHints.Count > 0)
            // {
            //     foreach (var item in ListSometimesHints)
            //     {
            //         panelLayout.Controls.Add(new SometimesHint(listSometimesHintsSuggestions, item));
            //     }
            // }
            //
            // if (ListChronometers.Count > 0)
            // {
            //     foreach (var item in ListChronometers)
            //     {
            //         if (item.Visible)
            //             panelLayout.Controls.Add(new Chronometer(item).ChronoLabel);
            //     }
            // }
            //
            if (ListPanelWotH.Count > 0)
            {
                foreach (var item in ListPanelWotH)
                {
                    var panel = new PanelWothBarren(item, settings, listPlacesWithTag, listKeycodesWithTag);
                    panelLayout.Children.Add(panel);
                    panelLayout.Children.Add(panel.textBoxCustom.SuggestionContainer);
                    ListUpdatables.Add(panel);
                    panel.SetSuggestionContainer();
                }
            }
            
            if (ListPanelBarren.Count > 0)
            {
                foreach (var item in ListPanelBarren)
                {
                    var panel = new PanelWothBarren(item, settings, listPlacesWithTag);
                    panelLayout.Children.Add(panel);
                    panelLayout.Children.Add(panel.textBoxCustom.SuggestionContainer);
                    ListUpdatables.Add(panel);
                    panel.SetSuggestionContainer();
                }
            }
            
            if (ListPanelQuantity.Count > 0)
            {
                foreach (var item in ListPanelQuantity)
                {
                    var panel = new PanelWothBarren(item, settings, listPlacesWithTag);
                    panelLayout.Children.Add(panel);
                    panelLayout.Children.Add(panel.textBoxCustom.SuggestionContainer);
                    ListUpdatables.Add(panel);
                    panel.SetSuggestionContainer();
                }
            }
            
            if (ListPanelMixed.Count > 0)
            {
                foreach (var item in ListPanelMixed)
                {
                    var panel = new PanelWothBarren(item, settings, listPlacesWithTag, listKeycodesWithTag);
                    panelLayout.Children.Add(panel);
                    panelLayout.Children.Add(panel.textBoxCustom.SuggestionContainer);
                    ListUpdatables.Add(panel);
                    panel.SetSuggestionContainer();
                }
            }
            
            if (ListPanelSpoiler.Count > 0)
            {
                foreach (var item in ListPanelSpoiler)
                {
                    var panel = new SpoilerPanel(item, settings, isOnBroadcast);
                    panelLayout.Children.Add(panel);
                    ListUpdatables.Add(panel);
                }
            }
            
            if (ListPanelNowPlaying.Count > 0)
            {
                foreach (var item in ListPanelNowPlaying)
                {
                    var panel = new NowPlayingPanel(item, isOnBroadcast);
                    panelLayout.Children.Add(panel);
                }
            }
            
            // if (ListGoMode.Count > 0)
            // {
            //     foreach (var item in ListGoMode)
            //     {
            //         var element = new GoMode(item);
            //         panelLayout.Controls.Add(element);
            //         element.SetLocation();
            //     }
            // }
        }


        public void ApplyAlternates(string name, string groupname, bool check, string lastUsed, bool initialSetup)
        {
            if (!initialSetup) ControlsToBeUpdated.Clear();
            int mult = (check) ? 1 : -1;
            if (groupname == null)
            {
                if (name.Contains("_:_"))
                {
                    string[] words = Regex.Split(name, @"_\:_");
                    name = words[1];
                    groupname = words[0] + "_:_";
                } else if (name.Contains("_::_"))
                {
                    string[] words = Regex.Split(name, @"_\::_");
                    name = words[1];
                    groupname = words[0] + "_::_";
                }
                else
                {
                    AlternateSettings targetAlt = ListAlternates.Find(item => item.Name == name);
                    if (targetAlt == null) { return; }
                    IterateAlternateChanges(targetAlt.Changes, mult);
                    targetAlt.Enabled = check;
                    if (targetAlt.ConditionalChanges != null) IterateConditionalChanges(targetAlt, mult);
                }
            } 
            if (groupname != null) {
                AlternateSettings targetAlt = null;
                string[] words = null;
                // if this is a subgroup within a subcollection
                if (groupname.Contains("_:_")) {
                    words = Regex.Split(groupname, @"_\:_");
                    targetAlt = ListAlternates.Find(item => (item.Name == lastUsed) && (item.Group == words[1]) && (item.Collection == words[0]));
                } else if (groupname.Contains("_::_"))
                {
                    words = Regex.Split(groupname, @"_\::_");
                    targetAlt = ListAlternates.Find(item => (item.Name == lastUsed) && (item.Group == words[1]) && (item.Collection == words[0]));
                } else
                {
                    targetAlt = ListAlternates.Find(item => (item.Name == lastUsed) && (item.Group == groupname));
                }
                // if theres no previosly checked value
                if (targetAlt != null)
                {
                    // get the previously marked setting, undo those
                    IterateAlternateChanges(targetAlt.Changes, -1);
                    targetAlt.Enabled = false;
                    if (targetAlt.ConditionalChanges != null) IterateConditionalChanges(targetAlt, -1);
                }

                if (words != null)
                {
                    targetAlt = ListAlternates.Find(item => (item.Name == name) && (item.Group == words[1]) && (item.Collection == words[0]));
                } else
                {
                    targetAlt = ListAlternates.Find(item => (item.Name == name) && (item.Group == groupname));
                    if (targetAlt == null) targetAlt = ListAlternates.Find(item => (item.Name == name) && (item.Collection == groupname));
                }
                // if this is not disabled
                if (targetAlt != null)
                {
                    IterateAlternateChanges(targetAlt.Changes, mult);
                    targetAlt.Enabled = check;
                    if (targetAlt.ConditionalChanges != null) IterateConditionalChanges(targetAlt, mult);
                }
            }

            
            if (!initialSetup) ConfirmAllAlternates();

            // push to broadcast
            if (hostForm is MainWindow)
            {
                if (App_Settings.EnableBroadcast && BroadcastManager.Window != null)
                {
                    BroadcastManager.GetBroadcast().CurrentLayout.ApplyAlternates(name, groupname, check, lastUsed, initialSetup);
                }
                // f1.TheAutotracker?.CalibrateTracks();
            }
        }

        public void ConfirmAllAlternates()
        {
            foreach (var y in ControlsToBeUpdated)
            {
                y.ConfirmAlternates();
            }
            ControlsToBeUpdated.Clear();
            
        }

        private void IterateConditionalChanges(AlternateSettings targetAlt, int mult)
        {
            foreach (SubAltSettings cond in targetAlt.ConditionalChanges)
            {
                bool EveryoneElseEnabled = true;
                foreach (string name in cond.Names)
                {
                    // if name's Alt is NOT enabled, set allGood to false and break
                    if (!ListAlternates.Find(item => item.Name == name).Enabled) { EveryoneElseEnabled = false; break; }
                }

                if (EveryoneElseEnabled)
                {
                    // something something at the end
                    IterateAlternateChanges(cond.Changes, mult);
                }
            }
        }

        private void IterateAlternateChanges(Dictionary<string, dynamic[]> Changes, int mult)
        {
            if (Changes == null) return;
            foreach (var x in Changes)
            {
                foreach (var y in x.Value)
                {
                    if (x.Key == "Items")
                    {
                        Item? target = null;
                        ObjectPoint? ogPoint = null;
                        string[]? names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    } else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as Item;
                                        ogPoint = ListItems.First(g => g.Name == target.Name);
                                    }
                                }
                            } catch (InvalidOperationException){
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as Item;
                                        ogPoint = ListItems.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                } else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "ItemGrids")
                    {
                        ObjectPointGrid ogGrid = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                // find original entry in grids
                                if (z.Name == "Name") ogGrid = ListItemGrids.First(g => g.Name == z.Value.ToString());
                            }
                            catch (InvalidOperationException){
                                //ignore
                            }
                            // we ignore cols and rows (for now)
                            int namenum = 0;
                            if (ogGrid != null && z.Name != "Name" && z.Name != "Columns" && z.Name != "Rows")
                            {
                                for (int j = 0; j < ogGrid.Rows; j++)
                                {
                                    for (int i = 0; i < ogGrid.Columns; i++)
                                    {
                                        Item? target = hostForm.LayoutContent.Children.First(o => o.Name ==ogGrid.Name + namenum) as Item;
                                        if (z.Name == "Spacing")
                                        {
                                            var newvalues = z.Value.ToString().Split(',');
                                            ApplyAlternatesChanges(target, ogGrid, "X", i * int.Parse(newvalues[0]), mult);
                                            ApplyAlternatesChanges(target, ogGrid, "Y", j * int.Parse(newvalues[1]), mult);
                                        } else
                                        {
                                            ApplyAlternatesChanges(target, ogGrid, z.Name, z.Value, mult);
                                        }
                                        namenum++;
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                            }
                        }
                    } 
                    else if (x.Key == "TextBoxes")
                    {
                        TextBoxPlus? target = null;
                        ObjectPointTextbox? ogPoint = null;
                        string[]? names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as TextBoxPlus;
                                        ogPoint = ListTextBoxes.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as TextBoxPlus;
                                        ogPoint = ListTextBoxes.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                    }
                    else if (x.Key == "TextBoxGrids")
                    {
                        ObjectPointGrid ogGrid = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                // find original entry in grids
                                if (z.Name == "Name") ogGrid = ListTextBoxGrids.Where(g => g.Name == z.Value.ToString()).First();
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            // we ignore cols and rows (for now)
                            int namenum = 0;
                            if (ogGrid != null && z.Name != "Name" && z.Name != "Columns" && z.Name != "Rows")
                            {
                                for (int j = 0; j < ogGrid.Rows; j++)
                                {
                                    for (int i = 0; i < ogGrid.Columns; i++)
                                    {
                                        TextBoxPlus target = hostForm.LayoutContent.Children.First(o => o.Name == ogGrid.Name + namenum.ToString()) as TextBoxPlus;
                                        if (z.Name == "Spacing")
                                        {
                                            var newvalues = z.Value.ToString().Split(',');
                                            ApplyAlternatesChanges(target, ogGrid, "X", i * int.Parse(newvalues[0]), mult);
                                            ApplyAlternatesChanges(target, ogGrid, "Y", j * int.Parse(newvalues[1]), mult);
                                        }
                                        else
                                        {
                                            ApplyAlternatesChanges(target, ogGrid, z.Name, z.Value, mult);
                                        }
                                        namenum++;
                    
                                    }
                                }
                            }
                        }
                    }
                    else if (x.Key == "Labels")
                    {
                        LabelExtended? target = null;
                        GenericLabel? ogPoint = null;
                        string[]? names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as LabelExtended;
                                        ogPoint = ListLabels.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as LabelExtended;
                                        ogPoint = ListLabels.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                    }
                    else if (x.Key == "DoubleItems")
                    {
                        DoubleItem target = null;
                        ObjectPoint ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as DoubleItem;
                                        ogPoint = ListDoubleItems.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as DoubleItem;
                                        ogPoint = ListDoubleItems.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "CollectedItems")
                    {
                        CollectedItem target = null;
                        ObjectPointCollectedItem ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as CollectedItem;
                                        ogPoint = ListCollectedItems.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as CollectedItem;
                                        ogPoint = ListCollectedItems.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "GuaranteedHints")
                    {
                        GuaranteedHint target = null;
                        ObjectPoint ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as GuaranteedHint;
                                        ogPoint = ListGuaranteedHints.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as GuaranteedHint;
                                        ogPoint = ListGuaranteedHints.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "GossipStones")
                    {
                        GossipStone target = null;
                        ObjectPoint ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as GossipStone;
                                        ogPoint = ListGossipStones.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as GossipStone;
                                        ogPoint = ListGossipStones.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "GossipStoneGrids")
                    {
                        ObjectPointGrid ogGrid = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                // find original entry in grids
                                if (z.Name == "Name") ogGrid = ListGossipStoneGrids.Where(g => g.Name == z.Value.ToString()).First();
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            // we ignore cols and rows (for now)
                            int namenum = 0;
                            if (ogGrid != null && z.Name != "Name" && z.Name != "Columns" && z.Name != "Rows")
                            {
                                for (int j = 0; j < ogGrid.Rows; j++)
                                {
                                    for (int i = 0; i < ogGrid.Columns; i++)
                                    {
                                        GossipStone target = hostForm.LayoutContent.Children.First(o => o.Name == ogGrid.Name + namenum.ToString()) as GossipStone;
                                        if (z.Name == "Spacing")
                                        {
                                            var newvalues = z.Value.ToString().Split(',');
                                            ApplyAlternatesChanges(target, ogGrid, "X", i * int.Parse(newvalues[0]), mult);
                                            ApplyAlternatesChanges(target, ogGrid, "Y", j * int.Parse(newvalues[1]), mult);
                                        }
                                        else
                                        {
                                            ApplyAlternatesChanges(target, ogGrid, z.Name, z.Value, mult);
                                        }
                                        namenum++;
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                            }
                        }
                    }
                    else if (x.Key == "Medallions")
                    {
                        Medallion target = null;
                        ObjectPointMedallion ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as Medallion;
                                        ogPoint = ListMedallions.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as Medallion;
                                        ogPoint = ListMedallions.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "Songs")
                    {
                        Song target = null;
                        ObjectPointSong ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as Song;
                                        ogPoint = ListSongs.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as Song;
                                        ogPoint = ListSongs.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "PanelSpoiler")
                    {
                        SpoilerPanel target = null;
                        ObjectPanelSpoiler ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as SpoilerPanel;
                                        ogPoint = ListPanelSpoiler.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as SpoilerPanel;
                                        ogPoint = ListPanelSpoiler.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "PanelWoth")
                    {
                        PanelWothBarren target = null;
                        ObjectPanelWotH ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as PanelWothBarren;
                                        ogPoint = ListPanelWotH.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as PanelWothBarren;
                                        ogPoint = ListPanelWotH.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "PanelQuantity")
                    {
                        PanelWothBarren target = null;
                        ObjectPanelQuantity ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as PanelWothBarren;
                                        ogPoint = ListPanelQuantity.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as PanelWothBarren;
                                        ogPoint = ListPanelQuantity.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "PanelBarren")
                    {
                        PanelWothBarren target = null;
                        ObjectPanelBarren ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as PanelWothBarren;
                                        ogPoint = ListPanelBarren.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as PanelWothBarren;
                                        ogPoint = ListPanelBarren.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "PanelMixed")
                    {
                        PanelWothBarren target = null;
                        ObjectPanelMixed ogPoint = null;
                        string[] names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as PanelWothBarren;
                                        ogPoint = ListPanelMixed.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as PanelWothBarren;
                                        ogPoint = ListPanelMixed.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                        if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                        if (target != null && names == null)
                        {
                            if (!ControlsToBeUpdated.Contains(target)) ControlsToBeUpdated.Add(target);
                        }
                    }
                    else if (x.Key == "PanelNowPlaying")
                    {
                        NowPlayingPanel? target = null;
                        ObjectPanelNowPlaying? ogPoint = null;
                        string[]? names = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    if (z.Value.Count() > 1)
                                    {
                                        names = ((JArray)z.Value).ToObject<string[]>();
                                    }
                                    else
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == z.Value.ToString()) as NowPlayingPanel;
                                        ogPoint = ListPanelNowPlaying.Where(g => g.Name == target.Name).First();
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                //ignore
                            }
                            if ((target != null || names != null) && z.Name != "Name")
                            {
                                if (names != null)
                                {
                                    foreach (string zname in names)
                                    {
                                        target = hostForm.LayoutContent.Children.First(o => o.Name == zname) as NowPlayingPanel;
                                        ogPoint = ListPanelNowPlaying.Where(g => g.Name == target.Name).First();
                                        ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                                    }
                                }
                                else ApplyAlternatesChanges(target, ogPoint, z.Name, z.Value, mult);
                            }
                        }
                    }
                    else if (x.Key == "AppSize")
                    {
                        // HACKY AF
                        foreach (JProperty z in y)
                        {
                            if (z.Name == "Width")
                            {
                                hostForm.LayoutContent.Width += int.Parse(z.Value.ToString()) * mult;
                                hostForm.Width += int.Parse(z.Value.ToString()) * mult;

                            } else if (z.Name == "Height")
                            {
                                hostForm.LayoutContent.Height += int.Parse(z.Value.ToString()) * mult;
                                hostForm.Height += int.Parse(z.Value.ToString()) * mult;
                            }
                        }
                        //hostForm.Refresh();
                    }
                }
            }

        }

        private void ApplyAlternatesChanges(Control target, object ogPoint, string name, object value, int mult)
        {
            string translatedname = TranslationLayer(name, target.GetType());
            
            // unique? override
            if (translatedname == "Location")
            {
                if (name == "X")
                {
                    // target.GetType().GetProperty(translatedname).SetValue(target, new Point(po.X + int.Parse(value.ToString()) * mult, po.Y));
                    Canvas.SetLeft(target, Canvas.GetLeft(target) + int.Parse(value.ToString()) * mult);
                    if (target is CollectedItem ci) ci.SetCounterPosition();
                } else if (name == "Y")
                {
                    // target.GetType().GetProperty(translatedname).SetValue(target, new Point(po.X, po.Y + int.Parse(value.ToString()) * mult));
                    Canvas.SetTop(target, Canvas.GetTop(target) + int.Parse(value.ToString()) * mult);
                    if (target is CollectedItem ci) ci.SetCounterPosition();
                }

                return;
            }

            var targetType = target.GetType().GetProperty(translatedname);
            if (targetType == null)
            {
                ((IAlternatableObject)target).SpecialtyImport(ogPoint, name, value, mult);
            } else
            {
                switch (targetType.GetValue(target, null))
                {
                    case Size si:
                    
                        // format: `1,2`
                        var newvalues = value.ToString().Split(',');
                        target.GetType().GetProperty(translatedname).SetValue(target, new Size(si.Width + int.Parse(newvalues[0]) * mult, si.Height + int.Parse(newvalues[1]) * mult));
                        break;
                    case Point po:
                        if (name == "X")
                        {
                            target.GetType().GetProperty(translatedname).SetValue(target, new Point(po.X + int.Parse(value.ToString()) * mult, po.Y));
                        } else if (name == "Y")
                        {
                            target.GetType().GetProperty(translatedname).SetValue(target, new Point(po.X, po.Y + int.Parse(value.ToString()) * mult));
                        }
                        break;
                    case int i:
                        target.GetType().GetProperty(translatedname).SetValue(target, i + int.Parse(value.ToString())*mult);
                        break;
                    case double d:
                        target.GetType().GetProperty(translatedname).SetValue(target, d + int.Parse(value.ToString())*mult);
                        break;
                    case bool _:
                        if (mult < 0)
                        {
                            object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                            if (name == "Visible")
                            {
                                //visibiliyu is handled differently, due to the possibility of sub-objects
                                ((IAlternatableObject)target).SetVisible((bool)ogValue);
                            } else
                            {
                                target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                            }
                        }
                        else
                        {
                            if (name == "Visible")
                            {
                                ((IAlternatableObject)target).SetVisible(bool.Parse(value.ToString()));
                            } else
                            {
                                target.GetType().GetProperty(translatedname).SetValue(target, bool.Parse(value.ToString()));
                            }
                        }
                        break;
                    case string _:
                        if (mult < 0)
                        {
                            object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                            target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                        } else
                        {
                            if (value.ToString() == "null") target.GetType().GetProperty(translatedname).SetValue(target, string.Empty);
                            else target.GetType().GetProperty(translatedname).SetValue(target, value.ToString());
                        }
                        break;
                    case string[] _:
                        if (mult < 0)
                        {
                            object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                            target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                        } else
                        {
                            target.GetType().GetProperty(translatedname).SetValue(target, ((JArray)value).ToObject<string[]>());
                        }
                        break;
                    case PictureBoxSizeMode _:
                        if (mult < 0)
                        {
                            object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                            target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                        } else
                        {
                            target.GetType().GetProperty(translatedname).SetValue(target, (PictureBoxSizeMode)(int.Parse(value.ToString())));
                        }
                        break;
                    case Color _:
                        if (mult < 0)
                        {
                            var ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                            Color tempcolor;
                            // format: `1,2,3`
                            var newrgb = ogValue?.ToString().Split(',');
                            // if there isnt more than 1 response, assume its a word and not rgb
                            if (newrgb.Length > 1) tempcolor =  Color.FromArgb(int.Parse(newrgb[1].Substring(3)), int.Parse(newrgb[2].Substring(3)), int.Parse(newrgb[3].Substring(3).Split(']')[0]));
                            else tempcolor = Color.FromName(ogValue.ToString());
                            if (tempcolor.Name == "Transparent")
                            {
                                target.GetType().GetProperty(translatedname).SetValue(target, null);
                            } else
                            {
                                target.GetType().GetProperty(translatedname).SetValue(target, tempcolor);
                            }
                        }
                        else
                        {
                            // format: `1,2,3`
                            var newrgb = value?.ToString().Split(',');
                            // if there isnt more than 1 response, assume its a word and not rgb
                            if (newrgb.Length > 1) target.GetType().GetProperty(translatedname).SetValue(target, Color.FromArgb(int.Parse(newrgb[0]), int.Parse(newrgb[1]), int.Parse(newrgb[2])));
                            else target.GetType().GetProperty(translatedname).SetValue(target, Color.FromName(value.ToString()));
                        }
                        break;
                    default:
                        throw new NotImplementedException($"Data type {targetType.GetValue(target, null).GetType()} (used for {name}) has not yet been implemented. Go pester JXJacob to go fix it.");
            
                }
            }
        }

        private string TranslationLayer(string input, Type targettype)
        {
            // only accounts for Properties, not for attributes that are only used for calculations or subobjects
            // this used to be stupid back in Winforms, but being able to translate Winforms to Avalonia was a incredible bit of foresight/luck
            switch (input)
            {
                case "BackColor":
                    // if (targettype == typeof(SpoilerPanel)) { return "storedBackColor"; }
                    return input;
                case "CounterSize":
                    return "GossipStoneSize";
                case "CountPosition":
                    return "CollectedItemCountPosition";
                case "Color":
                    return "ForeColor";
                case "DataRowPadding":
                    return "topRowPadding";
                case "GossipStoneImageCollection":
                    return "ListImage_WothItemsOption";
                case "ImageCollection":
                    return "ImageNames";
                case "isMinimal":
                    return "MinimalMode";
                case "IsScrollable":
                    return "isScrollable";
                case "PathGoalImageCollection":
                    return "ListImage_GoalsOption";
                case "Size":
                    return targettype == typeof(Song) ? "forciblyfail" : input;
                case "SubTextBoxSize":
                    return "subBoxSize";
                case "Width":
                    return input;
                    // return targettype == typeof(PanelWothBarren) || targettype == typeof(NowPlayingPanel) ? "forciblyfail" : input;
                case "X":
                case "Y":
                    return "Location";
                case "Visible":
                    return "IsVisible";
                default:
                    return input;
            }
        }

    }

    public interface IAlternatableObject
    {
        void SetVisible(bool visible);
        void SpecialtyImport(object ogPoint, string name, object value, int mult);
        void ConfirmAlternates();
    }

    public class GenericLabel
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public FontStyle FontStyle { get; set; }
        public string Color { get; set; }
        // public Size MaxSize { get; set; }
        public bool Visible { get; set; }
        public int Width { get; set; }
        public ContentAlignment TextAlignment { get; set; } = ContentAlignment.TopLeft;
    }

    public class ObjectPointTextbox
    {
        public string Name { get; set; }
        public string Text { get; set; } = "";
        public ContentAlignment TextAlignment {get; set;} = ContentAlignment.TopLeft;
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public FontStyle FontStyle { get; set; }
        public Color FontColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BorderStyle BorderStyle { get; set; } = BorderStyle.FixedSingle;
        public bool isBroadcastable { get; set; } = false;
    }

    public class ObjectPoint
    {
        public string Name { get; set; }
        public int DK64_ID { get; set; } = -1;
        public bool isMarkable { get; set; } = true;
        public int X { get; set; }
        public int Y { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; }
        public string[] ImageCollection { get; set; }
        public string[] TinyImageCollection { get; set; }
        public int DefaultIndex { get; set; } = 0;
        public bool isScrollable{ get; set; } = true;
        public bool isBroadcastable { get; set; } = false;
        public bool isDraggable { get; set; } = true;
        public string DoubleBroadcastName { get; set; } = null;
        public string DoubleBroadcastSide { get; set; } = null;
        public int LeftDK64_ID { get; set; } = -1;
        public int RightDK64_ID { get; set; } = -1;
        public string AutoName { get; set; } = null;
        public string OuterPathID { get; set; } = null;
        public string LinkedItem {  get; set; } = null;
        public bool CanCycle { get; set; } = false;
        public string DragImage { get; set; } = null;
        public Color BackColor { get; set; } = Color.Transparent;
        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;
    }

    public class ObjectPointSong
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; }
        public string DragAndDropImageName { get; set; }
        public string[] ImageCollection { get; set; }
        public string[] TinyImageCollection { get; set; }
        public string ActiveSongImage { get; set; }
        public string ActiveTinySongImage { get; set; }
        public bool isBroadcastable { get; set; } = false;
        public string AutoName { get; set; } = null;
        public string LinkedItem { get; set; } = null;
        public bool isMarkable { get; set; } = true;
    }

    public class MedallionLabel
    {
        public string[]? TextCollection { get; set; }
        public int? DefaultValue { get; set; }
        public bool? Wraparound { get; set; }
        public int? FontSize { get; set; }
        public string? FontName { get; set; }
        public FontStyle? FontStyle { get; set; }
    }

    public class ObjectPointMedallion
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; }
        public string[] ImageCollection { get; set; }
        public MedallionLabel? Label { get; set; }
        public bool isBroadcastable { get; set; } = false;
        public string AutoName { get; set; } = null;
        public bool isMarkable { get; set; } = true;
    }

    public class ObjectPointGrid
    {
        public string Name { get; set; }
        public string Text { get; set; } = "";
        public ContentAlignment TextAlignment { get; set; } = ContentAlignment.TopLeft;
        public int X { get; set; }
        public int Y { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public Size Size { get; set; }
        public Size Spacing { get; set; }
        public bool Visible { get; set; }
        public string[] ImageCollection { get; set; }
        public string[] TinyImageCollection { get; set; }

        public Color BackColor { get; set; }
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public FontStyle FontStyle { get; set; }
        public Color FontColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;
        public bool isBroadcastable { get; set; } = false;
        public string AutoName { get; set; } = null;
        public string OuterPathID { get; set; } = null;
        public bool isDraggable { get; set; } = true;
        public string LinkedItem { get; set; } = null;
        public bool CanCycle { get; set; } = false;
        public BorderStyle BorderStyle { get; set; } = BorderStyle.FixedSingle;
        public bool isMarkable { get; set; } = true;
    }

    public class AutoFillTextBox
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int FontSize { get; set; }
        public string FontName { get; set; }
        public FontStyle FontStyle { get; set; }
        public Color FontColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ObjectPanelWotH
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int NbMaxRows { get; set; }
        public bool IsScrollable { get; set; }

        public string TextBoxName { get; set; }
        public Color TextBoxBackColor { get; set; }
        public string TextBoxFontName { get; set; }
        public int TextBoxFontSize { get; set; }
        public FontStyle TextBoxFontStyle { get; set; }
        public int TextBoxHeight { get; set; }
        public string TextBoxText { get; set; }

        public Color LabelColor { get; set; }
        public Color LabelBackColor { get; set; }
        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public FontStyle LabelFontStyle { get; set; }
        public int LabelHeight { get; set; }

        public Size GossipStoneSize { get; set; }
        public int? GossipStoneCount { get; set; }
        public string[] GossipStoneImageCollection { get; set; }
        public int GossipStoneSpacing { get; set; }
        public Color GossipStoneBackColor { get; set; }

        public int? PathGoalCount { get; set; }
        public string[] PathGoalImageCollection { get; set; }
        public int PathGoalSpacing { get; set; }
        public bool PathCycling { get; set; } = false;
        public string OuterPathID { get; set; }

        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;
        public bool isBroadcastable { get; set; } = false;
        public bool isWotH { get; set; } = true;
        public bool isMarkable { get; set; } = true;
    }

    public class ObjectPanelBarren
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int NbMaxRows { get; set; }
        public bool IsScrollable { get; set; }

        public string TextBoxName { get; set; }
        public Color TextBoxBackColor { get; set; }
        public string TextBoxFontName { get; set; }
        public int TextBoxFontSize { get; set; }
        public FontStyle TextBoxFontStyle { get; set; }
        public int TextBoxHeight { get; set; }
        public string TextBoxText { get; set; }

        public Color LabelColor { get; set; }
        public Color LabelBackColor { get; set; }
        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public FontStyle LabelFontStyle { get; set; }
        public int LabelHeight { get; set; }

        public bool isBroadcastable { get; set; } = false;
    }

    public class ObjectPanelQuantity
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int NbMaxRows { get; set; }
        public bool IsScrollable { get; set; }

        public string TextBoxName { get; set; }
        public Color TextBoxBackColor { get; set; }
        public string TextBoxFontName { get; set; }
        public int TextBoxFontSize { get; set; }
        public FontStyle TextBoxFontStyle { get; set; }
        public int TextBoxHeight { get; set; }
        public string TextBoxText { get; set; }

        public Color LabelColor { get; set; }
        public Color LabelBackColor { get; set; }
        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public FontStyle LabelFontStyle { get; set; }
        public int LabelHeight { get; set; }

        public int CounterFontSize { get; set; }
        public int CounterSpacing { get; set; }
        public Size CounterSize { get; set; }

        public Size SubTextBoxSize { get; set; }

        public bool isBroadcastable { get; set; } = false;
    }

    public class ObjectPanelMixed
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int NbMaxRows { get; set; }
        public bool IsScrollable { get; set; }

        public string DefaultTextBoxName { get; set; }
        public Color DefaultTextBoxBackColor { get; set; }
        public string DefaultTextBoxFontName { get; set; }
        public int DefaultTextBoxFontSize { get; set; }
        public FontStyle DefaultTextBoxFontStyle { get; set; }
        public int DefaultTextBoxHeight { get; set; }
        public string DefaultTextBoxText { get; set; }

        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public FontStyle LabelFontStyle { get; set; }
        public int LabelHeight { get; set; }

        public MixedSubPanels[] SubPanels { get; set; }
    }

    public class MixedSubPanels
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Keycode { get; set; }
        public int Order { get; set; }

        // common
        public Color LabelColor { get; set; }
        public Color LabelBackColor { get; set; }

        // woth-only
        public Size GossipStoneSize { get; set; }
        public int? GossipStoneCount { get; set; }
        public string[] GossipStoneImageCollection { get; set; }
        public int GossipStoneSpacing { get; set; }
        public Color GossipStoneBackColor { get; set; }

        public int? PathGoalCount { get; set; } = 0;
        public string[] PathGoalImageCollection { get; set; }
        public int PathGoalSpacing { get; set; }
        public bool PathCycling { get; set; } = false;
        public string OuterPathID { get; set; }

        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;
        public bool isBroadcastable { get; set; } = false;
        public bool isWotH { get; set; } = true;
        public bool isMarkable { get; set; } = true;

        // quantity-only
        public int CounterFontSize { get; set; }
        public int CounterSpacing { get; set; }
        public Size CounterSize { get; set; }

        public Size SubTextBoxSize { get; set; }
    }

    public class ObjectPanelSpoiler
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public Color DefaultColor { get; set; }
        public Color CellBackColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int RowPadding { get; set; }
        public int ColPadding { get; set; }
        public bool WriteByRow { get; set; } = true;
        public bool ExtendFinalCell { get; set; } = false;
        public bool isMinimal { get; set; }
        public int DataRowHeight { get; set; }
        public int DataRowPadding { get; set; } = 0;
        public int WorldNumWidth { get; set; }
        public int WorldNumHeight { get; set; }
        public int WorldLabelWidth { get; set; }
        public int PotionWidth { get; set; }
        public int PotionHeight { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public FontStyle FontStyle { get; set; }
        public int LabelSpacing { get; set; }
        public int LabelWidth { get; set; }
        public bool isBroadcastable { get; set; } = false;
        public bool isMarkable { get; set; } = true;
    }

    public class ObjectPanelNowPlaying
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool Visible { get; set; }
        public Color BackColor { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool isBroadcastable { get; set; } = false;

        public int GamePositionX { get; set; }
        public int GamePositionY { get; set; }
        public int GameFontSize { get; set; }
        public string GameFontName { get; set; }
        public FontStyle GameFontStyle { get; set; }
        public Color GameFontColor { get; set; }

        public int TitlePositionX { get; set; }
        public int TitlePositionY { get; set; }
        public int TitleFontSize { get; set; }
        public string TitleFontName { get; set; }
        public FontStyle TitleFontStyle { get; set; }
        public Color TitleFontColor { get; set; }
    }

    public class ObjectPointGoMode
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Size Size { get; set; }
        public bool Visible { get; set; }
        public string[] ImageCollection { get; set; }
        public string BackgroundImage { get; set; }
    }

    public class ObjectPointCollectedItem
    {
        public string Name { get; set; }
        public int X { get; set; }
        public double Y { get; set; }
        public Size Size { get; set; }
        public Size CountPosition { get; set; }
        public int CountMin { get; set; }
        public int? CountMax { get; set; }
        public int DefaultValue { get; set; }
        public int Step { get; set; }
        public bool Visible { get; set; }
        public string[] ImageCollection { get; set; }
        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public string AutoName { get; set; } = null;
        public string AutoSubName { get; set; } = null;
        public FontStyle LabelFontStyle { get; set; }
        public Color LabelColor { get; set; }
        public bool isBroadcastable { get; set; } = false;
        public bool hasSlash { get; set; } = false;
        public Color BackColor { get; set; } = Color.Black;
        public Color BackGroundColor { get; set; } = Color.Transparent;
        public bool isMarkable { get; set; } = true;
    }

    public class AppSettings
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Color? BackgroundColor { get;set; }
        public string[] DefaultSongMarkerImages { get; set; } = null;
        public string[] DefaultGossipStoneImages { get; set; } = null;
        public string[] DefaultPathGoalImages { get; set; } = null; 
        public int? DefaultWothGossipStoneCount { get; set; } = null;
        public int? DefaultPathGoalCount { get; set; } = null;
        public string[] WothColors { get; set; }
        public string[] BarrenColors { get; set; }
        public int? DefaultWothColorIndex { get; set; }
        public MedallionLabel DefaultDungeonNames { get; set; } = null;
        public bool EnableBroadcast { get; set; } = false;
        public string? BroadcastFile { get; set; } = null;
        public string AutotrackingGame { get; set; } = null;
        public string PlacesFile { get; set; } = null;
    }

    public class SubAltSettings
    {
        public string[] Names { get; set; } = null;
        public Dictionary<string, dynamic[]> Changes { get; set; } = null;
        public bool isOrganic { get; set; } = true;
    }

    public class AlternateSettings
    {
        public string Name { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Collection { get; set; } = string.Empty;
        public Dictionary<string, dynamic[]> Changes { get; set; } = null;
        public List<SubAltSettings> ConditionalChanges { get; set; } = null;
        public bool Enabled { get; set; } = false;
    }
}
