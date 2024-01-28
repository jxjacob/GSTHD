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
using System.Timers;
using System.Windows.Forms;

namespace GSTHD
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> ListPlacesWithTag = new Dictionary<string, string>();
        SortedSet<string> ListPlaces = new SortedSet<string>();
        SortedSet<string> ListSometimesHintsSuggestions = new SortedSet<string>();

        Form1_MenuBar MenuBar;
        public Layout CurrentLayout;
        Panel LayoutContent;
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

            /*
            if(e.KeyCode == Keys.F2)
            {
                var window = new Editor(CurrentLayout);
                window.Show();
            }
            */
        }

        private void LoadAll(object sender, EventArgs e)
        {
            var assembly = Assembly.GetEntryAssembly().GetName();
            this.Text = $"{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title} v{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}";
            this.AcceptButton = null;
            this.MaximizeBox = false;

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

            ListPlaces.Clear();
            ListPlaces.Add("");
            ListPlacesWithTag.Clear();
            currentlyCycling.Clear();
            JObject json_places = JObject.Parse(File.ReadAllText(@"" + Settings.ActivePlaces));
            foreach (var property in json_places)
            {
                ListPlaces.Add(property.Key.ToString());
                ListPlacesWithTag.Add(property.Key, property.Value.ToString());
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
            if (LayoutContent != null) LayoutContent.Dispose();
            LayoutContent = new Panel();
            CurrentLayout = new Layout();
            CurrentLayout.LoadLayout(LayoutContent, Settings, ListSometimesHintsSuggestions, ListPlacesWithTag, this);
            Size = new Size(LayoutContent.Size.Width, LayoutContent.Size.Height + MenuBar.Size.Height);
            LayoutContent.Dock = DockStyle.Top;
            Controls.Add(LayoutContent);
            MenuBar.Dock = DockStyle.Top;
            Controls.Add(MenuBar);
        }

        public void UpdateLayoutFromSettings()
        {
            CurrentLayout.UpdateFromSettings();
            //StoneCyclingTimer.Change(TimeSpan.FromSeconds(Settings.GossipCycleTime), TimeSpan.FromSeconds(Settings.GossipCycleTime));
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
            TheAutotracker = new Autotracker(emulator, offset, this);
        }

        public void SetAutotracker(Process emulator, ulong offset)
        {
            TheAutotracker = new Autotracker(emulator, offset, this);
        }

        private void StopAutotracker()
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
                    int state = x.GetState();
                    if (state != x.DefaultIndex)
                    {
                        thejson.Add(x.Name, state.ToString());
                    }
                }
            }
            foreach (CollectedItem x in this.Controls[0].Controls.OfType<CollectedItem>())
            {
                if (x.Name != "")
                {
                    int state = x.GetState();
                    if (state != x.CollectedItemDefault)
                    {
                        thejson.Add(x.Name, state.ToString());
                    }
                }
            }
            foreach (DoubleItem x in this.Controls[0].Controls.OfType<DoubleItem>())
            {
                if (x.Name != "")
                {
                    int state = x.GetState();
                    if (state != 0)
                    {
                        thejson.Add(x.Name, state.ToString());
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
                    if (conv != "False,,0")
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
                    if (conv != "0,0")
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
                    if (conv != "0,False,,0")
                    {
                        thejson.Add(x.Name, conv);
                    }
                }
            }
            foreach (PanelWothBarren x in this.Controls[0].Controls.OfType<PanelWothBarren>())
            {
                if (x.Name != "")
                {
                    if (x.ListBarren.Count > 0)
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
                    else if (x.ListWotH.Count > 0)
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
                    } else if (x.ListQuantity.Count > 0)
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
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
                saveFileDialog1.Title = "Save state to JSON file";
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
            OpenFileDialog filedia = new OpenFileDialog();
            filedia.Title = "Load state from JSON file";
            filedia.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            filedia.Multiselect = false;
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
                    if (found is Item)
                    {
                        int conv = (int)x.Value;
                        ((Item)found).SetState(conv);
                    } else if (found is DoubleItem)
                    {
                        int conv = (int)x.Value;
                        ((DoubleItem)found).SetState(conv);
                    }
                    else if (found is CollectedItem)
                    {
                        int conv = (int)x.Value;
                        ((CollectedItem)found).SetState(conv);
                    } 
                    else if (found is TextBox)
                    {
                        string conv = (string)x.Value;
                        ((TextBox)found).Text = conv;
                    } 
                    else if (found is GossipStone)
                    {
                        string conv = (string)x.Value;
                        string[] words = conv.Split(',');
                        string[] hi = words[1].Split('|');

                        GossipStoneState newstate = new GossipStoneState()
                        {
                            HoldsImage = Boolean.Parse(words[0]),
                            HeldImages = hi.ToList(),
                            ImageIndex = int.Parse(words[2]),
                        };
                        ((GossipStone)found).SetState(newstate);
                    }
                    else if (found is Medallion)
                    {
                        string conv = (string)x.Value;
                        string[] words = conv.Split(',');
                        MedallionState newstate = new MedallionState()
                        {
                            DungeonIndex = int.Parse(words[0]),
                            ImageIndex = int.Parse(words[1]),
                        };
                        ((Medallion)found).SetState(newstate);
                    }
                    else if (found is Song)
                    {
                        string conv = (string)x.Value;
                        string[] words = conv.Split(',');
                        SongState newstate = new SongState()
                        {
                            ImageIndex = int.Parse(words[0]),
                            MarkerState = new SongMarkerState()
                            {
                                HoldsImage = Boolean.Parse(words[1]),
                                HeldImageName = words[2],
                                ImageIndex = int.Parse(words[3]),
                            },
                        };
                        ((Song)found).SetWholeState(newstate);
                    }
                    else if (found is PanelWothBarren) 
                    {
                        if (((PanelWothBarren)found).isWotH == 0){
                            ((PanelWothBarren)found).SetWotH((string)x.Value);
                        } else if (((PanelWothBarren)found).isWotH == 1){
                            ((PanelWothBarren)found).SetBarren((string)x.Value);
                        } else if (((PanelWothBarren)found).isWotH == 2)
                        {
                            ((PanelWothBarren)found).SetQuantities((string)x.Value);
                        }



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

        public void UpdateAll()
        {
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
        }
    }
}
