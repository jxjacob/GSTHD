using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GSTHD
{
    public interface UpdatableFromSettings
    {
        void UpdateFromSettings();
    }

    public class Layout
    {
        public List<GenericLabel> ListLabels { get; }  = new List<GenericLabel>();
        public List<ObjectPointTextbox> ListTextBoxes { get; } = new List<ObjectPointTextbox>();
        public List<ObjectPointGrid> ListTextBoxGrids { get; } = new List<ObjectPointGrid>();
        public List<ObjectPoint> ListItems { get; } = new List<ObjectPoint>();
        public List<ObjectPointGrid> ListItemGrids { get; } = new List<ObjectPointGrid>();
        public List<ObjectPointSong> ListSongs { get; } = new List<ObjectPointSong>();
        public List<ObjectPoint> ListDoubleItems { get; } = new List<ObjectPoint>();
        public List<ObjectPointCollectedItem> ListCollectedItems { get; } = new List<ObjectPointCollectedItem>();
        public List<ObjectPointMedallion> ListMedallions { get; } = new List<ObjectPointMedallion>();
        public List<ObjectPoint> ListGuaranteedHints { get; } = new List<ObjectPoint>();
        public List<ObjectPoint> ListGossipStones { get; } = new List<ObjectPoint>();
        public List<ObjectPointGrid> ListGossipStoneGrids { get; } = new List<ObjectPointGrid>();
        public List<AutoFillTextBox> ListSometimesHints { get; } = new List<AutoFillTextBox>();
        public List<AutoFillTextBox> ListChronometers { get; } = new List<AutoFillTextBox>();
        public List<ObjectPanelWotH> ListPanelWotH { get; } = new List<ObjectPanelWotH>();
        public List<ObjectPanelBarren> ListPanelBarren { get; } = new List<ObjectPanelBarren>();
        public List<ObjectPanelQuantity> ListPanelQuantity { get; } = new List<ObjectPanelQuantity>();
        public List<ObjectPanelSpoiler> ListPanelSpoiler { get; } = new List<ObjectPanelSpoiler>();
        public List<ObjectPointGoMode> ListGoMode { get; } = new List<ObjectPointGoMode>();

        public List<AlternateSettings> ListAlternates { get; } = new List<AlternateSettings>();

        public List<UpdatableFromSettings> ListUpdatables = new List<UpdatableFromSettings>();

        public AppSettings App_Settings = new AppSettings();

        private GSTForms hostForm;

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
            bool isOnBroadcast = (hostForm.Name == "GSTHD_DK64 Broadcast View");


            ListUpdatables.Clear();
            if (settings.ActiveLayout != string.Empty)
            {
                JObject json_layout;
                try
                {
                    if (!isOnBroadcast)
                    {
                        json_layout = JObject.Parse(File.ReadAllText(@"" + settings.ActiveLayout));
                        if (!json_layout.ContainsKey("AppSize"))
                        {
                            MessageBox.Show("Layout file " + settings.ActiveLayout.ToString() + " does not appear to contain any GSTHD layout data.\nReverting to dk64.json.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            // set settings to dk64.json
                            settings.ActiveLayout = "layouts\\dk64.json";
                            settings.Write();
                            // force reload
                            form.Reset(null);
                            return;
                        }
                    } else
                    {
                        json_layout = JObject.Parse(File.ReadAllText($"{Path.GetFileName(Path.GetDirectoryName(settings.ActiveLayout))}\\{settings.ActiveLayoutBroadcastFile}"));
                    }
                } catch (JsonReaderException)
                {
                    MessageBox.Show("File " + settings.ActiveLayout.ToString() + " does not appear to be a proper json file.\nReverting to dk64.json.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // set settings to dk64.json
                    settings.ActiveLayout = "layouts\\dk64.json";
                    settings.Write();
                    // force reload
                    form.Reset(null);
                    return;
                } catch (FileNotFoundException)
                {
                    MessageBox.Show("File " + settings.ActiveLayout.ToString() + " could not be found.\nReverting to dk64.json.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // set settings to dk64.json
                    settings.ActiveLayout = "layouts\\dk64.json";
                    settings.Write();
                    // force reload
                    form.Reset(null);
                    return;
                }
                
                foreach (var category in json_layout)
                {
                    if (category.Key.ToString() == "AppSize")
                    {
                        App_Settings = JsonConvert.DeserializeObject<AppSettings>(category.Value.ToString());
                    }

                    if (category.Key.ToString() == "Alternates")
                    {
                        foreach (var element in category.Value)
                        {
                            ListAlternates.Add(JsonConvert.DeserializeObject<AlternateSettings>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "Labels")
                    {
                        foreach (var element in category.Value)
                        {
                            ListLabels.Add(JsonConvert.DeserializeObject<GenericLabel>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "TextBoxes")
                    {
                        foreach (var element in category.Value)
                        {
                            ListTextBoxes.Add(JsonConvert.DeserializeObject<ObjectPointTextbox>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "TextBoxGrids")
                    {
                        foreach (var element in category.Value)
                        {
                            ListTextBoxGrids.Add(JsonConvert.DeserializeObject<ObjectPointGrid>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "Items")
                    {
                        foreach (var element in category.Value)
                        {
                            ListItems.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "ItemGrids")
                    {
                        foreach (var element in category.Value)
                        {
                            ListItemGrids.Add(JsonConvert.DeserializeObject<ObjectPointGrid>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "Songs")
                    {
                        foreach (var element in category.Value)
                        {
                            ListSongs.Add(JsonConvert.DeserializeObject<ObjectPointSong>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "DoubleItems")
                    {
                        foreach (var element in category.Value)
                        {
                            ListDoubleItems.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "CollectedItems")
                    {
                        foreach (var element in category.Value)
                        {
                            ListCollectedItems.Add(JsonConvert.DeserializeObject<ObjectPointCollectedItem>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "Medallions")
                    {
                        foreach (var element in category.Value)
                        {
                            ListMedallions.Add(JsonConvert.DeserializeObject<ObjectPointMedallion>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "GuaranteedHints")
                    {
                        foreach (var element in category.Value)
                        {
                            ListGuaranteedHints.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "GossipStones")
                    {
                        foreach (var element in category.Value)
                        {
                            ListGossipStones.Add(JsonConvert.DeserializeObject<ObjectPoint>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "GossipStoneGrids")
                    {
                        foreach (var element in category.Value)
                        {
                            ListGossipStoneGrids.Add(JsonConvert.DeserializeObject<ObjectPointGrid>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "SometimesHints")
                    {
                        foreach (var element in category.Value)
                        {
                            ListSometimesHints.Add(JsonConvert.DeserializeObject<AutoFillTextBox>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "Chronometers")
                    {
                        foreach (var element in category.Value)
                        {
                            ListChronometers.Add(JsonConvert.DeserializeObject<AutoFillTextBox>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "PanelWoth")
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelWotH.Add(JsonConvert.DeserializeObject<ObjectPanelWotH>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "PanelBarren")
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelBarren.Add(JsonConvert.DeserializeObject<ObjectPanelBarren>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "PanelQuantity")
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelQuantity.Add(JsonConvert.DeserializeObject<ObjectPanelQuantity>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "PanelSpoiler")
                    {
                        foreach (var element in category.Value)
                        {
                            ListPanelSpoiler.Add(JsonConvert.DeserializeObject<ObjectPanelSpoiler>(element.ToString()));
                        }
                    }

                    if (category.Key.ToString() == "GoMode")
                    {
                        foreach (var element in category.Value)
                        {
                            ListGoMode.Add(JsonConvert.DeserializeObject<ObjectPointGoMode>(element.ToString()));
                        }
                    }
                }

                //Debug.WriteLine(App_Settings.BroadcastFile);
                if (App_Settings.BroadcastFile != String.Empty && !isOnBroadcast)
                {
                    settings.ActiveLayoutBroadcastFile = App_Settings.BroadcastFile;
                } else
                {
                    if (!isOnBroadcast) settings.ActiveLayoutBroadcastFile = null;
                }
                settings.Write();

                panelLayout.Size = new Size(App_Settings.Width, App_Settings.Height);
                if (App_Settings.BackgroundColor.HasValue)
                    hostForm.BackColor = App_Settings.BackgroundColor.Value;
                panelLayout.BackColor = hostForm.BackColor;

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


                //TODO: somewhere in this declaration, i will need to do Settings lookups to see if they are checked or not
                    //and then later, actually apply the stuff

                if (!isOnBroadcast)
                {
                    var theMenu = ((Form1)hostForm).MenuBar;
                    theMenu.ClearAlternates();
                    if (ListAlternates.Count > 0)
                    {
                        foreach(var item in ListAlternates)
                        {
                            //Debug.WriteLine(item.Name);
                            if (item.Group != string.Empty)
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


                if (ListLabels.Count > 0)
                {
                    foreach (var item in ListLabels)
                    {
                        panelLayout.Controls.Add(new Label()
                        {
                            Name = item.Name,
                            Visible = item.Visible,
                            Text = item.Text,
                            Left = item.X,
                            Top = item.Y,
                            Font = new Font(new FontFamily(item.FontName), item.FontSize, item.FontStyle),
                            ForeColor = Color.FromName(item.Color),
                            BackColor = Color.Transparent,
                            AutoSize = (item.TextAlignment == ContentAlignment.TopLeft),
                            TextAlign = item.TextAlignment,
                            Width = item.Width,
                        });
                    }
                }

                if (ListTextBoxes.Count > 0)
                {
                    foreach (var box in ListTextBoxes)
                    {
                        
                        if (isOnBroadcast)
                        {
                            // converts boxes to labels
                            ContentAlignment ca;
                            switch (box.TextAlignment) {
                                case HorizontalAlignment.Center:
                                    ca = ContentAlignment.TopCenter;
                                    break;
                                case HorizontalAlignment.Right:
                                    ca = ContentAlignment.TopRight;
                                    break;
                                case HorizontalAlignment.Left:
                                default:
                                    ca = ContentAlignment.TopLeft;
                                    break;
                            }
                            panelLayout.Controls.Add(new Label()
                            {
                                Name = box.Name,
                                Visible= box.Visible,
                                Text = box.Text,
                                Left = box.X,
                                Top = box.Y,
                                Font = new Font(new FontFamily(box.FontName), box.FontSize, box.FontStyle),
                                ForeColor = box.FontColor,
                                BackColor = Color.Transparent,
                                AutoSize = (ca == ContentAlignment.TopLeft),
                                TextAlign = ca,
                                Width = box.Width,
                            });
                        } else
                        {
                            panelLayout.Controls.Add(new TextBoxPlus(box, settings, isOnBroadcast));
                        }
                        
                    }
                }

                if (ListTextBoxGrids.Count > 0)
                {
                    foreach (var item in ListTextBoxGrids)
                    {
                        
                        for (int j = 0; j < item.Rows; j++)
                        {
                            for (int i = 0; i < item.Columns; i++)
                            {
                                ObjectPointTextbox temp = new ObjectPointTextbox()
                                {
                                    Text = item.Text,
                                    Visible = item.Visible,
                                    BackColor = item.BackColor,
                                    Name = item.Name + j + i,
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
                                panelLayout.Controls.Add(new TextBoxPlus(temp, settings, isOnBroadcast));
                            }
                        }
                        
                    }
                }

                if (ListItems.Count > 0)
                {
                    foreach (var item in ListItems)
                    {
                        panelLayout.Controls.Add(new Item(item, settings, isOnBroadcast) { Visible = item.Visible} );
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
                                };
                                panelLayout.Controls.Add(new Item(gs, settings, isOnBroadcast) { Visible = item.Visible } );
                                namenum++;
                            }
                        }
                        
                    }
                }

                if (ListSongs.Count > 0)
                {
                    foreach (var song in ListSongs)
                    {
                        var s = new Song(song, settings, isOnBroadcast) { Visible = song.Visible };
                        panelLayout.Controls.Add(s);
                        ListUpdatables.Add(s);
                    }
                }

                if (ListDoubleItems.Count > 0)
                {
                    foreach (var doubleItem in ListDoubleItems)
                    {
                        panelLayout.Controls.Add(new DoubleItem(doubleItem, settings, isOnBroadcast) { Visible = doubleItem.Visible });
                    }
                }

                if (ListCollectedItems.Count > 0)
                {
                    foreach (var item in ListCollectedItems)
                    {
                        panelLayout.Controls.Add(new CollectedItem(item, settings, isOnBroadcast) { Visible = item.Visible });
                    }
                }

                if (ListMedallions.Count > 0)
                {
                    foreach (var medallion in ListMedallions)
                    {
                        var element = new Medallion(medallion, settings, isOnBroadcast) { Visible = medallion.Visible };
                        panelLayout.Controls.Add(element);
                        panelLayout.Controls.Add(element.SelectedDungeon);
                        ListUpdatables.Add(element);
                        element.SetSelectedDungeonLocation();
                        element.SelectedDungeon.BringToFront();
                    }
                }

                if (ListGuaranteedHints.Count > 0)
                {
                    foreach (var item in ListGuaranteedHints)
                    {
                        panelLayout.Controls.Add(new GuaranteedHint(item, settings, isOnBroadcast) { Visible = item.Visible });
                    }
                }

                if (ListGossipStones.Count > 0)
                {
                    foreach (var item in ListGossipStones)
                    {
                        var g = new GossipStone(item, settings, isOnBroadcast) { Visible = item.Visible };
                        panelLayout.Controls.Add(g);
                        ListUpdatables.Add(g);
                    }
                }

                if (ListGossipStoneGrids.Count > 0)
                {
                    foreach (var item in ListGossipStoneGrids)
                    {
                        for (int j = 0; j < item.Rows; j++)
                        {
                            for (int i = 0; i < item.Columns; i++)
                            {
                                var gs = new ObjectPoint()
                                {
                                    Name = item.Name + j + i,
                                    X = item.X + i * (item.Size.Width + item.Spacing.Width),
                                    Y = item.Y + j * (item.Size.Height + item.Spacing.Height),
                                    Size = item.Size,
                                    ImageCollection = item.ImageCollection,
                                    TinyImageCollection = item.TinyImageCollection,
                                    Visible = item.Visible,
                                    SizeMode = item.SizeMode,
                                    isBroadcastable = item.isBroadcastable
                                };
                                var g = new GossipStone(gs, settings, isOnBroadcast) { Visible = item.Visible };
                                panelLayout.Controls.Add(g);
                                ListUpdatables.Add(g);
                            }
                        }
                        
                    }
                }

                if (ListSometimesHints.Count > 0)
                {
                    foreach (var item in ListSometimesHints)
                    {
                        panelLayout.Controls.Add(new SometimesHint(listSometimesHintsSuggestions, item) { Visible = item.Visible });
                    }
                }

                if (ListChronometers.Count > 0)
                {
                    foreach (var item in ListChronometers)
                    {
                        if (item.Visible)
                            panelLayout.Controls.Add(new Chronometer(item).ChronoLabel);
                    }
                }

                if (ListPanelWotH.Count > 0)
                {
                    foreach (var item in ListPanelWotH)
                    {
                        var panel = new PanelWothBarren(item, settings) { Visible = item.Visible };
                        panel.PanelWoth(listPlacesWithTag, listKeycodesWithTag, item);
                        panelLayout.Controls.Add(panel);
                        panelLayout.Controls.Add(panel.textBoxCustom.SuggestionContainer);
                        ListUpdatables.Add(panel);
                        panel.SetSuggestionContainer();
                    }
                }

                if (ListPanelBarren.Count > 0)
                {
                    foreach (var item in ListPanelBarren)
                    {
                        var panel = new PanelWothBarren(item, settings) { Visible = item.Visible };
                        panel.PanelBarren(listPlacesWithTag, item);
                        panelLayout.Controls.Add(panel);
                        panelLayout.Controls.Add(panel.textBoxCustom.SuggestionContainer);
                        ListUpdatables.Add(panel);
                        panel.SetSuggestionContainer();
                    }
                }

                if (ListPanelQuantity.Count > 0)
                {
                    foreach (var item in ListPanelQuantity)
                    {
                        var panel = new PanelWothBarren(item, settings) { Visible = item.Visible };
                        panel.PanelQuantity(listPlacesWithTag, item);
                        panelLayout.Controls.Add(panel);
                        panelLayout.Controls.Add(panel.textBoxCustom.SuggestionContainer);
                        ListUpdatables.Add(panel);
                        panel.SetSuggestionContainer();
                    }
                }

                if (ListPanelSpoiler.Count > 0)
                {
                    foreach (var item in ListPanelSpoiler)
                    {
                        var panel = new SpoilerPanel(item, settings, isOnBroadcast) { Visible = item.Visible };
                        panelLayout.Controls.Add(panel);
                        ListUpdatables.Add(panel);
                    }
                }

                if (ListGoMode.Count > 0)
                {
                    foreach (var item in ListGoMode)
                    {
                        var element = new GoMode(item) { Visible = item.Visible };
                        panelLayout.Controls.Add(element);
                        element.SetLocation();
                    }
                }
            }
        }


        public void ApplyAlternates(string name, string groupname, bool check, string lastUsed)
        {
            int mult = (check) ? 1 : -1;
            if (groupname == null)
            {
                AlternateSettings targetAlt = ListAlternates.Find(item => item.Name == name);
                IterateAlternateChanges(targetAlt, mult, (mult < 0));
            } else
            {
                AlternateSettings targetAlt = ListAlternates.Find(item => (item.Name == lastUsed) && (item.Group == groupname));
                // if theres no previosly checked value
                if (targetAlt != null)
                {
                    // get the previously marked setting, undo those
                    IterateAlternateChanges(targetAlt, -1, true);

                }

                targetAlt = ListAlternates.Find(item => (item.Name == name) && (item.Group == groupname));
                // if this is not disabled
                if (targetAlt != null)
                {
                    IterateAlternateChanges(targetAlt, mult);
                }
            }
            // push to broadcast
            if (hostForm is Form1 f1)
            {
                if (App_Settings.EnableBroadcast && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).CurrentLayout.ApplyAlternates(name, groupname, check, lastUsed);
                }
            }
        }

        private void IterateAlternateChanges(AlternateSettings targetAlt, int mult, bool undo=false)
        {
            foreach (var x in targetAlt.Changes)
            {
                foreach (var y in x.Value)
                {
                    if (x.Key == "Items")
                    {
                        Item target = null;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name") target = hostForm.Controls.Find(z.Value.ToString(), true)[0] as Item;
                            } catch (IndexOutOfRangeException){
                                //ignore
                            }
                            if (target != null && z.Name != "Name")
                            {
                                if (undo)
                                {
                                    ApplyAlternatesChanges(target, z.Name, z.Value, mult, true);
                                } else
                                {
                                    ApplyAlternatesChanges(target, z.Name, z.Value, mult, false);
                                }
                            }
                        }
                    } else if (x.Key == "ItemGrids")
                    {
                        ObjectPointGrid ogGrid = null;
                        int namenum = 0;
                        foreach (JProperty z in y)
                        {
                            try
                            {
                                if (z.Name == "Name")
                                {
                                    // find original entry in grids
                                    ogGrid = ListItemGrids.Where(g => g.Name == z.Value.ToString()).First();
                                }
                            }
                            catch (IndexOutOfRangeException)
                            {
                                //ignore
                            }
                            // we ignore cols and rows (for now)
                            if (ogGrid != null && z.Name != "Name" && z.Name != "Columns" && z.Name != "Rows")
                            {
                                for (int j = 0; j < ogGrid.Rows; j++)
                                {
                                    for (int i = 0; i < ogGrid.Columns; i++)
                                    {
                                        Item target = hostForm.Controls.Find(ogGrid.Name + namenum.ToString(), true)[0] as Item;
                                        ApplyAlternatesChanges(target, z.Name, z.Value, mult, undo, namenum.ToString());
                                        namenum++;

                                    }
                                }
                            }
                        }
                    }
                    // keys is collecteditems, grids, etc







                }
            }

            //keeping this around for future reference on how to dial in
            //foreach (var x in item.Changes)
            //{
            //    Debug.WriteLine("    " + x.Key);
            //    foreach (var y in x.Value)
            //    {
            //        foreach (JProperty z in y)
            //        {
            //            Debug.WriteLine("        " + z.Name + ":" + z.Value.ToString());
            //        }
            //        Debug.WriteLine("        ");
            //    }
            //}

        }

        private void ApplyAlternatesChanges(Control target, string name, object value, int mult, bool undoing, string namenum="")
        {
            string translatedname = TranslationLayer(name);
            var targetType = target.GetType().GetProperty(translatedname).GetValue(target, null);
            switch (targetType)
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
                    } else
                    {
                        target.GetType().GetProperty(translatedname).SetValue(target, new Point(po.X, po.Y + int.Parse(value.ToString()) * mult));
                    }
                    break;
                case int i:
                    target.GetType().GetProperty(translatedname).SetValue(target, i + int.Parse(value.ToString())*mult);
                    break;
                case bool _:
                    if (undoing || mult < 0)
                    {
                        object ogPoint;
                        if (namenum == "")
                        {
                            ogPoint = ListItems.Where(g => g.Name == target.Name).First();
                        }
                        else
                        {
                            ogPoint = ListItemGrids.Where(g => g.Name == target.Name.Substring(0, target.Name.Length - namenum.Length)).First();
                        }
                        object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                        target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                    }
                    else
                    {
                        target.GetType().GetProperty(translatedname).SetValue(target, bool.Parse(value.ToString()));
                    }
                    break;
                case string _:
                    if (undoing || mult < 0)
                    {
                        object ogPoint;
                        if (namenum == "")
                        {
                            ogPoint = ListItems.Where(g => g.Name == target.Name).First();
                        }
                        else
                        {
                            ogPoint = ListItemGrids.Where(g => g.Name == target.Name.Substring(0, target.Name.Length - namenum.Length)).First();
                        }
                        object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                        target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                    } else
                    {
                        if (value.ToString() == "null") target.GetType().GetProperty(translatedname).SetValue(target, null);
                        else target.GetType().GetProperty(translatedname).SetValue(target, value.ToString());
                    }
                    break;
                case string[] _:
                    if (undoing || mult < 0)
                    {
                        object ogPoint;
                        if (namenum == "")
                        {
                            ogPoint = ListItems.Where(g => g.Name == target.Name).First();
                        } else {
                            ogPoint = ListItemGrids.Where(g => g.Name == target.Name.Substring(0, target.Name.Length - namenum.Length)).First();
                        }
                        object ogValue = ogPoint.GetType().GetProperty(name).GetValue(ogPoint, null);
                        target.GetType().GetProperty(translatedname).SetValue(target, ogValue);
                    } else
                    {
                        target.GetType().GetProperty(translatedname).SetValue(target, ((JArray)value).ToObject<string[]>());
                    }
                    break;
                default:
                    throw new NotImplementedException(targetType.GetType().ToString());
            }
            if (target is OrganicImage oi)
            {
                if (oi is Item i) i.UpdateImage();
                oi.Invalidate();
            }
        }

        private string TranslationLayer(string input)
        {
            // why did i not make the names 1 to 1 aaaaaaaaaaaaaa
            switch (input)
            {
                case "ImageCollection":
                    return "ImageNames";
                case "X":
                case "Y":
                    return "Location";
                default:
                    return input;
            }
        }

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
        public HorizontalAlignment TextAlignment {get; set;} = HorizontalAlignment.Left;
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
    }

    public class MedallionLabel
    {
        public string[] TextCollection { get; set; }
        public int? DefaultValue { get; set; }
        public bool? Wraparound { get; set; }
        public int? FontSize { get; set; }
        public string FontName { get; set; }
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
        public MedallionLabel Label { get; set; }
        public bool isBroadcastable { get; set; } = false;
        public string AutoName { get; set; } = null;
    }

    public class ObjectPointGrid
    {
        public string Name { get; set; }
        public string Text { get; set; } = "";
        public HorizontalAlignment TextAlignment { get; set; } = HorizontalAlignment.Left;
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
        public bool CanCycle { get; set; } = false;
        public BorderStyle BorderStyle { get; set; } = BorderStyle.FixedSingle;
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

        public int? PathGoalCount { get; set; }
        public string[] PathGoalImageCollection { get; set; }
        public int PathGoalSpacing { get; set; }
        public bool PathCycling { get; set; } = false;
        public string OuterPathID { get; set; }

        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;
        public bool isBroadcastable { get; set; } = false;
        public bool isWotH { get; set; } = true;
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
        public int TextBoxWidth { get; set; }
        public int TextBoxHeight { get; set; }
        public string TextBoxText { get; set; }

        public Color LabelColor { get; set; }
        public Color LabelBackColor { get; set; }
        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public FontStyle LabelFontStyle { get; set; }
        public int LabelWidth { get; set; }
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
        public int TextBoxWidth { get; set; }
        public int TextBoxHeight { get; set; }
        public string TextBoxText { get; set; }

        public Color LabelColor { get; set; }
        public Color LabelBackColor { get; set; }
        public string LabelFontName { get; set; }
        public int LabelFontSize { get; set; }
        public FontStyle LabelFontStyle { get; set; }
        public int LabelWidth { get; set; }
        public int LabelHeight { get; set; }

        public int CounterFontSize { get; set; }
        public int CounterSpacing { get; set; }
        public Size CounterSize { get; set; }
        public string CounterImage { get; set; }

        public Size SubTextBoxSize { get; set; }
        public int SubTextBoxFontSize { get; set; }
        public Color SubTextBoxBackColor { get; set; }
        public Color SubTextBoxFontColor { get; set; }

        public bool isBroadcastable { get; set; } = false;
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
        public int Y { get; set; }
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
        public string BroadcastFile { get; set; } = null;
        public string AutotrackingGame { get; set; } = null;
    }

    public class AlternateSettings
    {
        public string Name { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Collection { get; set; } = string.Empty;
        public Dictionary<string, dynamic[]> Changes { get; set; } = null;
    }
}
