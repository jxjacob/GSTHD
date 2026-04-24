using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using Newtonsoft.Json.Linq;

namespace GSTHD;

public interface GSTForms
{
    void Reset(object sender);

    void SetLayoutColor(Color color);
    IBrush? Background { get; }
    string? Name { get; }
    string? Title { get; }
    double Width { get; set; }
    double Height { get; set; }
    Canvas LayoutContent { get; set; }

    // Control.ControlCollection Controls { get; }

}

public partial class MainWindow : Window, GSTForms
{
    Dictionary<string, string> ListPlacesWithTag = new();
    Dictionary<string, string> ListKeycodesWithTag = new();
    SortedSet<string> ListSometimesHintsSuggestions = [];
    
    public Layout CurrentLayout { get; set; }
    public Canvas LayoutContent { get; set; }
    public Autotracker TheAutotracker;
    public Timer? StoneCyclingTimer;
    private int cyclecount = 0;

    public Color BackColor { get; set; }

    public List<object> currentlyCycling = [];
    public Settings Settings;
    
    private SparkleUpdater _sparkle;
    
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoadAll(object sender, RoutedEventArgs e)
    {
        var assembly = Assembly.GetExecutingAssembly().GetName();
        if (assembly.Version != null) Title = $"GSTHD: DK64 Edition v{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build} {((!Environment.Is64BitProcess) ? "(32-bit)" : string.Empty)}";
        CanMaximize = false;
        LayoutContent = Layout;
        
        _sparkle = new SparkleUpdater(
            "https://raw.githubusercontent.com/jxjacob/GSTHD/master/GSTHD/SparkleUpdater.xml",
            new Ed25519Checker(SecurityMode.Strict, 
                "NLQIYkFDCK4wVRpMEyYk3dX47JTBD52VyuaxnEEiRcI=") // your base 64 public key -- generate this with the NetSparkleUpdater.Tools.AppCastGenerator .NET CLI tool on any OS
        ) {
            UIFactory = new NetSparkleUpdater.UI.Avalonia.UIFactory(), // or null, or choose some other UI factory, or build your own IUIFactory implementation!
            RelaunchAfterUpdate = true, // default is false; set to true if you want your app to restart after updating (keep as false if your installer will start your app for you)
            UseNotificationToast = false,
        };
        Dispatcher.UIThread.Post(() => _sparkle.StartLoop(true));  // `true` to run an initial check online -- only call StartLoop **once** for a given SparkleUpdater instance! 
        
        LoadSettings();

        MenuBar.InitFormsAndSettings(this, Settings);
        
        LoadLayout();
        SetMenuBar();
        if (Settings.DeleteOldAutosaves) CleanUpOldAutos();
        
    }
    
    private void Reload(bool changeLayout = false)
    {
        if (!changeLayout)
        {
            if (CurrentLayout.App_Settings.EnableBroadcast && BroadcastManager.Window != null)
            {
                ((BroadcastWindow)BroadcastManager.GetBroadcast()).Reload();
            }
        } else
        {
            if (BroadcastManager.Window != null)
            {
                ((BroadcastWindow)BroadcastManager.GetBroadcast()).Layout.Children.Clear();
            }
        }
        LoadLayout();
        SetMenuBar();
    }

    public void LoadSettings()
    {
        Settings = Settings.Read();
        Settings.PathReformat();

        LoadPlaces();

        ListSometimesHintsSuggestions.Clear();
        JObject json_hints = JObject.Parse(File.ReadAllText(@"" + Settings.ActiveSometimesHints));
        foreach (var category in json_hints)
        {
            foreach (var hint in category.Value!)
            {
                ListSometimesHintsSuggestions.Add(hint.ToString());
            }
        }
    }
    
    public void LoadPlaces()
    {
        ListKeycodesWithTag.Clear();
        ListPlacesWithTag.Clear();
        currentlyCycling.Clear();
        JObject json_places = JObject.Parse(File.ReadAllText(@"" + Settings.ActivePlaces));
        if (json_places.ContainsKey("places") && json_places.ContainsKey("keycodes"))
        {
            //new format
            foreach (var property in JObject.Parse(json_places.SelectToken("places")!.ToString()))
            {
                ListPlacesWithTag.Add(property.Key, property.Value!.ToString());
            }

            foreach (var property in JObject.Parse(json_places.SelectToken("keycodes")!.ToString()))
            {
                ListKeycodesWithTag.Add(property.Key, property.Value!.ToString());
            }

        }
        else
        {
            // legacy format
            foreach (var property in json_places)
            {
                ListPlacesWithTag.Add(property.Key, property.Value!.ToString());
            }

        }
    }
    
    private void SetMenuBar()
    {
        // this is the function that changes the font colour, mostly.
        MenuBar.SetRenderer();
    }
    
    private static void CleanUpOldAutos()
    {
        // list everything in dir
        // if more than 25, get the oldest ones then nuke em
        if (!Directory.Exists("Autosaves")) return;
        var files = Directory.GetFiles("Autosaves").OrderBy(f => new FileInfo(f).CreationTime);
        int todelete = files.Count() - 25;
        if (todelete <= 0) return;
        foreach (string file in files)
        {
            //Debug.WriteLine(file + " gets deleted");
            FileInfo fi = new FileInfo(file);
            fi.Delete();
            todelete--;
            if (todelete == 0) break;
        }

    }
    
    private void LoadLayout()
    {
        Layout.Children.Clear();
        if (StoneCyclingTimer != null)
        {
            StoneCyclingTimer.Elapsed -= IncrementStones;
            StoneCyclingTimer.Stop();
            StoneCyclingTimer.Close();
            StoneCyclingTimer = null;
        }
        StoneCyclingTimer = new System.Timers.Timer(Settings.GossipCycleTime*1000);
        StoneCyclingTimer.Elapsed += IncrementStones;
        StoneCyclingTimer.AutoReset = true;
        StoneCyclingTimer.Enabled = true;
        CurrentLayout = new Layout();
        CurrentLayout.LoadLayout(Layout, Settings, ListSometimesHintsSuggestions, ListPlacesWithTag, ListKeycodesWithTag, this);
        
        Width = Layout.Width;
        Height = Layout.Height;
        if (MenuBar.Enabled) Height += MenuBar.SavedHeight;
        ApplyAltsFromSettings();
    }

    public void SetLayoutColor(Color color)
    {
        BackColor = color;
        Layout.Background = new SolidColorBrush(color);
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
                    BroadcastWindow f2 = BroadcastManager.GetBroadcast();
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
            // CurrentLayout.UpdateFromSettings();
            // i CANNOT figure out why the broadcast view wont accept the new settings across the items, but the main view will
            //if (this.CurrentLayout.App_Settings.EnableBroadcast && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
            //{
            //    Debug.WriteLine("pushing new settings to broadcast");
            //    ((BroadcastWindow)Application.OpenForms["GSTHD_DK64 Broadcast View"]).ForceLoadSettings(Settings);
            //    ((BroadcastWindow)Application.OpenForms["GSTHD_DK64 Broadcast View"]).UpdateLayoutFromSettings();
            //}
            StoneCyclingTimer.Close();
            StoneCyclingTimer = new System.Timers.Timer(Settings.GossipCycleTime * 1000);
            StoneCyclingTimer.Elapsed += IncrementStones;
            StoneCyclingTimer.AutoReset = true;
            StoneCyclingTimer.Enabled = true;
        }

        public void Reset(object? sender)
        {
            Layout.Children.Clear();
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
                    if (((MenuItem)sender).Header.ToString() == "Open Layout")
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
                // CurrentLayout.ListUpdatables.Remove(TheAutotracker);
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

        private void IncrementStones(object? state, ElapsedEventArgs elapsedEventArgs)
        {
            // for stone in updatables
            // stone.incrementcycle
            try
            {
                foreach (var obj in currentlyCycling)
                {
                    if (obj is GossipStone gs)
                    {
                        if (gs.HeldImages.Count > 1) Dispatcher.UIThread.Post(() => gs.IncrementCycle());
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
                if (Settings.EnableAutosave) Dispatcher.UIThread.Post(() => SaveState(true));
            }
        }

        public async void SaveState(bool force = false)
        {
            JObject thejson = new JObject();
        
            foreach (Item x in this.Layout.Children.OfType<Item>())
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
            foreach (CollectedItem x in this.Layout.Children.OfType<CollectedItem>())
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
            foreach (DoubleItem x in this.Layout.Children.OfType<DoubleItem>())
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
            foreach (TextBox x in this.Layout.Children.OfType<TextBox>())
            {
                if (x.Name != "" && x.Text != "") 
                {
                    thejson.Add(x.Name, x.Text);
                }
            }
            foreach (GossipStone x in this.Layout.Children.OfType<GossipStone>())
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
            foreach (Medallion x in this.Layout.Children.OfType<Medallion>())
            {
                if (x.Name != "")
                {
                    MedallionState state = x.GetState();
                    string conv = state.ToString();
                    if (conv != $"{x.DefaultDungeonIndex},0,0")
                    {
                        thejson.Add(x.Name, conv);
                    }
                }
            }
            foreach (Song x in this.Layout.Children.OfType<Song>())
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
            foreach (PanelWothBarren x in this.Layout.Children.OfType<PanelWothBarren>())
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
                            thejson.Add(x.Name, x.GetAlls());
                        }
                    }
                }
            }
            foreach (SpoilerPanel x in this.Layout.Children.OfType<SpoilerPanel>())
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
            foreach (GuaranteedHint x in this.Layout.Children.OfType<GuaranteedHint>())
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
                    File.WriteAllText(@"Autosaves/auto" + DateTime.Now.ToString("MM-dd-yyyy") + "_" + this.Settings.GetLayoutName() + ".json", thejson.ToString());
        
                }
            }
            else
            {
                // open file dialog for jsons
                var toplevel = TopLevel.GetTopLevel(this);
                var files = await toplevel!.StorageProvider.SaveFilePickerWithResultAsync(new FilePickerSaveOptions
                {
                    Title = "Save state to JSON file",
                    FileTypeChoices = [FilePickerFileTypes.Json],
                    SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(AppContext.BaseDirectory + "/Layouts"))
                });

                // put that filename into settings' ActiveLayout
                if (files.File.Name != "")
                    await File.WriteAllTextAsync(files.File.Name, thejson.ToString());
                
            }
        }
        
        public async void LoadState()
        {
            // open file dialog for jsons
            var toplevel = TopLevel.GetTopLevel(this);
            var files = await toplevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load State from JSON file",
                FileTypeFilter = [FilePickerFileTypes.Json],
                AllowMultiple =  false,
                SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(
                    $"{AppContext.BaseDirectory}/Layouts"))
            });

            // put that filename into settings' ActiveLayout
            if (files.Count == 1)
            {
                // reset for safekeepings
                Reset(null);
                //all of the fucking things
                JObject loadedjson = JObject.Parse(await File.ReadAllTextAsync(files[0].Path.AbsolutePath));
                int missingItems = 0;
                foreach (JProperty x in (JToken)loadedjson)
                {
                    Control found = null;
                    try
                    {
                        found = this.Layout.Children.First(y => y.Name == x.Name);
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
                        } else if (pa.isWotH == HintPanelType.Mixed){
                            pa.SetAlls((string)x.Value);
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
                    var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                        $"Warning: {missingItems.ToString()} items in the state file could not be found in your layout. Skipping.",
                        ButtonEnum.Ok);
                    await box.ShowAsync();
                }
            }
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
            foreach (Item x in this.Layout.Children.OfType<Item>())
            {
                x.UpdateImage();
            }
            foreach (CollectedItem x in this.Layout.Children.OfType<CollectedItem>())
            {
                x.UpdateImage();
                x.UpdateCount();
            }
            foreach (DoubleItem x in this.Layout.Children.OfType<DoubleItem>())
            {
                x.UpdateImage();
            }
            foreach (GossipStone x in this.Layout.Children.OfType<GossipStone>())
            {
                // this is not the correct way of doing this but lol. lmao, even
                var lazy = x.GetState();
                x.SetState(lazy);
            }
            foreach (Medallion x in this.Layout.Children.OfType<Medallion>())
            {
                // this is not the correct way of doing this but lol. lmao, even
                var lazy = x.GetState();
                x.SetState(lazy);
            }
            foreach (Song x in this.Layout.Children.OfType<Song>())
            {
                // this is not the correct way of doing this but lol. lmao, even
                var lazy = x.GetState();
                x.SetState(lazy);
            }
            foreach (TextBoxPlus x in this.Layout.Children.OfType<TextBoxPlus>())
            {
                x.Push();
            }
            foreach (SpoilerPanel x in this.Layout.Children.OfType<SpoilerPanel>())
            {
                x.Push();
            }
            ApplyAltsFromSettings(true);
        }
}