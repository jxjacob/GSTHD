using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using static GSTHD.Settings;
using System.Xml.Linq;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace GSTHD
{
    
    public class GSTMenuBar : Menu
    {
        // why the FUCK does this need to be here just to use the inhereted rendering method????
        protected override Type StyleKeyOverride => typeof(Menu);

        private struct MenuItems
        {
            // Layout
            public MenuItem OpenLayout;
            public MenuItem OpenPlaces;
            public MenuItem OpenSpoilerHint;
            public MenuItem Reset;
            public MenuItem SaveState;
            public MenuItem LoadState;
            public MenuItem ShowMenuBar;
            public MenuItem BroadcastView;

            // Options
            // Mouse Controls
            public MenuItem IncrementWraparound;
            public MenuItem IncrementButton;
            public MenuItem DecrementButton;
            public MenuItem ResetButton;
            public MenuItem ExtraButton;
            public MenuItem DragButton;
            public MenuItem AutocheckDragButton;
            public MenuItem InvertScrollWheel;
            public MenuItem Wraparound;

            // Song Markers
            public MenuItem MoveLocation;
            // public ToolStripMenuItem Autocheck;
            public MenuItem Behaviour;

            public MenuItem WothSubheader;
            public MenuItem BarrenSubheader;

            // WotH
            public MenuItem EnableLastWoth;
            public MenuItem LastWothColor;
            public MenuItem EnableDuplicateWoth;
            public MenuItem EnableHintPathAutofill;
            public MenuItem EnableHintPathAutofillAggressive;

            // Barren
            public MenuItem EnableBarrenColors;

            // Spoiler Hints
            public MenuItem SpoilerLevelOrder;
            public MenuItem SpoilerHideStarting;
            public MenuItem SpoilerPointColor;
            public MenuItem SpoilerWothColor;
            public MenuItem SpoilerEmptyColor;
            public MenuItem SpoilerKindaEmptyColor;
            public MenuItem CellOverrideCheckMark;
            public MenuItem CellCountWothMarks;

            // Gossip Stone Stuff
            public MenuItem OverrideHeldImage;
            public MenuItem StoneOverrideCheckMark;
            public MenuItem CycleLength;
            public MenuItem ForceGossipCycles;

            //Autosaving
            public MenuItem EnableAutosaves;
            public MenuItem DeleteOldAutosaves;

            // Memory Engine
            public MenuItem SelectEmulator;
            public MenuItem ConnectToEmulator;
            public MenuItem SubtractItem;
            public MenuItem EnableSongTracking;
            public MenuItem WriteSongDataToFile;
        }

        private readonly Dictionary<DragButtonOption, string> DragButtonNames = new()
        {
            { Settings.DragButtonOption.None, "None" },
            { Settings.DragButtonOption.Left, "Left Click" },
            { Settings.DragButtonOption.Middle, "Middle Click" },
            { Settings.DragButtonOption.Right, "Right Click" },
            { Settings.DragButtonOption.LeftAndRight, "Left + Right Click" },
            { Settings.DragButtonOption.Shift, "Shift + Left Click" },
            { Settings.DragButtonOption.Control, "Control + Left Click" },
            { Settings.DragButtonOption.Alt, "Alt + Left Click" },
        };

        private readonly Dictionary<Settings.ExtraActionModButton, string> ExtraButtonNames =
            new Dictionary<Settings.ExtraActionModButton, string>
            {
                { Settings.ExtraActionModButton.None, "None" },
                { Settings.ExtraActionModButton.Left, "Left Click" },
                { Settings.ExtraActionModButton.Middle, "Middle Click" },
                { Settings.ExtraActionModButton.Right, "Right Click" },
                { Settings.ExtraActionModButton.MouseButton1, "Mouse Button 1" },
                { Settings.ExtraActionModButton.MouseButton2, "Mouse Button 2" },
                { Settings.ExtraActionModButton.DoubleLeft, "Double Left Click" },
                { Settings.ExtraActionModButton.Shift, "Shift + Left Click" },
                { Settings.ExtraActionModButton.Control, "Control + Left Click" },
                { Settings.ExtraActionModButton.Alt, "Alt + Left Click" },
            };

        private readonly Dictionary<Settings.BasicActionButtonOption, string> BasicButtonNames =
            new Dictionary<Settings.BasicActionButtonOption, string>
            {
                { Settings.BasicActionButtonOption.None, "None" },
                { Settings.BasicActionButtonOption.Left, "Left Click" },
                { Settings.BasicActionButtonOption.Middle, "Middle Click" },
                { Settings.BasicActionButtonOption.Right, "Right Click" },
                { Settings.BasicActionButtonOption.MouseButton1, "Mouse Button 1" },
                { Settings.BasicActionButtonOption.MouseButton2, "Mouse Button 2" },
                { Settings.BasicActionButtonOption.Shift, "Shift + Left Click" },
                { Settings.BasicActionButtonOption.Control, "Control + Left Click" },
                { Settings.BasicActionButtonOption.Alt, "Alt + Left Click" },
            };

        private readonly Dictionary<Settings.SongMarkerBehaviourOption, string> SongMarkerBehaviourNames =
            new Dictionary<Settings.SongMarkerBehaviourOption, string>
            {
                { Settings.SongMarkerBehaviourOption.None, "None" },
                { Settings.SongMarkerBehaviourOption.CheckOnly, "Click to Check" },
                { Settings.SongMarkerBehaviourOption.DropOnly, "Drop Items/Songs onto" },
                { Settings.SongMarkerBehaviourOption.DragAndDrop, "Full Drag & Drop" },
                { Settings.SongMarkerBehaviourOption.DropAndCheck, "Drop Items/Songs onto, Click to Check" },
                { Settings.SongMarkerBehaviourOption.Full, "Full Drag & Drop, Click to Check" },
            };

        private readonly Dictionary<Settings.SelectEmulatorOption, string> SelectEmulatorNames =
            new Dictionary<Settings.SelectEmulatorOption, string>
            {
                { Settings.SelectEmulatorOption.Project64, "Project64 3.0.1 and 4.0.0.5758" },
                { Settings.SelectEmulatorOption.Project64_4, "Project64 4.0.0.6556 (or later)" },
                { Settings.SelectEmulatorOption.Bizhawk, "Bizhawk-DK64" },
                { Settings.SelectEmulatorOption.RMG, "Rosalie's Mupen GUI" },
                { Settings.SelectEmulatorOption.simple64, "simple64" },
                { Settings.SelectEmulatorOption.parallel, "Parallel Launcher" },
                { Settings.SelectEmulatorOption.parallel903, "Parallel Launcher (9.0.3+)" },
                { Settings.SelectEmulatorOption.retroarch, "RetroArch" }
            };

        private readonly Dictionary<Settings.SpoilerOrderOption, string> SpoilerOrderNames =
            new Dictionary<Settings.SpoilerOrderOption, string>
            {
                { Settings.SpoilerOrderOption.Numerical, "Numerical (1-7)" },
                { Settings.SpoilerOrderOption.Chronological, "Chronological (Japes-Castle)" },
            };

        private readonly Dictionary<Settings.MarkModeOption, string> MarkModeNames =
            new Dictionary<Settings.MarkModeOption, string>
            {
                { Settings.MarkModeOption.Checkmark, "Green Checkmark" },
                { Settings.MarkModeOption.X, "Red X" },
                { Settings.MarkModeOption.Question, "Blue ?" },
                { Settings.MarkModeOption.Star, "Yellow Star" },
            };

        private readonly Dictionary<Settings.SongFileWriteOption, string> SongFileWriteNames =
            new Dictionary<Settings.SongFileWriteOption, string>
            {
                { Settings.SongFileWriteOption.Disabled, "Disabled" },
                { Settings.SongFileWriteOption.Single, "Single TXT File" },
                { Settings.SongFileWriteOption.Multi, "Multiple TXT Files" },
            };

        Window Form;
        Settings Settings;
        MenuItems StockMenuItems;
        Dictionary<Settings.BasicActionButtonOption, MenuItem> IncrementButtonOptions;
        Dictionary<Settings.BasicActionButtonOption, MenuItem> DecrementButtonOptions;
        Dictionary<Settings.BasicActionButtonOption, MenuItem> ResetButtonOptions;
        Dictionary<Settings.DragButtonOption, MenuItem> DragButtonOptions;
        Dictionary<Settings.DragButtonOption, MenuItem> AutocheckDragButtonOptions;
        Dictionary<Settings.ExtraActionModButton, MenuItem> ExtraButtonOptions;
        Dictionary<Settings.SongMarkerBehaviourOption, MenuItem> SongMarkerBehaviourOptions;
        Dictionary<Settings.SelectEmulatorOption, MenuItem> SelectEmulatorOptions;
        Dictionary<Settings.SpoilerOrderOption, MenuItem> SpoilerOrderOptions;
        Dictionary<Settings.MarkModeOption, MenuItem> MarkModeOptions;
        Dictionary<Settings.SongFileWriteOption, MenuItem> SongFileOptions;
        Dictionary<KnownColor, MenuItem> LastWothColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerPointColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerWothColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerEmptyColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerKindaEmptyColorOptions;
        Dictionary<double, MenuItem> GossipeCycleLengthOptions;
        double SavedHeight;
        private bool Enabled;

        delegate void ATCheckCallback(bool enabled);

        public void InitFormsAndSettings(Window form, Settings settings)
        {
            Debug.WriteLine("init started");
            Form = form;
            Settings = settings;

            MakeMenu();

            SavedHeight = Height;

            ReadSettings();
        }

        private void MakeMenu()
        {
            StockMenuItems = new MenuItems();
            
            var layoutMenu = new MenuItem { Header = "File"};
            {
                                
                // StockMenuItems.zzzz = new MenuItem
                // {
                //     Header = "zzzz",
                // };
                // StockMenuItems.zzzz.Click += menuBar_zzzz;
                // layoutMenu.Items.Add(StockMenuItems.zzzz);
                
                
                StockMenuItems.OpenLayout = new MenuItem
                {
                    Header = "Open Layout",
                    InputGesture = new KeyGesture(Key.O, KeyModifiers.Control),
                    HotKey = new KeyGesture(Key.O, KeyModifiers.Control),
                };
                StockMenuItems.OpenLayout.Click += menuBar_OpenLayout;
                layoutMenu.Items.Add(StockMenuItems.OpenLayout);
                
                
                StockMenuItems.OpenPlaces = new MenuItem
                {
                    Header = "Open Places",
                    InputGesture = new KeyGesture(Key.O, KeyModifiers.Control | KeyModifiers.Shift),
                    HotKey = new KeyGesture(Key.O, KeyModifiers.Control | KeyModifiers.Shift),
                };
                StockMenuItems.OpenPlaces.Click += menubar_OpenPlaces;
                layoutMenu.Items.Add(StockMenuItems.OpenPlaces);

                
                StockMenuItems.OpenSpoilerHint = new MenuItem
                {
                    Header = "Open Spoiler Log"
                };
                // StockMenuItems.OpenSpoilerHint.Click += menuBar_OpenSpoilerHint;
                layoutMenu.Items.Add(StockMenuItems.OpenSpoilerHint);
                
                
                StockMenuItems.Reset = new MenuItem
                {
                    Header = "Reset",
                    InputGesture = new KeyGesture(Key.R, KeyModifiers.Control),
                    HotKey = new KeyGesture(Key.R, KeyModifiers.Control),
                };
                // StockMenuItems.Reset.Click += menuBar_Reset;
                layoutMenu.Items.Add(StockMenuItems.Reset);
                
                
                layoutMenu.Items.Add(new Separator());
                
                
                StockMenuItems.SaveState = new MenuItem
                {
                    Header = "Save Tracker State",
                };
                // StockMenuItems.SaveState.Click += menuBar_SaveState;
                layoutMenu.Items.Add(StockMenuItems.SaveState);
                
                
                StockMenuItems.LoadState = new MenuItem
                {
                    Header = "Load Tracker State",
                };
                // StockMenuItems.LoadState.Click += menuBar_LoadState;
                layoutMenu.Items.Add(StockMenuItems.LoadState);
                
                
                layoutMenu.Items.Add(new Separator());
                
                
                StockMenuItems.ShowMenuBar = new MenuItem
                {
                    Header = "Show Menu Bar",
                    InputGesture = new KeyGesture(Key.F10),
                    HotKey = new KeyGesture(Key.F10),
                    // CheckOnClick = true,
                };
                // StockMenuItems.ShowMenuBar.Click += menuBar_Enable;
                layoutMenu.Items.Add(StockMenuItems.ShowMenuBar);

                
                StockMenuItems.BroadcastView = new MenuItem
                {
                    Header = "Broadcast View",
                    InputGesture = new KeyGesture(Key.F2),
                    HotKey = new KeyGesture(Key.F2),
                    // CheckOnClick = true,
                };
                // StockMenuItems.BroadcastView.Click += menuBar_Broadcast;
                layoutMenu.Items.Add(StockMenuItems.BroadcastView);

            }
            Items.Add(layoutMenu);
            
            
            var optionMenu = new MenuItem { Header = "Global Options"};
            {
                var mouseControlsSubMenu = new MenuItem { Header = "Mouse Controls"};
                {
                    
                    StockMenuItems.IncrementWraparound = new MenuItem
                    {
                        Header = "Increment/Decrement Wraparound",
                        ToggleType = MenuItemToggleType.CheckBox
                    };
                    // StockMenuItems.IncrementWraparound.Click += menuBar_ToggleIncrementWraparound;
                    mouseControlsSubMenu.Items.Add(StockMenuItems.IncrementWraparound);
                    
                
                    StockMenuItems.IncrementButton = new MenuItem { Header = "\"Increment Item\" Button" };
                    StockMenuItems.DecrementButton = new MenuItem { Header = "\"Decrement Item\" Button" };
                    StockMenuItems.ResetButton = new MenuItem { Header = "\"Reset Item\" Button" };
                    IncrementButtonOptions = new Dictionary<BasicActionButtonOption, MenuItem>();
                    DecrementButtonOptions = new Dictionary<BasicActionButtonOption, MenuItem>();
                    ResetButtonOptions = new Dictionary<BasicActionButtonOption, MenuItem>();
                    foreach (var button in BasicButtonNames)
                    {
                        var temp1 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio};
                        // temp1.Click += menuBar_SetIncrementButton;
                        IncrementButtonOptions.Add(button.Key, temp1);
                        StockMenuItems.IncrementButton.Items.Add(temp1);
                        var temp2 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp2.Click += menuBar_SetDecrementButton;
                        DecrementButtonOptions.Add(button.Key, temp2);
                        StockMenuItems.DecrementButton.Items.Add(temp2);
                        var temp3 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp3.Click += menuBar_SetResetButton;
                        ResetButtonOptions.Add(button.Key, temp3);
                        StockMenuItems.ResetButton.Items.Add(temp3);
                    }
                    mouseControlsSubMenu.Items.Add(StockMenuItems.IncrementButton);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.DecrementButton);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.ResetButton);
                    
                    
                    StockMenuItems.ExtraButton = new MenuItem { Header = "\"Mark Item\" Button" };;
                    ExtraButtonOptions = new Dictionary<ExtraActionModButton, MenuItem>();
                    foreach (var button in ExtraButtonNames)
                    {
                        var temp = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp1.Click += menuBar_SetExtraButton;
                        ExtraButtonOptions.Add(button.Key, temp);
                        StockMenuItems.ExtraButton.Items.Add(temp);
                    }
                    StockMenuItems.ExtraButton.Items.Add(new Separator());
                    // add the two options
                    var MarkModeOrder = new MenuItem { Header = "Enabled Marks" };
                    MarkModeOptions = new Dictionary<MarkModeOption, MenuItem>();
                    foreach (var button in MarkModeNames)
                    {
                        var temp = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.CheckBox };
                        // temp1.Click += menuBar_SetMarkMode;
                        MarkModeOptions.Add(button.Key, temp);
                        MarkModeOrder.Items.Add(temp);
                    }
                    StockMenuItems.ExtraButton.Items.Add(MarkModeOrder);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.ExtraButton);
                    
                    
                    StockMenuItems.DragButton = new MenuItem { Header = "Drag Button" };
                    StockMenuItems.AutocheckDragButton = new MenuItem { Header = "Autocheck Drag Button" };
                    DragButtonOptions = new Dictionary<DragButtonOption, MenuItem>();
                    AutocheckDragButtonOptions = new Dictionary<DragButtonOption, MenuItem>();
                    foreach (var button in DragButtonNames)
                    {
                        var temp1 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp1.Click += menuBar_SetDragButton;
                        DragButtonOptions.Add(button.Key, temp1);
                        StockMenuItems.DragButton.Items.Add(temp1);
                        
                        var temp2 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp2.Click += menuBar_SetAutocheckDragButton;
                        AutocheckDragButtonOptions.Add(button.Key, temp2);
                        StockMenuItems.AutocheckDragButton.Items.Add(temp2);
                    }
                    mouseControlsSubMenu.Items.Add(StockMenuItems.DragButton);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.AutocheckDragButton);
                    
                    
                    var scrollWheelSubMenu = new MenuItem { Header = "Scroll Wheel" };
                    {
                        StockMenuItems.InvertScrollWheel = new MenuItem  { Header = "Invert Scroll Wheel", ToggleType = MenuItemToggleType.CheckBox };
                        // StockMenuItems.InvertScrollWheel.Click += menuBar_ToggleInvertScrollWheel;
                        scrollWheelSubMenu.Items.Add(StockMenuItems.InvertScrollWheel);
                    
                        StockMenuItems.Wraparound = new MenuItem  { Header = "Wraparound Dungeon Names", ToggleType = MenuItemToggleType.CheckBox };
                        // StockMenuItems.Wraparound.Click += menuBar_ToggleWraparound;
                        scrollWheelSubMenu.Items.Add(StockMenuItems.Wraparound);
                    }
                    mouseControlsSubMenu.Items.Add(scrollWheelSubMenu);
                }
                optionMenu.Items.Add(mouseControlsSubMenu);
                
                
                
                var songMarkersSubMenu = new MenuItem { Header = "Song Markers" };
                {
                    StockMenuItems.MoveLocation = new MenuItem { Header = "Move Location to Song", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.MoveLocation.Click += menuBar_ToggleMoveLocation;
                    songMarkersSubMenu.Items.Add(StockMenuItems.MoveLocation);
                
                    //Items.Autocheck = new ToolStripMenuItem("Autocheck Songs", null, new EventHandler(menuBar_ToggleAutocheckSongs))
                    //{
                    //    CheckOnClick = true,
                    //};
                    //songMarkersSubMenu.Items.Add(Items.Autocheck);
                
                    StockMenuItems.Behaviour = new MenuItem { Header = "Default Song Marker Behaviour" };
                    SongMarkerBehaviourOptions = new Dictionary<SongMarkerBehaviourOption, MenuItem>();
                    foreach (var behaviour in SongMarkerBehaviourNames)
                    {
                        var temp = new MenuItem { Header = behaviour.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp.Click += menuBar_SetSongMarkerBehaviour;
                        SongMarkerBehaviourOptions.Add(behaviour.Key, temp);
                        StockMenuItems.Behaviour.Items.Add(temp);
                    }
                    songMarkersSubMenu.Items.Add(StockMenuItems.Behaviour);
                }
                optionMenu.Items.Add(songMarkersSubMenu);
                
                
                
                var gossipSubMenu = new MenuItem { Header = "Gossip Stones" };
                {
                    StockMenuItems.OverrideHeldImage = new MenuItem  { Header = "Allow Override of Held Image", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.OverrideHeldImage.Click += menuBar_ToggleOverrideHeldImage;
                    gossipSubMenu.Items.Add(StockMenuItems.OverrideHeldImage);

                    StockMenuItems.StoneOverrideCheckMark = new MenuItem { Header = "Ignore Incoming Marks", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.StoneOverrideCheckMark.Click += menuBar_ToggleOverrideStoneCheckmark;
                    gossipSubMenu.Items.Add(StockMenuItems.StoneOverrideCheckMark);

                    
                    StockMenuItems.CycleLength = new MenuItem  { Header = "Cycle Delay", ToggleType = MenuItemToggleType.Radio };
                    GossipeCycleLengthOptions = new Dictionary<double, MenuItem>();
                    for (double i = 0.25; i <= 2.0; i += 0.25)
                    {
                        var temp =  new MenuItem { Header = $"{i} sec", ToggleType = MenuItemToggleType.Radio };
                        // temp.Click += menuBar_SetGossipCycleLength;
                        GossipeCycleLengthOptions.Add(i,temp);
                        StockMenuItems.CycleLength.Items.Add(temp);
                    }
                    gossipSubMenu.Items.Add(StockMenuItems.CycleLength);

                    
                    StockMenuItems.ForceGossipCycles = new MenuItem {  Header = "Allow All Stones to Cycle", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.ForceGossipCycles.Click += menuBar_ToggleForceGossipCycles;
                    gossipSubMenu.Items.Add(StockMenuItems.ForceGossipCycles);
                }
                optionMenu.Items.Add(gossipSubMenu);

                
                var hintPanelSubMenu = new MenuItem { Header = "Hint Panels" };
                {
                    StockMenuItems.WothSubheader = new MenuItem { Header = "WotH" };
                    {
                
                        StockMenuItems.EnableLastWoth = new MenuItem  { Header = "Enable Last WotH", ToggleType = MenuItemToggleType.CheckBox };
                        // StockMenuItems.EnableLastWoth.Click += menuBar_ToggleEnableLastWotH;
                        StockMenuItems.WothSubheader.Items.Add(StockMenuItems.EnableLastWoth);


                        StockMenuItems.LastWothColor = new MenuItem { Header = "Last WotH Color" };
                        // these are for a different submenu but need to be declared here for efficiency
                        StockMenuItems.SpoilerPointColor = new MenuItem { Header = "Point Number Color" };
                        StockMenuItems.SpoilerWothColor = new MenuItem { Header = "WotH Number Color" };
                        StockMenuItems.SpoilerEmptyColor = new MenuItem { Header = "\"0 Points Left\" Color" };
                        StockMenuItems.SpoilerKindaEmptyColor = new MenuItem { Header = "\"0 Points Left (w/ Faded)\" Color" };
                        LastWothColorOptions = new Dictionary<KnownColor, MenuItem>();
                        SpoilerPointColorOptions = new Dictionary<KnownColor, MenuItem>();
                        SpoilerWothColorOptions = new Dictionary<KnownColor, MenuItem>();
                        SpoilerEmptyColorOptions = new Dictionary<KnownColor, MenuItem>();
                        SpoilerKindaEmptyColorOptions = new Dictionary<KnownColor, MenuItem>();
                
                        const int firstColorId = 28;
                        const int lastColorId = 167;
                
                        for (int i = firstColorId; i <= lastColorId; i++)
                        {
                            var color = (KnownColor)i;
                            
                            var temp1 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            // temp1.Click += menuBar_SetLastWothColor;
                            LastWothColorOptions.Add(color, temp1);
                            StockMenuItems.LastWothColor.Items.Add(temp1);
                            
                            var temp2 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            // temp2.Click += menuBar_SetSpoilerPointColor;
                            SpoilerPointColorOptions.Add(color,temp2);
                            StockMenuItems.SpoilerPointColor.Items.Add(temp2);
                            
                            var temp3 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            // temp3.Click += menuBar_SetSpoilerWothColor;
                            SpoilerWothColorOptions.Add(color, temp3);
                            StockMenuItems.SpoilerWothColor.Items.Add(temp3);
                            
                            var temp4 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            // temp4.Click += menuBar_SetSpoilerEmptyColor;
                            SpoilerEmptyColorOptions.Add(color, temp4);
                            StockMenuItems.SpoilerEmptyColor.Items.Add(temp4);
                            
                            var temp5 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            // temp5.Click += menuBar_SetSpoilerKindaEmptyColor;
                            SpoilerKindaEmptyColorOptions.Add(color,temp5);
                            StockMenuItems.SpoilerKindaEmptyColor.Items.Add(temp5);
                            
                            i++;
                        }
                        StockMenuItems.WothSubheader.Items.Add(StockMenuItems.LastWothColor);
                
                        
                        StockMenuItems.EnableDuplicateWoth = new MenuItem {  Header = "Allow Duplicate WotH Entries", ToggleType = MenuItemToggleType.CheckBox };
                        // StockMenuItems.EnableDuplicateWoth.Click += menuBar_ToggleEnableDuplicateWotH;
                        StockMenuItems.WothSubheader.Items.Add(StockMenuItems.EnableDuplicateWoth);
                    }
                    hintPanelSubMenu.Items.Add(StockMenuItems.WothSubheader);
                
                    StockMenuItems.BarrenSubheader = new MenuItem { Header = "Barren" };
                    {
                        StockMenuItems.EnableBarrenColors = new MenuItem { Header = "Enable Barren Colors", ToggleType = MenuItemToggleType.CheckBox };
                        // StockMenuItems.BarrenSubheader.Click += menuBar_ToggleEnableBarrenColors;
                        StockMenuItems.BarrenSubheader.Items.Add(StockMenuItems.EnableBarrenColors);
                    }
                    hintPanelSubMenu.Items.Add(StockMenuItems.BarrenSubheader);
                
                
                    hintPanelSubMenu.Items.Add(new Separator());
                
                    StockMenuItems.EnableHintPathAutofill = new MenuItem {  Header = "Path Goal Autofill (Experimental)", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.EnableHintPathAutofill.Click += menuBar_ToggleEnableHintPathAutofill;
                    hintPanelSubMenu.Items.Add(StockMenuItems.EnableHintPathAutofill);
                    
                    StockMenuItems.EnableHintPathAutofillAggressive = new MenuItem { Header = "Ignore Invalid Autofill Keycodes",  ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.EnableHintPathAutofillAggressive.Click += menuBar_ToggleEnableHintPathAutofillAggressive;
                    hintPanelSubMenu.Items.Add(StockMenuItems.EnableHintPathAutofillAggressive);
                
                }
                optionMenu.Items.Add(hintPanelSubMenu);



                var spoilerSubMenu = new MenuItem { Header = "DK64 Spoiler Hints" };
                {

                    StockMenuItems.SpoilerLevelOrder = new MenuItem { Header = "Level Order" };
                    SpoilerOrderOptions = new Dictionary<Settings.SpoilerOrderOption, MenuItem>();
                    foreach (var button in SpoilerOrderNames)
                    {
                        var temp = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp.Click += menuBar_SetSpoilerLevelOrder;
                        SpoilerOrderOptions.Add(button.Key, temp);
                        StockMenuItems.SpoilerLevelOrder.Items.Add(temp);
                    }
                
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerLevelOrder);
                
                    StockMenuItems.SpoilerHideStarting = new MenuItem {  Header = "Hide Starting Moves", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.SpoilerHideStarting.Click += menuBar_ToggleEnableHideStarting;
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerHideStarting);
                
                    StockMenuItems.CellOverrideCheckMark = new MenuItem {  Header = "Ignore Incoming Marks", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.CellOverrideCheckMark.Click += menuBar_ToggleOverrideCellCheckmark;
                    spoilerSubMenu.Items.Add(StockMenuItems.CellOverrideCheckMark);
                
                    StockMenuItems.CellCountWothMarks = new MenuItem { Header = "Decrement WotH Count with Marks", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.CellCountWothMarks.Click += menuBar_ToggleOverrideCellCountMarks;
                    spoilerSubMenu.Items.Add(StockMenuItems.CellCountWothMarks);
                
                    // these were initialized elsewhere, actually adding them here
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerPointColor);
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerKindaEmptyColor);
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerEmptyColor);
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerWothColor);
                
                }
                optionMenu.Items.Add(spoilerSubMenu);
                
                
                
                var AutosaveSubMenu = new MenuItem { Header = "Autosaves" };
                {
                    StockMenuItems.EnableAutosaves = new MenuItem { Header = "Enable Autosaving", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.EnableAutosaves.Click += menuBar_ToggleEnableAutosaves;
                    AutosaveSubMenu.Items.Add(StockMenuItems.EnableAutosaves);
                
                
                    StockMenuItems.DeleteOldAutosaves = new MenuItem {  Header = "Automatically Delete Old Autosaves", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.DeleteOldAutosaves.Click += menuBar_ToggleDeleteOldAutosaves;
                    AutosaveSubMenu.Items.Add(StockMenuItems.DeleteOldAutosaves);
                }
                optionMenu.Items.Add(AutosaveSubMenu);
            }
            Items.Add(optionMenu);
            
            
            Items.Add(new MenuItem { Header = "Layout Options"});

            var MemoryMenu = new MenuItem { Header = "Autotracker" };
            {
                StockMenuItems.SelectEmulator = new MenuItem { Header = "Select Emulator" };
                SelectEmulatorOptions = new Dictionary<SelectEmulatorOption, MenuItem>();
                foreach (var button in SelectEmulatorNames)
                {
                    var temp = new  MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                    // temp.Click += menuBar_SetSelectEmulator;
                    SelectEmulatorOptions.Add(button.Key, temp);
                }
                MemoryMenu.Items.Add(StockMenuItems.SelectEmulator);
            
                StockMenuItems.SubtractItem = new MenuItem { Header = "Subtract from Collectables", ToggleType = MenuItemToggleType.CheckBox };
                // StockMenuItems.SubtractItem.Click += menuBar_ToggleSubtractItem;
                MemoryMenu.Items.Add(StockMenuItems.SubtractItem);
            
            
                var SongTrackerSubmenu = new MenuItem { Header = "Song Tracking"};
                {
                    StockMenuItems.EnableSongTracking = new MenuItem { Header = "Enable (DK64R 4.0+ only)", ToggleType = MenuItemToggleType.CheckBox };
                    // StockMenuItems.EnableSongTracking.Click += menuBar_ToggleEnableSongTracking;
                    SongTrackerSubmenu.Items.Add(StockMenuItems.EnableSongTracking);
            
                    StockMenuItems.WriteSongDataToFile = new MenuItem { Header = "Write Song Data to File", ToggleType = MenuItemToggleType.CheckBox };
                    SongFileOptions = new Dictionary<Settings.SongFileWriteOption, MenuItem>();
                    foreach (var button in SongFileWriteNames)
                    {
                        var temp =  new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        // temp.Click += menuBar_SetWriteSongFile;
                        SongFileOptions.Add(button.Key,temp);
                        StockMenuItems.WriteSongDataToFile.Items.Add(temp);
                    }
            
                    SongTrackerSubmenu.Items.Add(StockMenuItems.WriteSongDataToFile);
                }
                MemoryMenu.Items.Add(SongTrackerSubmenu);
            
            
            
                StockMenuItems.ConnectToEmulator = new MenuItem { Header = "Connect to Emulator" };
                // StockMenuItems.ConnectToEmulator.Click += menuBar_ConnectToEmulator;
                MemoryMenu.Items.Add(StockMenuItems.ConnectToEmulator);
            }
            Items.Add(MemoryMenu);


        }

        public void ReadSettings()
        {
            StockMenuItems.ShowMenuBar.IsChecked = Settings.ShowMenuBar;
            Enabled = Settings.ShowMenuBar;
            // if (Enabled)
            //     menuBar_Show();
            // else
            //     menuBar_Hide();
            
            StockMenuItems.InvertScrollWheel.IsChecked = Settings.InvertScrollWheel;
            StockMenuItems.Wraparound.IsChecked = Settings.WraparoundDungeonNames;
            
            StockMenuItems.IncrementWraparound.IsChecked = Settings.WraparoundItems;
            DragButtonOptions[Settings.DragButton].IsChecked = true;
            AutocheckDragButtonOptions[Settings.AutocheckDragButton].IsChecked = true;
            ExtraButtonOptions[Settings.ExtraActionButton].IsChecked = true;
            if (Settings.EnabledMarks != null)
            {
                foreach (var mark in Settings.EnabledMarks)
                {
                    MarkModeOptions[mark].IsChecked = true;
                }
            }
            else
            {
                // this is the first time EnabledMarks is being set, so its gonna have checkmark set later
                MarkModeOptions[MarkModeOption.Checkmark].IsChecked = true;
            }
            IncrementButtonOptions[Settings.IncrementActionButton].IsChecked = true;
            DecrementButtonOptions[Settings.DecrementActionButton].IsChecked = true;
            ResetButtonOptions[Settings.ResetActionButton].IsChecked = true;
            
            StockMenuItems.MoveLocation.IsChecked = Settings.MoveLocationToSong;
            //Items.Autocheck.Checked = Settings.AutoCheckSongs;
            SongMarkerBehaviourOptions[Settings.SongMarkerBehaviour].IsChecked = true;
            
            StockMenuItems.EnableHintPathAutofill.IsChecked = Settings.HintPathAutofill;
            StockMenuItems.EnableHintPathAutofillAggressive.IsChecked = Settings.HintPathAutofillAggressive;
            StockMenuItems.EnableDuplicateWoth.IsChecked = Settings.EnableDuplicateWoth;
            StockMenuItems.EnableLastWoth.IsChecked = Settings.EnableLastWoth;
            LastWothColorOptions[Settings.LastWothColor].IsChecked = true;
            
            StockMenuItems.EnableBarrenColors.IsChecked = Settings.EnableBarrenColors;
            
            StockMenuItems.OverrideHeldImage.IsChecked = Settings.OverrideHeldImage;
            StockMenuItems.CellOverrideCheckMark.IsChecked = Settings.CellOverrideCheckMark;
            StockMenuItems.CellCountWothMarks.IsChecked = Settings.CellCountWothMarks;
            StockMenuItems.StoneOverrideCheckMark.IsChecked = Settings.StoneOverrideCheckMark;
            StockMenuItems.ForceGossipCycles.IsChecked = Settings.ForceGossipCycles;
            
            StockMenuItems.EnableAutosaves.IsChecked = Settings.EnableAutosave;
            StockMenuItems.DeleteOldAutosaves.IsChecked = Settings.DeleteOldAutosaves;
            try
            {
                GossipeCycleLengthOptions[Settings.GossipCycleTime].IsChecked = true;
            }
            catch (Exception)
            {
                // ignored
            }
            
            SpoilerOrderOptions[Settings.SpoilerOrder].IsChecked = true;
            
            
            StockMenuItems.SpoilerHideStarting.IsChecked = Settings.HideStarting;
            SpoilerPointColorOptions[Settings.SpoilerPointColour].IsChecked = true;
            SpoilerWothColorOptions[Settings.SpoilerWOTHColour].IsChecked = true;
            SpoilerEmptyColorOptions[Settings.SpoilerEmptyColour].IsChecked = true;
            SpoilerKindaEmptyColorOptions[Settings.SpoilerKindaEmptyColour].IsChecked = true;
            
            SelectEmulatorOptions[Settings.SelectEmulator].IsChecked = true;
            StockMenuItems.SubtractItem.IsChecked = Settings.SubtractItems;
            StockMenuItems.EnableSongTracking.IsChecked = Settings.EnableSongTracking;
            SongFileOptions[Settings.WriteSongDataToFile].IsChecked = true;
        }

        public void SetRenderer()
        {
            Background = Form.Background;

            // ProfessionalColorTable theme;
            // if (MenuStrip.BackColor.GetBrightness() < 0.5)
            // {
            //     this.font = Color.White;
            //     theme = new Form1_MenuBar_ColorTable_DarkTheme(MenuStrip.BackColor);
            // }
            // else
            // {
            //     MenuStrip.ForeColor = Color.Black;
            //     theme = new Form1_MenuBar_ColorTable_LightTheme(MenuStrip.BackColor);
            // }
            //
            // MenuStrip.Renderer = new ToolStripProfessionalRenderer(theme);
        }

        public async void menuBar_OpenLayout(object? sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("menuBar_OpenLayout");
            // open file dialog for jsons

            var toplevel = TopLevel.GetTopLevel(this);

            var files = await toplevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open GST Layout file",
                FileTypeFilter = [FilePickerFileTypes.Json],
                AllowMultiple =  false,
                // might need Application.StartupPath in here too
                SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync("\\Layouts")
            });

            // put that filename into settings' ActiveLayout
            if (files.Count == 1)
            {
                // if (files[0].Path.Contains(Application.StartupPath))
                // {
                //     Settings.ActiveLayout = filedia.FileName.Substring((Application.StartupPath.Length + 1));
                // } else
                // {
                //     Settings.ActiveLayout = filedia.FileName.ToString();
                // }
                // Settings.Write();
                // Form.Reset(sender);
                Debug.WriteLine($"Open GST Layout file: {files[0].Path}");
            }
        }
        
        private void menubar_OpenPlaces(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}