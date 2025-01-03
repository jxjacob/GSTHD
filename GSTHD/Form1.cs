using GSTHD.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using AutoUpdaterDotNET;

namespace GSTHD
{
    public interface GSTForms
    {
        void Reset(object sender);

        Color BackColor { get; set; }
        String Name { get; set; }
        Size Size { get; set; }
        Panel LayoutContent { get; set; }

        Control.ControlCollection Controls { get; }

    }

    public partial class Form1 : Form, GSTForms
    {
        Dictionary<string, string> ListPlacesWithTag = new Dictionary<string, string>();
        Dictionary<string, string> ListKeycodesWithTag = new Dictionary<string, string>();
        SortedSet<string> ListSometimesHintsSuggestions = new SortedSet<string>();

        public Form1_MenuBar MenuBar;
        public Layout CurrentLayout { get; set; }
        public Panel LayoutContent { get; set; }
        public Autotracker TheAutotracker;
        public System.Timers.Timer StoneCyclingTimer;
        private int cyclecount = 0;

        public List<GossipStone> currentlyCycling = new List<GossipStone>();

        public Settings Settings;

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Control && e.KeyCode == Keys.R)
            //{
            //    this.Controls.Clear();
            //    e.Handled = true;
            //    e.SuppressKeyPress = true;
            //    this.Form1_Load(sender, new EventArgs());
            //}

            // scrapped feature. does not play well with new OrganicImages
            //if (e.Modifiers == Keys.Control)
            //{
            //    if (e.Control && e.KeyCode == Keys.Oemplus)
            //    {
            //        ZoomIn();
            //    } else if (e.Control && e.KeyCode == Keys.OemMinus)
            //    {
            //        ZoomOut();
            //    } else if (e.Control && e.KeyCode == Keys.D0)
            //    {
            //        ZoomReset();
            //    }
            //}
        }

        private void LoadAll(object sender, EventArgs e)
        {
            var assembly = Assembly.GetEntryAssembly().GetName();
            this.Text = $"{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title} v{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build} {((!Environment.Is64BitProcess) ? "(32-bit)" : string.Empty)}";
            this.AcceptButton = null;
            this.MaximizeBox = false;

            AutoUpdater.UpdateFormSize = new Size(800, 500);
            AutoUpdater.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToBitmap();

            AutoUpdater.Start("https://raw.githubusercontent.com/jxjacob/GSTHD/master/GSTHD/AutoUpdateInfo.xml");

            LoadSettings();
            
            MenuBar = new Form1_MenuBar(this, Settings);

            LoadLayout();
            SetMenuBar();
            //setAutoTracker();
            if (Settings.DeleteOldAutosaves) CleanUpOldAutos();


            this.KeyPreview = true;
            //this.KeyDown += changeCollectedSkulls;
        }

        private void Reload(bool changeLayout = false)
        {
            //trying to figure out why this was here. is it cuz in the older version you couldnt just open a new layout and had to edit the json to get it to swap?
            // otherwise, why would you need to reload all the settings
            //LoadSettings();
            if (!changeLayout)
            {
                if (this.CurrentLayout.App_Settings.EnableBroadcast && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).Reload();
                }
            } else
            {
                if (Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).ClearAndDispose();
                }
            }
            LoadLayout();
            SetMenuBar();
        }

        public void LoadSettings()
        {
            Settings = Settings.Read();

            ListKeycodesWithTag.Clear();
            ListPlacesWithTag.Clear();
            currentlyCycling.Clear();
            JObject json_places = JObject.Parse(File.ReadAllText(@"" + Settings.ActivePlaces));
            if (json_places.ContainsKey("places") && json_places.ContainsKey("keycodes"))
            {
                //new format
                foreach (var property in JObject.Parse(json_places.SelectToken("places").ToString()))
                {
                    ListPlacesWithTag.Add(property.Key, property.Value.ToString());
                }

                foreach (var property in JObject.Parse(json_places.SelectToken("keycodes").ToString()))
                {
                    ListKeycodesWithTag.Add(property.Key, property.Value.ToString());
                }

            } else
            {
                // legacy format
                foreach (var property in json_places)
                {
                    ListPlacesWithTag.Add(property.Key, property.Value.ToString());
                }

            }

            ListSometimesHintsSuggestions.Clear();
            JObject json_hints = JObject.Parse(File.ReadAllText(@"" + Settings.ActiveSometimesHints));
            foreach (var categorie in json_hints)
            {
                foreach (var hint in categorie.Value)
                {
                    ListSometimesHintsSuggestions.Add(hint.ToString());
                }
            }

            
        }

        private void SetMenuBar()
        {
            MenuBar.SetRenderer();
        }

        private void CleanUpOldAutos()
        {
            // list everything in dir
            // if more than 25, get the oldest ones then nukeem
            //Debug.WriteLine("------begin");
            if (Directory.Exists("Autosaves"))
            {
                var files = Directory.GetFiles("Autosaves").OrderBy(f => new FileInfo(f).CreationTime);
                int todelete = files.Count() - 25;
                if (todelete > 0)
                {
                    foreach (string file in files)
                    {
                        //Debug.WriteLine(file + " gets deleted");
                        FileInfo fi = new FileInfo(file);
                        fi.Delete();
                        todelete--;
                        if (todelete == 0) break;
                    }
                    //files.OrderBy(x => ((FileInfo)x).CreationTime;
                    //foreach (string file in files)
                    //{
                    //    FileInfo fi = new FileInfo(file);
                    //    if (fi.LastAccessTime < DateTime.Now.AddMonths(-3))
                    //        fi.Delete();
                    //}
                }
            }
            
        }

        private void LoadLayout()
        {
            Controls.Clear();
            if (StoneCyclingTimer != null)
            {
                StoneCyclingTimer.Elapsed -= IncrementStones;
                StoneCyclingTimer.Stop();
                StoneCyclingTimer.Close();
                StoneCyclingTimer = null;
            }
            //StoneCyclingTimer = new System.Threading.Timer(IncrementStones, null, TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
            StoneCyclingTimer = new System.Timers.Timer(Settings.GossipCycleTime*1000);
            StoneCyclingTimer.Elapsed += IncrementStones;
            StoneCyclingTimer.AutoReset = true;
            StoneCyclingTimer.Enabled = true;
            LayoutContent?.Dispose();
            LayoutContent = new Panel();
            CurrentLayout = new Layout();
            CurrentLayout.LoadLayout(LayoutContent, Settings, ListSometimesHintsSuggestions, ListPlacesWithTag, ListKeycodesWithTag, this);
            Size = new Size(LayoutContent.Size.Width, LayoutContent.Size.Height + MenuBar.Size.Height);
            LayoutContent.Dock = DockStyle.Top;
            Controls.Add(LayoutContent);
            MenuBar.Dock = DockStyle.Top;
            Controls.Add(MenuBar);
            ApplyAltsFromSettings();
        }

        private void ApplyAltsFromSettings(bool broadcast = false)
        {
            if (Settings.AlternateSettings.Count > 0)
            {
                //Settings lookups to see if they are checked or not
                var altlists = Settings.AlternateSettings.Where(x => x.LayoutName == Settings.ActiveLayout);
                if (!altlists.Any()) return;
                AltSettings thealt = altlists.First();
                if (broadcast)
                {
                    Form2 f2 = (Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"];
                    foreach (var alt in thealt.Changes)
                    {
                        if (alt.Value == "True")
                        {
                            f2.CurrentLayout.ApplyAlternates(alt.Key, null, true, string.Empty, true);
                        }
                        else if (alt.Value != "False" || alt.Value != "Disabled")
                        {
                            f2.CurrentLayout.ApplyAlternates(alt.Value, alt.Key, true, string.Empty, true);
                        }
                    }
                    f2.CurrentLayout.ConfirmAllAlternates();
                } else
                {
                    foreach (var alt in thealt.Changes)
                    {
                        if (alt.Value == "True")
                        {
                            if (alt.Key.Contains("_::_"))
                            {
                                var words = Regex.Split(alt.Key, @"_\:\:_").ToList();
                                MenuBar.CheckmarkAlternateOption(words[0], words[1]);
                                CurrentLayout.ApplyAlternates(words[1], words[0], true, string.Empty, true);
                            } else
                            {
                                MenuBar.CheckmarkAlternateOption(string.Empty, alt.Key);
                                CurrentLayout.ApplyAlternates(alt.Key, null, true, string.Empty, true);
                            }
                        }
                        else if (alt.Value != "False" || alt.Value != "Disabled")
                        {
                            MenuBar.CheckmarkAlternateOption(alt.Key, alt.Value);
                            CurrentLayout.ApplyAlternates(alt.Value, alt.Key, true, string.Empty, true);
                        }
                    }
                    CurrentLayout.ConfirmAllAlternates();
                }
            }
        }

        public void UpdateLayoutFromSettings()
        {
            CurrentLayout.UpdateFromSettings();
            // i CANNOT figure out why the broadcast view wont accept the new settings across the items, but the main view will
            //if (this.CurrentLayout.App_Settings.EnableBroadcast && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            //{
            //    Debug.WriteLine("pushing new settings to broadcast");
            //    ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).ForceLoadSettings(Settings);
            //    ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).UpdateLayoutFromSettings();
            //}
            StoneCyclingTimer.Close();
            StoneCyclingTimer = new System.Timers.Timer(Settings.GossipCycleTime * 1000);
            StoneCyclingTimer.Elapsed += IncrementStones;
            StoneCyclingTimer.AutoReset = true;
            StoneCyclingTimer.Enabled = true;
        }

        private void changeCollectedSkulls(object sender, KeyEventArgs k)
        {
            if (k.KeyCode == Keys.F9) { }
            //button_chrono_Click(sender, new EventArgs());
            if (k.KeyCode == Keys.F11) { }
            //label_collectedSkulls_MouseDown(pbox_collectedSkulls.Controls[0], new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            if (k.KeyCode == Keys.F12) { }
            //label_collectedSkulls_MouseDown(pbox_collectedSkulls.Controls[0], new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0));
        }

        public void Reset(object sender)
        {
            ControlExtensions.ClearAndDispose(LayoutContent);
            currentlyCycling.Clear();
            StopAutotracker();
            if (StoneCyclingTimer != null)
            {
                StoneCyclingTimer.Elapsed -= IncrementStones;
                StoneCyclingTimer.Stop();
                StoneCyclingTimer.Close();
                StoneCyclingTimer = null;
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            try
            {
                if (sender == null) { Reload(); }
                else
                {
                    if (((ToolStripItem)sender).Text == "Open Layout")
                    {
                        Reload(true);
                    }
                    else
                    {
                        Reload();
                    }
                }
                    
            } catch (Exception)
            {
                Reload();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Process.GetCurrentProcess().Refresh();
        }

        public void SetAutotracker(Process emulator, uint offset)
        {
            StopAutotracker();
            TheAutotracker = new Autotracker(emulator, offset, this);
        }

        public void SetAutotracker(Process emulator, ulong offset)
        {
            StopAutotracker();
            TheAutotracker = new Autotracker(emulator, offset, this);
        }

        public void StopAutotracker()
        {
            if (TheAutotracker != null)
            {
                CurrentLayout.ListUpdatables.Remove(TheAutotracker);
                TheAutotracker.NukeTimer();
            }
            TheAutotracker = null;
        }

        public void AddCycling(GossipStone gs) 
        { 
            currentlyCycling.Add(gs);
        }

        public void RemoveCycling(GossipStone gs) 
        {
            currentlyCycling.Remove(gs);
        }

        private void IncrementStones(object state, ElapsedEventArgs e)
        {
            // for stone in ubdatables
            // stone.incrementcycle
            try
            {
                foreach (var obj in currentlyCycling)
                {
                    if (obj is GossipStone gs)
                    {
                        if (gs.HeldImages.Count > 1) gs.IncrementCycle();
                    }
                }
            } catch (InvalidOperationException) {
            }
            
            cyclecount++;
            //Debug.WriteLine(currentlyCycling.Count);
            // collect every 5 minutesish
            if (cyclecount > 300 / Settings.GossipCycleTime)
            {
                GC.Collect();
                cyclecount = 0;
                // jank autosave
                if (Settings.EnableAutosave) SaveState(true);
            }
        }

        public void SaveState(bool force = false)
        {
            JObject thejson = new JObject();

            foreach (Item x in this.Controls[0].Controls.OfType<Item>())
            {
                if (x.Name != "")
                {
                    ItemState state = x.GetState();
                    if (state.ImageIndex != x.DefaultIndex || state.isMarked != 0)
                    {
                        thejson.Add(x.Name, $"{state.ImageIndex},{(int)state.isMarked}");
                    }
                }
            }
            foreach (CollectedItem x in this.Controls[0].Controls.OfType<CollectedItem>())
            {
                if (x.Name != "")
                {
                    CollectedItemState state = x.GetState();
                    if (state.CollectedItems != x.DefaultValue || state.isMarked != 0)
                    {
                        thejson.Add(x.Name, $"{state.CollectedItems},{(int)state.isMarked}");
                    }
                }
            }
            foreach (DoubleItem x in this.Controls[0].Controls.OfType<DoubleItem>())
            {
                if (x.Name != "")
                {
                    DoubleItemState state = x.GetState();
                    if (state.ImageIndex != 0 || state.isMarked != 0)
                    {
                        thejson.Add(x.Name, $"{state.ImageIndex},{(int)state.isMarked}");
                    }
                }
            }
            foreach (TextBox x in this.Controls[0].Controls.OfType<TextBox>())
            {
                if (x.Name != "" && x.Text != "") 
                {
                    thejson.Add(x.Name, x.Text);
                }
            }
            foreach (GossipStone x in this.Controls[0].Controls.OfType<GossipStone>())
            {
                if (x.Name != "")
                {
                    GossipStoneState state = x.GetState();
                    string conv = state.ToString();
                    if (conv != "False,,0,0")
                    {
                        thejson.Add(x.Name, conv);
                    }
                }
            }
            foreach (Medallion x in this.Controls[0].Controls.OfType<Medallion>())
            {
                if (x.Name != "")
                {
                    MedallionState state = x.GetState();
                    string conv = state.ToString();
                    if (conv != "0,0,0")
                    {
                        thejson.Add(x.Name, conv);
                    }
                }
            }
            foreach (Song x in this.Controls[0].Controls.OfType<Song>())
            {
                if (x.Name != "")
                {
                    SongState state = x.GetWholeState();
                    string conv = state.ToString();
                    if (conv != "0,0,False,,0,0")
                    {
                        thejson.Add(x.Name, conv);
                    }
                }
            }
            foreach (PanelWothBarren x in this.Controls[0].Controls.OfType<PanelWothBarren>())
            {
                if (x.Name != "")
                {
                    
                    if (x.ListHints.Count > 0)
                    {
                        if (x.isWotH == 0)
                        {
                            string thestring = "";
                            foreach (WotHState y in x.GetWotHs())
                            {
                                if (thestring.Length > 0)
                                {
                                    thestring += "\n";
                                }
                                thestring += y.ToString();
                            }
                            thejson.Add(x.Name, thestring);
                        } 
                        else if (x.isWotH == HintPanelType.Barren)
                        {
                            string thestring = "";
                            foreach (BarrenState y in x.GetBarrens())
                            {
                                if (thestring.Length > 0)
                                {
                                    thestring += "\n";
                                }
                                thestring += y.ToString();
                            }
                            thejson.Add(x.Name, thestring);
                        } 
                        else if (x.isWotH == HintPanelType.Quantity)
                        {
                            string thestring = "";
                            foreach (QuantityState y in x.GetQuantities())
                            {
                                if (thestring.Length > 0)
                                {
                                    thestring += "\n";
                                }
                                thestring += y.ToString();
                            }
                            thejson.Add(x.Name, thestring);
                        } 
                        else if (x.isWotH == HintPanelType.Mixed)
                        {
                            // TODO: this and the loadstate but for multipanels
                        }
                    }
                }
            }
            foreach (SpoilerPanel x in this.Controls[0].Controls.OfType<SpoilerPanel>())
            {
                if (x.Name != "")
                {
                    if (x.spoilerLoaded)
                    {
                        string cellstring = "";
                        foreach (SpoilerCell cell in x.cells)
                        {
                            SpoilerCellState cs = cell.GetState();
                            if (cellstring.Length > 0)
                            {
                                cellstring += "\f";
                            }
                            cellstring += cell.Name + "::|::" + cs.ToString();
                        }
                        string ATstring = "";
                        foreach (int item in x.foundATItems)
                        {
                            if (ATstring.Length > 0)
                            {
                                ATstring += ",";
                            }
                            ATstring += item.ToString();
                        }
                        string thestring = $"{x.whereSpoiler}\v{ATstring}\v{x.howManySlams}\v{cellstring}";
                        thejson.Add(x.Name, thestring);
                    }
                }
            }
            foreach (GuaranteedHint x in this.Controls[0].Controls.OfType<GuaranteedHint>())
            {
                if (x.Name != "" && x.isMarked != 0)
                {
                    thejson.Add(x.Name, (int)x.isMarked);
                }
            }


            if (force)
            {
                if (thejson.HasValues)
                {
                    if (!Directory.Exists("Autosaves")) Directory.CreateDirectory("Autosaves");
                    File.WriteAllText(@"Autosaves/auto" + DateTime.Now.ToString("MM-dd-yyyy") + ".json", thejson.ToString());

                }
            }
            else
            {
                //open file to write to
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "Save state to JSON file"
                };
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "")
                {
                    File.WriteAllText(saveFileDialog1.FileName, thejson.ToString());
                }
                saveFileDialog1.Dispose();
            }
        }

        public void LoadState()
        {
            OpenFileDialog filedia = new OpenFileDialog
            {
                Title = "Load state from JSON file",
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*",
                Multiselect = false
            };
            // put that filename into settings' ActivePlaces
            if (filedia.ShowDialog() == DialogResult.OK)
            {
                // reset for safekeepings
                Reset(null);
                //all of the fucking things
                JObject loadedjson = JObject.Parse(File.ReadAllText(filedia.FileName));
                int missingItems = 0;
                foreach (JProperty x in (JToken)loadedjson)
                {
                    Control found = null;
                    try
                    {
                        found = this.Controls.Find(x.Name, true)[0];
                    } catch (IndexOutOfRangeException)
                    {
                        missingItems++;
                        Debug.WriteLine(x.Name + " not found in layout. Skipping");
                    }
                    if (found is Item i)
                    {
                        string[] words = ((string)x.Value).Split(',');
                        i.SetState(new ItemState { ImageIndex =  int.Parse(words[0]), isMarked = (MarkedImageIndex)int.Parse(words[1]) });
                    } else if (found is DoubleItem di)
                    {
                        string[] words = ((string)x.Value).Split(',');
                        di.SetState(new DoubleItemState { ImageIndex = int.Parse(words[0]), isMarked = (MarkedImageIndex)int.Parse(words[1]) });
                    }
                    else if (found is CollectedItem ci)
                    {
                        string[] words = ((string)x.Value).Split(',');
                        ci.SetState(new CollectedItemState { CollectedItems = int.Parse(words[0]), isMarked = (MarkedImageIndex)int.Parse(words[1]) });
                    } 
                    else if (found is TextBox tb)
                    {
                        tb.Text = (string)x.Value;
                    } 
                    else if (found is GossipStone gs)
                    {
                        string[] words = ((string)x.Value).Split(',');
                        string[] hi = words[1].Split('|');

                        GossipStoneState newstate = new GossipStoneState()
                        {
                            HoldsImage = Boolean.Parse(words[0]),
                            HeldImages = hi.ToList(),
                            ImageIndex = int.Parse(words[2]),
                            isMarked = (MarkedImageIndex)int.Parse(words[3]),
                        };
                        gs.SetState(newstate);
                    }
                    else if (found is Medallion md)
                    {
                        string[] words = ((string)x.Value).Split(',');
                        MedallionState newstate = new MedallionState()
                        {
                            DungeonIndex = int.Parse(words[0]),
                            ImageIndex = int.Parse(words[1]),
                            isMarked = (MarkedImageIndex)int.Parse(words[2])
                        };
                        md.SetState(newstate);
                    }
                    else if (found is Song s)
                    {
                        string[] words = ((string)x.Value).Split(',');
                        SongState newstate = new SongState()
                        {
                            ImageIndex = int.Parse(words[0]),
                            isMarked = (MarkedImageIndex)int.Parse(words[1]),
                            MarkerState = new SongMarkerState()
                            {
                                HoldsImage = Boolean.Parse(words[2]),
                                HeldImageName = words[3],
                                ImageIndex = int.Parse(words[4]),
                                isMarked = (MarkedImageIndex)int.Parse(words[5])
                            },
                        };
                        s.SetWholeState(newstate);
                    }
                    else if (found is PanelWothBarren pa) 
                    {
                        if (pa.isWotH == 0){
                            pa.SetWotH((string)x.Value);
                        } else if (pa.isWotH == HintPanelType.Barren){
                            pa.SetBarren((string)x.Value);
                        } else if (pa.isWotH == HintPanelType.Quantity){
                            pa.SetQuantities((string)x.Value);
                        }
                    }
                    else if (found is SpoilerPanel sp)
                    {
                        string[] words = ((string)x.Value).Split('\v');
                        sp.ImportFromJson(words[0]);
                        if (words[1] != "") sp.foundATItems = words[1].Split(',').Select(int.Parse).ToList();
                        sp.howManySlams = int.Parse(words[2]);
                        sp.SetCells(words[3]);
                    } else if (found is GuaranteedHint gh)
                    {
                        gh.isMarked = (MarkedImageIndex)int.Parse((string)x.Value);
                    }
                }
                if (missingItems > 0)
                {
                    MessageBox.Show("Warning: " + missingItems.ToString() + " items in the state file could not be found in your layout. Skipping.");
                }
            }
            filedia.Dispose();
        }

        public void ToggleMenuBroadcast()
        {
            MenuBar.menuBar_toggleBroadcast();
        }

        public void SetMenuAutotrackerCheck(bool enabled)
        {
            MenuBar.menuBar_AutotrackerCheck(enabled);
        }

        public void UpdateAll()
        {
            //TODO: push all alternates
            foreach (Item x in this.Controls[0].Controls.OfType<Item>())
            {
                x.UpdateImage();
            }
            foreach (CollectedItem x in this.Controls[0].Controls.OfType<CollectedItem>())
            {
                x.UpdateImage();
                x.UpdateCount();
            }
            foreach (DoubleItem x in this.Controls[0].Controls.OfType<DoubleItem>())
            {
                x.UpdateImage();
            }
            foreach (GossipStone x in this.Controls[0].Controls.OfType<GossipStone>())
            {
                // this is not the correct way of doing this but lol. lmao, even
                var lazy = x.GetState();
                x.SetState(lazy);
            }
            foreach (Medallion x in this.Controls[0].Controls.OfType<Medallion>())
            {
                // this is not the correct way of doing this but lol. lmao, even
                var lazy = x.GetState();
                x.SetState(lazy);
            }
            foreach (Song x in this.Controls[0].Controls.OfType<Song>())
            {
                // this is not the correct way of doing this but lol. lmao, even
                var lazy = x.GetState();
                x.SetState(lazy);
            }
            foreach (TextBoxPlus x in this.Controls[0].Controls.OfType<TextBoxPlus>())
            {
                x.Push();
            }
            foreach (SpoilerPanel x in this.Controls[0].Controls.OfType<SpoilerPanel>())
            {
                x.Push();
            }
            ApplyAltsFromSettings(true);
        }
    }
}
