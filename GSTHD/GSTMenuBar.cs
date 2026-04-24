using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using static GSTHD.Settings;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

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
            // public MenuItem Autocheck;
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
            { DragButtonOption.None, "None" },
            { DragButtonOption.Left, "Left Click" },
            { DragButtonOption.Middle, "Middle Click" },
            { DragButtonOption.Right, "Right Click" },
            { DragButtonOption.LeftAndRight, "Left + Right Click" },
            { DragButtonOption.Shift, "Shift + Left Click" },
            { DragButtonOption.Control, "Control + Left Click" },
            { DragButtonOption.Alt, "Alt + Left Click" },
        };

        private readonly Dictionary<ExtraActionModButton, string> ExtraButtonNames = new()
            {
                { ExtraActionModButton.None, "None" },
                { ExtraActionModButton.Left, "Left Click" },
                { ExtraActionModButton.Middle, "Middle Click" },
                { ExtraActionModButton.Right, "Right Click" },
                { ExtraActionModButton.MouseButton1, "Mouse Button 1" },
                { ExtraActionModButton.MouseButton2, "Mouse Button 2" },
                { ExtraActionModButton.DoubleLeft, "Double Left Click" },
                { ExtraActionModButton.Shift, "Shift + Left Click" },
                { ExtraActionModButton.Control, "Control + Left Click" },
                { ExtraActionModButton.Alt, "Alt + Left Click" },
            };

        private readonly Dictionary<BasicActionButtonOption, string> BasicButtonNames = new()
            {
                { BasicActionButtonOption.None, "None" },
                { BasicActionButtonOption.Left, "Left Click" },
                { BasicActionButtonOption.Middle, "Middle Click" },
                { BasicActionButtonOption.Right, "Right Click" },
                { BasicActionButtonOption.MouseButton1, "Mouse Button 1" },
                { BasicActionButtonOption.MouseButton2, "Mouse Button 2" },
                { BasicActionButtonOption.Shift, "Shift + Left Click" },
                { BasicActionButtonOption.Control, "Control + Left Click" },
                { BasicActionButtonOption.Alt, "Alt + Left Click" },
            };

        private readonly Dictionary<SongMarkerBehaviourOption, string> SongMarkerBehaviourNames = new()
            {
                { SongMarkerBehaviourOption.None, "None" },
                { SongMarkerBehaviourOption.CheckOnly, "Click to Check" },
                { SongMarkerBehaviourOption.DropOnly, "Drop Items/Songs onto" },
                { SongMarkerBehaviourOption.DragAndDrop, "Full Drag & Drop" },
                { SongMarkerBehaviourOption.DropAndCheck, "Drop Items/Songs onto, Click to Check" },
                { SongMarkerBehaviourOption.Full, "Full Drag & Drop, Click to Check" },
            };

        private readonly Dictionary<SelectEmulatorOption, string> SelectEmulatorNames = new()
            {
                { SelectEmulatorOption.Project64, "Project64 3.0.1 and 4.0.0.5758" },
                { SelectEmulatorOption.Project64_4, "Project64 4.0.0.6556 (or later)" },
                { SelectEmulatorOption.Bizhawk, "Bizhawk-DK64" },
                { SelectEmulatorOption.RMG, "Rosalie's Mupen GUI" },
                { SelectEmulatorOption.simple64, "simple64" },
                { SelectEmulatorOption.parallel, "Parallel Launcher" },
                { SelectEmulatorOption.parallel903, "Parallel Launcher (9.0.3+)" },
                { SelectEmulatorOption.retroarch, "RetroArch" }
            };

        private readonly Dictionary<SpoilerOrderOption, string> SpoilerOrderNames = new()
            {
                { SpoilerOrderOption.Numerical, "Numerical (1-7)" },
                { SpoilerOrderOption.Chronological, "Chronological (Japes-Castle)" },
            };

        private readonly Dictionary<MarkModeOption, string> MarkModeNames = new()
            {
                { MarkModeOption.Checkmark, "Green Checkmark" },
                { MarkModeOption.X, "Red X" },
                { MarkModeOption.Question, "Blue ?" },
                { MarkModeOption.Star, "Yellow Star" },
            };

        private readonly Dictionary<SongFileWriteOption, string> SongFileWriteNames = new()
            {
                { SongFileWriteOption.Disabled, "Disabled" },
                { SongFileWriteOption.Single, "Single TXT File" },
                { SongFileWriteOption.Multi, "Multiple TXT Files" },
            };

        MainWindow Form;
        Settings Settings;
        MenuItems StockMenuItems;
        Dictionary<BasicActionButtonOption, MenuItem> IncrementButtonOptions;
        Dictionary<BasicActionButtonOption, MenuItem> DecrementButtonOptions;
        Dictionary<BasicActionButtonOption, MenuItem> ResetButtonOptions;
        Dictionary<DragButtonOption, MenuItem> DragButtonOptions;
        Dictionary<DragButtonOption, MenuItem> AutocheckDragButtonOptions;
        Dictionary<ExtraActionModButton, MenuItem> ExtraButtonOptions;
        Dictionary<SongMarkerBehaviourOption, MenuItem> SongMarkerBehaviourOptions;
        Dictionary<SelectEmulatorOption, MenuItem> SelectEmulatorOptions;
        Dictionary<SpoilerOrderOption, MenuItem> SpoilerOrderOptions;
        Dictionary<MarkModeOption, MenuItem> MarkModeOptions;
        Dictionary<SongFileWriteOption, MenuItem> SongFileOptions;
        Dictionary<KnownColor, MenuItem> LastWothColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerPointColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerWothColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerEmptyColorOptions;
        Dictionary<KnownColor, MenuItem> SpoilerKindaEmptyColorOptions;
        Dictionary<double, MenuItem> GossipeCycleLengthOptions;
        public double SavedHeight;
        public bool Enabled;
        private bool isLinux;

        delegate void ATCheckCallback(bool enabled);

        public void InitFormsAndSettings(Window form, Settings settings)
        {
            Debug.WriteLine("init started");
            Form = (MainWindow)form;
            Settings = settings;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                isLinux = true;
            }

            MakeMenu();

            SavedHeight = Height;

            ReadSettings();
        }

        private void MakeMenu()
        {
            StockMenuItems = new MenuItems();
            
            var layoutMenu = new MenuItem { Header = "File"};
            {
                
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
                StockMenuItems.OpenPlaces.Click += menuBar_OpenPlaces;
                layoutMenu.Items.Add(StockMenuItems.OpenPlaces);

                
                StockMenuItems.OpenSpoilerHint = new MenuItem
                {
                    Header = "Open Spoiler Log"
                };
                StockMenuItems.OpenSpoilerHint.Click += menuBar_OpenSpoilerHint;
                layoutMenu.Items.Add(StockMenuItems.OpenSpoilerHint);
                
                
                StockMenuItems.Reset = new MenuItem
                {
                    Header = "Reset",
                    InputGesture = new KeyGesture(Key.R, KeyModifiers.Control),
                    HotKey = new KeyGesture(Key.R, KeyModifiers.Control),
                };
                StockMenuItems.Reset.Click += menuBar_Reset;
                layoutMenu.Items.Add(StockMenuItems.Reset);
                
                
                layoutMenu.Items.Add(new Separator());
                
                
                StockMenuItems.SaveState = new MenuItem
                {
                    Header = "Save Tracker State",
                };
                StockMenuItems.SaveState.Click += menuBar_SaveState;
                layoutMenu.Items.Add(StockMenuItems.SaveState);
                
                
                StockMenuItems.LoadState = new MenuItem
                {
                    Header = "Load Tracker State",
                };
                StockMenuItems.LoadState.Click += menuBar_LoadState;
                layoutMenu.Items.Add(StockMenuItems.LoadState);
                
                
                layoutMenu.Items.Add(new Separator());
                
                
                StockMenuItems.ShowMenuBar = new MenuItem
                {
                    Header = "Show Menu Bar",
                    InputGesture = new KeyGesture(Key.F10),
                    HotKey = new KeyGesture(Key.F10),
                    // CheckOnClick = true,
                };
                StockMenuItems.ShowMenuBar.Click += menuBar_Enable;
                layoutMenu.Items.Add(StockMenuItems.ShowMenuBar);

                
                StockMenuItems.BroadcastView = new MenuItem
                {
                    Header = "Broadcast View",
                    InputGesture = new KeyGesture(Key.F2),
                    HotKey = new KeyGesture(Key.F2),
                    // CheckOnClick = true,
                };
                StockMenuItems.BroadcastView.Click += menuBar_Broadcast;
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
                    StockMenuItems.IncrementWraparound.Click += menuBar_ToggleIncrementWraparound;
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
                        temp1.Click += menuBar_SetIncrementButton;
                        IncrementButtonOptions.Add(button.Key, temp1);
                        StockMenuItems.IncrementButton.Items.Add(temp1);
                        var temp2 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        temp2.Click += menuBar_SetDecrementButton;
                        DecrementButtonOptions.Add(button.Key, temp2);
                        StockMenuItems.DecrementButton.Items.Add(temp2);
                        var temp3 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        temp3.Click += menuBar_SetResetButton;
                        ResetButtonOptions.Add(button.Key, temp3);
                        StockMenuItems.ResetButton.Items.Add(temp3);
                    }
                    mouseControlsSubMenu.Items.Add(StockMenuItems.IncrementButton);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.DecrementButton);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.ResetButton);
                    
                    
                    StockMenuItems.ExtraButton = new MenuItem { Header = "\"Mark Item\" Button" };
                    ExtraButtonOptions = new Dictionary<ExtraActionModButton, MenuItem>();
                    foreach (var button in ExtraButtonNames)
                    {
                        var temp = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        temp.Click += menuBar_SetExtraButton;
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
                        temp.Click += menuBar_SetMarkMode;
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
                        temp1.Click += menuBar_SetDragButton;
                        DragButtonOptions.Add(button.Key, temp1);
                        StockMenuItems.DragButton.Items.Add(temp1);
                        
                        var temp2 = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        temp2.Click += menuBar_SetAutocheckDragButton;
                        AutocheckDragButtonOptions.Add(button.Key, temp2);
                        StockMenuItems.AutocheckDragButton.Items.Add(temp2);
                    }
                    mouseControlsSubMenu.Items.Add(StockMenuItems.DragButton);
                    mouseControlsSubMenu.Items.Add(StockMenuItems.AutocheckDragButton);
                    
                    
                    var scrollWheelSubMenu = new MenuItem { Header = "Scroll Wheel" };
                    {
                        StockMenuItems.InvertScrollWheel = new MenuItem  { Header = "Invert Scroll Wheel", ToggleType = MenuItemToggleType.CheckBox };
                        StockMenuItems.InvertScrollWheel.Click += menuBar_ToggleInvertScrollWheel;
                        scrollWheelSubMenu.Items.Add(StockMenuItems.InvertScrollWheel);
                    
                        StockMenuItems.Wraparound = new MenuItem  { Header = "Wraparound Dungeon Names", ToggleType = MenuItemToggleType.CheckBox };
                        StockMenuItems.Wraparound.Click += menuBar_ToggleWraparound;
                        scrollWheelSubMenu.Items.Add(StockMenuItems.Wraparound);
                    }
                    mouseControlsSubMenu.Items.Add(scrollWheelSubMenu);
                }
                optionMenu.Items.Add(mouseControlsSubMenu);
                
                
                
                var songMarkersSubMenu = new MenuItem { Header = "Song Markers" };
                {
                    StockMenuItems.MoveLocation = new MenuItem { Header = "Move Location to Song", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.MoveLocation.Click += menuBar_ToggleMoveLocation;
                    songMarkersSubMenu.Items.Add(StockMenuItems.MoveLocation);
                
                    //Items.Autocheck = new MenuItem("Autocheck Songs", null, new EventHandler(menuBar_ToggleAutocheckSongs))
                    //{
                    //    CheckOnClick = true,
                    //};
                    //songMarkersSubMenu.Items.Add(Items.Autocheck);
                
                    StockMenuItems.Behaviour = new MenuItem { Header = "Default Song Marker Behaviour" };
                    SongMarkerBehaviourOptions = new Dictionary<SongMarkerBehaviourOption, MenuItem>();
                    foreach (var behaviour in SongMarkerBehaviourNames)
                    {
                        var temp = new MenuItem { Header = behaviour.Value, ToggleType = MenuItemToggleType.Radio };
                        temp.Click += menuBar_SetSongMarkerBehaviour;
                        SongMarkerBehaviourOptions.Add(behaviour.Key, temp);
                        StockMenuItems.Behaviour.Items.Add(temp);
                    }
                    songMarkersSubMenu.Items.Add(StockMenuItems.Behaviour);
                }
                optionMenu.Items.Add(songMarkersSubMenu);
                
                
                
                var gossipSubMenu = new MenuItem { Header = "Gossip Stones" };
                {
                    StockMenuItems.OverrideHeldImage = new MenuItem  { Header = "Allow Override of Held Image", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.OverrideHeldImage.Click += menuBar_ToggleOverrideHeldImage;
                    gossipSubMenu.Items.Add(StockMenuItems.OverrideHeldImage);

                    StockMenuItems.StoneOverrideCheckMark = new MenuItem { Header = "Ignore Incoming Marks", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.StoneOverrideCheckMark.Click += menuBar_ToggleOverrideStoneCheckmark;
                    gossipSubMenu.Items.Add(StockMenuItems.StoneOverrideCheckMark);

                    
                    StockMenuItems.CycleLength = new MenuItem  { Header = "Cycle Delay", ToggleType = MenuItemToggleType.Radio };
                    GossipeCycleLengthOptions = new Dictionary<double, MenuItem>();
                    for (double i = 0.25; i <= 2.0; i += 0.25)
                    {
                        var temp =  new MenuItem { Header = $"{i} sec", ToggleType = MenuItemToggleType.Radio };
                        temp.Click += menuBar_SetGossipCycleLength;
                        GossipeCycleLengthOptions.Add(i,temp);
                        StockMenuItems.CycleLength.Items.Add(temp);
                    }
                    gossipSubMenu.Items.Add(StockMenuItems.CycleLength);

                    
                    StockMenuItems.ForceGossipCycles = new MenuItem {  Header = "Allow All Stones to Cycle", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.ForceGossipCycles.Click += menuBar_ToggleForceGossipCycles;
                    gossipSubMenu.Items.Add(StockMenuItems.ForceGossipCycles);
                }
                optionMenu.Items.Add(gossipSubMenu);

                
                var hintPanelSubMenu = new MenuItem { Header = "Hint Panels" };
                {
                    StockMenuItems.WothSubheader = new MenuItem { Header = "WotH" };
                    {
                
                        StockMenuItems.EnableLastWoth = new MenuItem  { Header = "Enable Last WotH", ToggleType = MenuItemToggleType.CheckBox };
                        StockMenuItems.EnableLastWoth.Click += menuBar_ToggleEnableLastWotH;
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
                            temp1.Click += menuBar_SetLastWothColor;
                            LastWothColorOptions.Add(color, temp1);
                            StockMenuItems.LastWothColor.Items.Add(temp1);
                            
                            var temp2 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            temp2.Click += menuBar_SetSpoilerPointColor;
                            SpoilerPointColorOptions.Add(color,temp2);
                            StockMenuItems.SpoilerPointColor.Items.Add(temp2);
                            
                            var temp3 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            temp3.Click += menuBar_SetSpoilerWothColor;
                            SpoilerWothColorOptions.Add(color, temp3);
                            StockMenuItems.SpoilerWothColor.Items.Add(temp3);
                            
                            var temp4 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            temp4.Click += menuBar_SetSpoilerEmptyColor;
                            SpoilerEmptyColorOptions.Add(color, temp4);
                            StockMenuItems.SpoilerEmptyColor.Items.Add(temp4);
                            
                            var temp5 = new MenuItem { Header = color, ToggleType = MenuItemToggleType.Radio };
                            temp5.Click += menuBar_SetSpoilerKindaEmptyColor;
                            SpoilerKindaEmptyColorOptions.Add(color,temp5);
                            StockMenuItems.SpoilerKindaEmptyColor.Items.Add(temp5);
                            
                            i++;
                        }
                        StockMenuItems.WothSubheader.Items.Add(StockMenuItems.LastWothColor);
                
                        
                        StockMenuItems.EnableDuplicateWoth = new MenuItem {  Header = "Allow Duplicate WotH Entries", ToggleType = MenuItemToggleType.CheckBox };
                        StockMenuItems.EnableDuplicateWoth.Click += menuBar_ToggleEnableDuplicateWotH;
                        StockMenuItems.WothSubheader.Items.Add(StockMenuItems.EnableDuplicateWoth);
                    }
                    hintPanelSubMenu.Items.Add(StockMenuItems.WothSubheader);
                
                    StockMenuItems.BarrenSubheader = new MenuItem { Header = "Barren" };
                    {
                        StockMenuItems.EnableBarrenColors = new MenuItem { Header = "Enable Barren Colors", ToggleType = MenuItemToggleType.CheckBox };
                        StockMenuItems.BarrenSubheader.Click += menuBar_ToggleEnableBarrenColors;
                        StockMenuItems.BarrenSubheader.Items.Add(StockMenuItems.EnableBarrenColors);
                    }
                    hintPanelSubMenu.Items.Add(StockMenuItems.BarrenSubheader);
                
                
                    hintPanelSubMenu.Items.Add(new Separator());
                
                    StockMenuItems.EnableHintPathAutofill = new MenuItem {  Header = "Path Goal Autofill (Experimental)", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.EnableHintPathAutofill.Click += menuBar_ToggleEnableHintPathAutofill;
                    hintPanelSubMenu.Items.Add(StockMenuItems.EnableHintPathAutofill);
                    
                    StockMenuItems.EnableHintPathAutofillAggressive = new MenuItem { Header = "Ignore Invalid Autofill Keycodes",  ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.EnableHintPathAutofillAggressive.Click += menuBar_ToggleEnableHintPathAutofillAggressive;
                    hintPanelSubMenu.Items.Add(StockMenuItems.EnableHintPathAutofillAggressive);
                
                }
                optionMenu.Items.Add(hintPanelSubMenu);



                var spoilerSubMenu = new MenuItem { Header = "DK64 Spoiler Hints" };
                {

                    StockMenuItems.SpoilerLevelOrder = new MenuItem { Header = "Level Order" };
                    SpoilerOrderOptions = new Dictionary<SpoilerOrderOption, MenuItem>();
                    foreach (var button in SpoilerOrderNames)
                    {
                        var temp = new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        temp.Click += menuBar_SetSpoilerLevelOrder;
                        SpoilerOrderOptions.Add(button.Key, temp);
                        StockMenuItems.SpoilerLevelOrder.Items.Add(temp);
                    }
                
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerLevelOrder);
                
                    StockMenuItems.SpoilerHideStarting = new MenuItem {  Header = "Hide Starting Moves", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.SpoilerHideStarting.Click += menuBar_ToggleEnableHideStarting;
                    spoilerSubMenu.Items.Add(StockMenuItems.SpoilerHideStarting);
                
                    StockMenuItems.CellOverrideCheckMark = new MenuItem {  Header = "Ignore Incoming Marks", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.CellOverrideCheckMark.Click += menuBar_ToggleOverrideCellCheckmark;
                    spoilerSubMenu.Items.Add(StockMenuItems.CellOverrideCheckMark);
                
                    StockMenuItems.CellCountWothMarks = new MenuItem { Header = "Decrement WotH Count with Marks", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.CellCountWothMarks.Click += menuBar_ToggleOverrideCellCountMarks;
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
                    StockMenuItems.EnableAutosaves.Click += menuBar_ToggleEnableAutosaves;
                    AutosaveSubMenu.Items.Add(StockMenuItems.EnableAutosaves);
                
                
                    StockMenuItems.DeleteOldAutosaves = new MenuItem {  Header = "Automatically Delete Old Autosaves", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.DeleteOldAutosaves.Click += menuBar_ToggleDeleteOldAutosaves;
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
                    if (isLinux && !(button.Key == SelectEmulatorOption.RMG || button.Key == SelectEmulatorOption.retroarch)) continue;
                    var temp = new  MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                    temp.Click += menuBar_SetSelectEmulator;
                    SelectEmulatorOptions.Add(button.Key, temp);
                    StockMenuItems.SelectEmulator.Items.Add(temp);
                }
                MemoryMenu.Items.Add(StockMenuItems.SelectEmulator);
            
                StockMenuItems.SubtractItem = new MenuItem { Header = "Subtract from Collectables", ToggleType = MenuItemToggleType.CheckBox };
                StockMenuItems.SubtractItem.Click += menuBar_ToggleSubtractItem;
                MemoryMenu.Items.Add(StockMenuItems.SubtractItem);
            
            
                var SongTrackerSubmenu = new MenuItem { Header = "Song Tracking"};
                {
                    StockMenuItems.EnableSongTracking = new MenuItem { Header = "Enable (DK64R 4.0+ only)", ToggleType = MenuItemToggleType.CheckBox };
                    StockMenuItems.EnableSongTracking.Click += menuBar_ToggleEnableSongTracking;
                    SongTrackerSubmenu.Items.Add(StockMenuItems.EnableSongTracking);
            
                    StockMenuItems.WriteSongDataToFile = new MenuItem { Header = "Write Song Data to File", ToggleType = MenuItemToggleType.CheckBox };
                    SongFileOptions = new Dictionary<SongFileWriteOption, MenuItem>();
                    foreach (var button in SongFileWriteNames)
                    {
                        var temp =  new MenuItem { Header = button.Value, ToggleType = MenuItemToggleType.Radio };
                        temp.Click += menuBar_SetWriteSongFile;
                        SongFileOptions.Add(button.Key,temp);
                        StockMenuItems.WriteSongDataToFile.Items.Add(temp);
                    }
            
                    SongTrackerSubmenu.Items.Add(StockMenuItems.WriteSongDataToFile);
                }
                MemoryMenu.Items.Add(SongTrackerSubmenu);
            
            
            
                StockMenuItems.ConnectToEmulator = new MenuItem { Header = "Connect to Emulator" };
                
                StockMenuItems.ConnectToEmulator.Click += menuBar_ConnectToEmulator;
                MemoryMenu.Items.Add(StockMenuItems.ConnectToEmulator);
            }
            Items.Add(MemoryMenu);


        }

        public void ReadSettings()
        {
            StockMenuItems.ShowMenuBar.IsChecked = Settings.ShowMenuBar;
            Enabled = Settings.ShowMenuBar;
            
            if (Enabled)
                menuBar_Show();
            else
                menuBar_Hide();
            
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
            //Items.Autocheck.IsChecked = Settings.AutoCheckSongs;
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

            try
            {
                SelectEmulatorOptions[Settings.SelectEmulator].IsChecked = true;
            }
            catch (Exception)
            {
                // ignored
            }
            StockMenuItems.SubtractItem.IsChecked = Settings.SubtractItems;
            StockMenuItems.EnableSongTracking.IsChecked = Settings.EnableSongTracking;
            SongFileOptions[Settings.WriteSongDataToFile].IsChecked = true;
        }

        public void SetRenderer()
        {
            Background = new SolidColorBrush(Form.BackColor);
            
            // TODO: figure out why this doesnt work?
            if (Form.BackColor.ToHsl().L < 0.5)
            {
                Foreground = new SolidColorBrush(Avalonia.Media.Color.FromRgb(255,255,255));
            }
            else
            {
                Foreground = new SolidColorBrush(Avalonia.Media.Color.FromRgb(0,0,0));
            }
        }
        
        public void ClearAlternates()
        {
            // empties menuitem 2 (alternates)
            ((MenuItem)Items[2]).Items.Clear();
        }

        public void AddEmptyAlternatesOption()
        {
            ((MenuItem)Items[2]).Items.Add(new MenuItem{ IsEnabled = false, Header = "None Available" });
        }

        public bool CheckAlternatesForSubmenu(string name, string? collectionname=null)
        {
            if (collectionname != null)
            {
                // if (((ToolStripDropDownItem)MenuStrip.Items[2]).DropDownItems.ContainsKey(collectionname))
                // {
                //     int temp = ((ToolStripDropDownItem)MenuStrip.Items[2]).DropDownItems.IndexOfKey(collectionname);
                //     if (((ToolStripMenuItem)((ToolStripDropDownItem)MenuStrip.Items[2]).DropDownItems[temp]).DropDownItems.ContainsKey(name))
                //     {
                //         //Debug.WriteLine($"found group {name} within colleciton {collectionname}");
                //         return true;
                //     }
                // }
                // return false;

                // rider made this whole long string I don't understand
                // but it finds a submenu with CollectionName, and then searches that submenu for a menu with Name
                return (from MenuItem? mi in ((MenuItem)Items[2]).Items where mi.Header.ToString() == collectionname from MenuItem? mi2 in mi.Items select mi2).Any(mi2 => mi2.Header.ToString() == name);
            }
            return ((MenuItem)Items[2]).Items.Cast<MenuItem?>().Any(mi => mi.Header.ToString() == name);
        }

        //TODO: might also wanna be careful about toolstrip names with spaces instead of underscores

        public void AddToggleToAlternates(string name)
        {
            //Debug.WriteLine("adding toggle " + name);
            var temp = new MenuItem { Header = name, ToggleType = MenuItemToggleType.CheckBox };
            temp.Click += Alternates_GenericFunction;
            ((MenuItem)Items[2]).Items.Add(temp);
        }

        public void AddGroupToAlternates(string groupname, string? collectionname = null)
        {
            //Debug.WriteLine("adding group " + name);
            if (collectionname != null)
            {
                var temp = ((MenuItem)Items[2]).Items.Cast<MenuItem>().First(x => x.Header.ToString() == collectionname);
                temp.Items.Add(new MenuItem{ Header = groupname, ToggleType = MenuItemToggleType.CheckBox });
            } else ((MenuItem)Items[2]).Items.Add(new MenuItem{ Header = groupname, ToggleType = MenuItemToggleType.CheckBox });
        }

        public void AddToAlternatesGroup(string groupname, string name)
        {
            //Debug.WriteLine("adding " + name + " to group " + groupname);
            var item = ((MenuItem)Items[2]).Items.Cast<MenuItem>().First(x => x.Header.ToString() == groupname);
            if (name == string.Empty)
            {
                var temp = new MenuItem { Header = "Disabled", Tag = groupname,ToggleType = MenuItemToggleType.Radio };
                temp.Click += Alternates_GenericFunction;
                item.Items.Add(temp);
            } else
            {
                var temp = new MenuItem { Header = name, Tag = groupname, ToggleType = MenuItemToggleType.Radio };
                temp.Click += Alternates_GenericFunction;
                item.Items.Add(temp);
            }
        }

        public void AddToAlternatesGroupInCollection(string collectionname, string groupname, string name)
        {
            //Debug.WriteLine($"adding {name} to group {groupname} within colleciton {collectionname}");
            // MenuItem temp = (ToolStripDropDownItem)((ToolStripDropDownItem)MenuStrip.Items[2]).DropDownItems.Find(groupname, true)[0];  
            MenuItem temp = ((MenuItem)Items[2]).Items.Cast<MenuItem>().First(x => x.Header.ToString() == groupname);  
            if (name == string.Empty)
            {
                var temp2 = new MenuItem { Header = "Disabled", Tag = $"{collectionname}_:_{groupname}", ToggleType = MenuItemToggleType.CheckBox };
                temp2.Click += Alternates_GenericFunction;
                temp.Items.Add(temp2);
            } else
            {
                var temp2 = new MenuItem { Header = name, Tag = $"{collectionname}_:_{groupname}", ToggleType = MenuItemToggleType.CheckBox };
                temp2.Click += Alternates_GenericFunction;
                temp.Items.Add(temp2);
            }


        }

        public void AddCollectionToAlternates(string groupname)
        {
            //Debug.WriteLine("adding group " + groupname);
            ((MenuItem)Items[2]).Items.Add(new MenuItem { Header = groupname });
        }

        public void AddToAlternatesCollection(string Collectionname, string name)
        {
            //Debug.WriteLine("adding " + name + " to group " + groupname);
            var item = ((MenuItem)Items[2]).Items.Cast<MenuItem>().First(x => x.Header.ToString() == Collectionname);
            var temp = new MenuItem { Header = name, Tag = Tag = $"{Collectionname}_::_", ToggleType = MenuItemToggleType.CheckBox };
            temp.Click += Alternates_GenericFunction;
            item.Items.Add(temp);
        }

        private void Alternates_GenericFunction(object sender, EventArgs e)
        {
            MenuItem choice = (MenuItem)sender;
            string usedtag;
            string LastUsed = string.Empty;
            List<string> words = new List<string>();

            if (choice.Tag != null) words = Regex.Split(choice.Tag?.ToString(), @"_\:_").ToList();
            if (words?.Count == 1) words.Add("");

            if (choice.Tag == null)
            {
                //choice.IsChecked = !choice.IsChecked;
                usedtag = null;
            } else
            {
                if (((words[1] == "" && !choice.Tag.ToString().Contains("_:_")) || ((words[1] != "" && choice.Tag.ToString().Contains("_:_")))) && !choice.Tag.ToString().Contains("_::_"))
                {
                    
                    LastUsed = Settings.GetLastUsed(choice.Tag.ToString());
                    // clicking the already enabled option is pointless, ignore
                    if (choice.Header == LastUsed) return;

                    usedtag = choice.Tag.ToString();

                } else
                {
                    usedtag = "";
                    //choice.IsChecked = !choice.IsChecked;
                }
            }

            Debug.WriteLine(choice.Header + " :: " + choice.Tag + " :: " + choice.IsChecked.ToString());
            Form.CurrentLayout.ApplyAlternates(choice.Header.ToString(), usedtag, choice.IsChecked, LastUsed, false);
            Settings.AddAltSetting(choice.Tag?.ToString(), choice.Header.ToString(), choice.IsChecked);
        }

        public void CheckmarkAlternateOption(string groupname, string name)
        {
            //((ToolStripMenuItem)((ToolStripDropDownItem)MenuStrip.Items[2]).DropDownItems.Find(name, true).First()).Checked = true;

            // [0] = collection         [1] = group
            var words = Regex.Split(groupname, @"_\:_").ToList();

            // for normal toggles
            if (words.Count == 1) words.Add("");

            bool superbreak = false;
            foreach (MenuItem x in ((MenuItem)Items[2]).Items)
            {
                if (superbreak) break;
                if (x.Items.Count > 1)
                {
                    foreach (MenuItem y in x.Items)
                    {
                        if (superbreak) break;
                        if (y.Tag?.ToString() != groupname && words[1] != string.Empty) { 
                            if (y.Items.Count > 1 && words[1] != string.Empty)
                            {
                                foreach (MenuItem z in y.Items)
                                {
                                    if (z.Tag?.ToString() == groupname && z.Header.ToString() == name) { z.IsChecked = true; superbreak = true; break; }
                                }
                            } else break;
                        }
                        if (y.Header.ToString() == name) { y.IsChecked = true; superbreak = true; break; }
                    }
                } else
                {
                    if (x.Header.ToString() == name) { x.IsChecked = true; break; }
                }
            }
        } 

        public async void menuBar_OpenLayout(object? sender, RoutedEventArgs routedEventArgs)
        {
            // open file dialog for jsons
            var toplevel = TopLevel.GetTopLevel(this);
            var files = await toplevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open GST Layout file",
                FileTypeFilter = [FilePickerFileTypes.Json],
                AllowMultiple =  false,
                SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(AppContext.BaseDirectory + "/Layouts"))
            });

            // put that filename into settings' ActiveLayout
            if (files.Count == 1)
            {
                if (files[0].Path.ToString().Contains(AppContext.BaseDirectory))
                {
                    Settings.ActiveLayout = files[0].Path.AbsolutePath[(AppContext.BaseDirectory.Length)..];
                } else
                {
                    Settings.ActiveLayout = files[0].Path.AbsolutePath;
                }
                Settings.Write();
                Form.Reset(sender);
            }
        }
        
        public async void menuBar_OpenPlaces(object? sender, EventArgs e)
        {
            // open file dialog for jsons
            var toplevel = TopLevel.GetTopLevel(this);
            var files = await toplevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open GST Places file",
                FileTypeFilter = [FilePickerFileTypes.Json],
                AllowMultiple =  false,
                SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(AppContext.BaseDirectory))
            });
            // put that filename into settings' ActivePlaces
            if (files.Count == 1)
            {
                if (files[0].Path.ToString().Contains(AppContext.BaseDirectory))
                {
                    Settings.ActivePlaces = files[0].Path.AbsolutePath[(AppContext.BaseDirectory.Length)..];
                } else
                {
                    Settings.ActivePlaces = files[0].Path.AbsolutePath;
                }
                Settings.Write();
                Form.LoadPlaces();
            }
        }

        public async void menuBar_OpenSpoilerHint(object? sender, EventArgs e)
        {
            // open file dialog for jsons
            var toplevel = TopLevel.GetTopLevel(this);
            var files = await toplevel!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open DK64 Spoiler Log",
                FileTypeFilter = [FilePickerFileTypes.Json],
                AllowMultiple =  false,
                SuggestedStartLocation = await toplevel.StorageProvider.TryGetFolderFromPathAsync(new Uri(AppContext.BaseDirectory))
            });
            if (files.Count == 1)
            {
                foreach (Control thing in Form.Layout.Children)
                {
                    if (thing is SpoilerPanel panel)
                    {
                        panel.ImportFromJson(files[0].Path.AbsolutePath);
                        return;
                    }
                }
                // var box = MessageBoxManager.GetMessageBoxStandard("GSTHD",
                //     "Could not find Spoiler Hint object on the current layout.\nMake sure the layout you are using supports Spoiler Hints before opening a spoiler log.",
                //     ButtonEnum.Ok);
                // await box.ShowAsync();
            }
        }
        
        
        public void menuBar_SaveState(object? sender, EventArgs e)
        {
            Form.SaveState();
        }

        public void menuBar_LoadState(object? sender, EventArgs e)
        {
            Form.LoadState();
        }

        public void menuBar_Reset(object? sender, EventArgs e)
        {
            Form.Reset(sender);
        }

        public void menuBar_Enable(object? sender, EventArgs e)
        {
            if (Enabled)
            {
                menuBar_Hide();
                Enabled = false;
            }
            else
            {
                menuBar_Show();
                Enabled = true;
            }

            Settings.ShowMenuBar = Enabled;
            StockMenuItems.ShowMenuBar.IsChecked = Enabled;
            Settings.Write();
        }

        public void menuBar_Broadcast(object? sender, EventArgs e)
        {
            if (Form.CurrentLayout.App_Settings is { EnableBroadcast: true, BroadcastFile: not null })
            {
                if (!StockMenuItems.BroadcastView.IsChecked)
                {
                    BroadcastWindow f2 = new BroadcastWindow();
                    f2.Show();
                    BroadcastManager.SetBroadcast();
                    DispatcherTimer.RunOnce(async () => { Form.UpdateAll(); }, TimeSpan.FromMilliseconds(200));
                    StockMenuItems.BroadcastView.IsChecked = true;
                }
                else if (BroadcastManager.Window != null)
                {
                    BroadcastManager.Window.Close();
                    BroadcastManager.RemoveBroadcast();
                    StockMenuItems.BroadcastView.IsChecked = false;
                }
            } 
            else
            {
                StockMenuItems.BroadcastView.IsChecked = false;
            }
        }

        public void menuBar_AutotrackerCheck(bool enabled)
        {
            Dispatcher.UIThread.Post(() => StockMenuItems.ConnectToEmulator.IsChecked = enabled);
        }

        public void menuBar_toggleBroadcast()
        {
            StockMenuItems.BroadcastView.IsChecked = !StockMenuItems.BroadcastView.IsChecked;
        }

        public void menuBar_Show()
        {
            Height = SavedHeight;
            Form.Height += Height;
            
            // Form.Refresh();
        }

        public void menuBar_Hide()
        {
            Form.Height -= Height;
            Height = 0;
            // Form.Refresh();
        }

        private void menuBar_ToggleInvertScrollWheel(object? sender, EventArgs e)
        {
            // StockMenuItems.InvertScrollWheel.IsChecked = !StockMenuItems.InvertScrollWheel.IsChecked;
            Settings.InvertScrollWheel = StockMenuItems.InvertScrollWheel.IsChecked;
            Settings.Write();
        }

        private void menuBar_ToggleWraparound(object? sender, EventArgs e)
        {
            // StockMenuItems.Wraparound.IsChecked = !StockMenuItems.Wraparound.IsChecked;
            Settings.WraparoundDungeonNames = StockMenuItems.Wraparound.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetDragButton(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            DragButtonOptions[Settings.DragButton].IsChecked = false;
            choice.IsChecked = true;

            var option = DragButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.DragButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetAutocheckDragButton(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            AutocheckDragButtonOptions[Settings.AutocheckDragButton].IsChecked = false;
            choice.IsChecked = true;

            var option = AutocheckDragButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.AutocheckDragButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetIncrementButton(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            IncrementButtonOptions[Settings.IncrementActionButton].IsChecked = false;
            choice.IsChecked = true;

            var option = IncrementButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.IncrementActionButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetDecrementButton(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            DecrementButtonOptions[Settings.DecrementActionButton].IsChecked = false;
            choice.IsChecked = true;

            var option = DecrementButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.DecrementActionButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetResetButton(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            ResetButtonOptions[Settings.ResetActionButton].IsChecked = false;
            choice.IsChecked = true;

            var option = ResetButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.ResetActionButton = option.Key;
            Settings.Write();
        }


        private void menuBar_SetExtraButton(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            ExtraButtonOptions[Settings.ExtraActionButton].IsChecked = false;
            choice.IsChecked = true;

            var option = ExtraButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.ExtraActionButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetSelectEmulator(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SelectEmulatorOptions[Settings.SelectEmulator].IsChecked = false;
            choice.IsChecked = true;

            var option = SelectEmulatorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SelectEmulator = option.Key;
            Settings.Write();
        }

        private void menuBar_SetSpoilerLevelOrder(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SpoilerOrderOptions[Settings.SpoilerOrder].IsChecked = false;
            choice.IsChecked = true;

            var option = SpoilerOrderOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerOrder = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetMarkMode(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            //MarkModeOptions[Settings.EnabledMarks].IsChecked = false;
            choice.IsChecked = !choice.IsChecked;

            var option = MarkModeOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            if (Settings.EnabledMarks.Contains(option.Key)) Settings.EnabledMarks.Remove(option.Key);
            else Settings.EnabledMarks.Add(option.Key);
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetWriteSongFile(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SongFileOptions[Settings.WriteSongDataToFile].IsChecked = false;
            choice.IsChecked = true;

            var option = SongFileOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.WriteSongDataToFile = option.Key;
            Settings.Write();
        }

        //private void menuBar_ToggleAutocheckSongs(object sender, EventArgs e)
        //{
        //    StockMenuItems.Autocheck.IsChecked = !StockMenuItems.Autocheck.IsChecked;
        //    Settings.AutoCheckSongs = StockMenuItems.Autocheck.IsChecked;
        //    Settings.Write();
        //}

        private void menuBar_ToggleMoveLocation(object? sender, EventArgs e)
        {
            // StockMenuItems.MoveLocation.IsChecked = !StockMenuItems.MoveLocation.IsChecked;
            Settings.MoveLocationToSong = StockMenuItems.MoveLocation.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSongMarkerBehaviour(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SongMarkerBehaviourOptions[Settings.SongMarkerBehaviour].IsChecked = false;
            choice.IsChecked = true;

            var option = SongMarkerBehaviourOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SongMarkerBehaviour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableDuplicateWotH(object? sender, EventArgs e)
        {
            // StockMenuItems.EnableLastWoth.Enabled = !StockMenuItems.EnableLastWoth.Enabled;
            Settings.EnableDuplicateWoth = StockMenuItems.EnableDuplicateWoth.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }
        
        private void menuBar_ToggleEnableHintPathAutofill(object? sender, EventArgs e)
        {
            Settings.HintPathAutofill = StockMenuItems.EnableHintPathAutofill.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableHintPathAutofillAggressive(object? sender, EventArgs e)
        {
            Settings.HintPathAutofillAggressive = StockMenuItems.EnableHintPathAutofillAggressive.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableLastWotH(object? sender, EventArgs e)
        {
            // StockMenuItems.EnableLastWoth.Enabled = !StockMenuItems.EnableLastWoth.Enabled;
            Settings.EnableLastWoth = StockMenuItems.EnableLastWoth.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableBarrenColors(object? sender, EventArgs e)
        {
            // StockMenuItems.EnableBarrenColors.Enabled = !StockMenuItems.EnableBarrenColors.Enabled;
            Settings.EnableBarrenColors = StockMenuItems.EnableBarrenColors.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableHideStarting(object? sender, EventArgs e)
        {
            Settings.HideStarting = StockMenuItems.SpoilerHideStarting.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleForceGossipCycles(object? sender, EventArgs e)
        {
            Settings.ForceGossipCycles = StockMenuItems.ForceGossipCycles.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideHeldImage(object? sender, EventArgs e)
        {
            Settings.OverrideHeldImage = StockMenuItems.OverrideHeldImage.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideStoneCheckmark(object? sender, EventArgs e)
        {
            Settings.StoneOverrideCheckMark = StockMenuItems.StoneOverrideCheckMark.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideCellCheckmark(object? sender, EventArgs e)
        {
            Settings.CellOverrideCheckMark = StockMenuItems.CellOverrideCheckMark.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideCellCountMarks(object? sender, EventArgs e)
        {
            Settings.CellCountWothMarks = StockMenuItems.CellCountWothMarks.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleIncrementWraparound(object? sender, EventArgs e)
        {
            Settings.WraparoundItems = StockMenuItems.IncrementWraparound.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleSubtractItem(object? sender, EventArgs e)
        {
            Settings.SubtractItems = StockMenuItems.SubtractItem.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableAutosaves(object? sender, EventArgs e)
        {
            Settings.EnableAutosave = StockMenuItems.EnableAutosaves.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleDeleteOldAutosaves(object? sender, EventArgs e)
        {
            Settings.DeleteOldAutosaves = StockMenuItems.DeleteOldAutosaves.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableSongTracking(object? sender, EventArgs e)
        {
            Settings.EnableSongTracking = StockMenuItems.EnableSongTracking.IsChecked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetGossipCycleLength(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            try
            {
                GossipeCycleLengthOptions[Settings.GossipCycleTime].IsChecked = false;
            }
            catch (Exception)
            {
                // ignored
            }

            choice.IsChecked = true;

            var option = GossipeCycleLengthOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.GossipCycleTime = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetLastWothColor(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            LastWothColorOptions[Settings.LastWothColor].IsChecked = false;
            choice.IsChecked = true;

            var option = LastWothColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.LastWothColor = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerPointColor(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SpoilerPointColorOptions[Settings.SpoilerPointColour].IsChecked = false;
            choice.IsChecked = true;

            var option = SpoilerPointColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerPointColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerWothColor(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SpoilerWothColorOptions[Settings.SpoilerWOTHColour].IsChecked = false;
            choice.IsChecked = true;

            var option = SpoilerWothColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerWOTHColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerEmptyColor(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SpoilerEmptyColorOptions[Settings.SpoilerEmptyColour].IsChecked = false;
            choice.IsChecked = true;

            var option = SpoilerEmptyColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerEmptyColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerKindaEmptyColor(object? sender, EventArgs e)
        {
            var choice = (MenuItem)sender!;

            SpoilerKindaEmptyColorOptions[Settings.SpoilerKindaEmptyColour].IsChecked = false;
            choice.IsChecked = true;

            var option = SpoilerKindaEmptyColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerKindaEmptyColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }
        
        public void menuBar_ConnectToEmulator(object sender, EventArgs e)
        {
            if (Form.CurrentLayout.App_Settings.AutotrackingGame != null)
            {
                if (StockMenuItems.ConnectToEmulator.IsChecked)
                {
                    Form.StopAutotracker();
                    menuBar_AutotrackerCheck(false);
                    return;
                }
                // connect to emulator as speficied through the other setting
                switch (Settings.SelectEmulator.ToString())
                {
                    case "Project64":
                        var resultPJ = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.Project64);
                        if (resultPJ != null)
                        {
                            if (resultPJ.Item1 != null)
                            {
                                Form.SetAutotracker(resultPJ.Item1, resultPJ.Item2);
                                // MessageBox.Show("Connection to PJ64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to PJ64\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "Project64_4":
                        var resultPJ4 = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.Project64_4);
                        if (resultPJ4 != null)
                        {
                            if (resultPJ4.Item1 != null)
                            {
                                Form.SetAutotracker(resultPJ4.Item1, resultPJ4.Item2);
                                // MessageBox.Show("Connection to PJ64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to PJ64\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "Bizhawk":
                        var resultBH = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.Bizhawk);
                        if (resultBH != null)
                        {
                            if (resultBH.Item1 != null)
                            {
                                Form.SetAutotracker(resultBH.Item1, resultBH.Item2);
                                // MessageBox.Show("Connection to Bizhawk-DK64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to Bizhawk-DK64\nMake sure the game you want to track is loaded in the emulator before connecting.\nIf you are experiencing persistent issues, try switching to the _32 exe of GSTHD instead.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "RMG":
                        var resultRMG = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.RMG);
                        if (resultRMG != null)
                        {
                            if (resultRMG.Item1 != null)
                            {
                                Form.SetAutotracker(resultRMG.Item1, resultRMG.Item2);
                                // MessageBox.Show("Connection to RMG sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to RMG\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "simple64":
                        var results64 = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.simple64);
                        if (results64 != null)
                        {
                            if (results64.Item1 != null)
                            {
                                Form.SetAutotracker(results64.Item1, results64.Item2);
                                // MessageBox.Show("Connection to simple64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to simple64\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "parallel":
                        //var resultpar = AttachToEmulators.attachToParallel(Form);
                        var resultpar = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.parallel);
                        if (resultpar != null)
                        {
                            if (resultpar.Item1 != null)
                            {
                                Form.SetAutotracker(resultpar.Item1, resultpar.Item2);
                                // MessageBox.Show("Connection to Parallel Launcher sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to Parallel Launcher\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "parallel903":
                        //var resultpar = AttachToEmulators.attachToParallel(Form);
                        var resultpar903 = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.parallel903);
                        if (resultpar903 != null)
                        {
                            if (resultpar903.Item1 != null)
                            {
                                Form.SetAutotracker(resultpar903.Item1, resultpar903.Item2);
                                // MessageBox.Show("Connection to Parallel Launcher sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to Parallel Launcher\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    case "retroarch":
                        var resultret = AttachToEmulators.attachToEmulator(Form, SelectEmulatorOption.retroarch);
                        if (resultret != null)
                        {
                            if (resultret.Item1 != null)
                            {
                                Form.SetAutotracker(resultret.Item1, resultret.Item2);
                                // MessageBox.Show("Connection to RetroArch sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            // MessageBox.Show("Could not connect to RetroArch\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        break;
                    default:
                        // MessageBox.Show("No supported emulator selected.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;

                }
                menuBar_AutotrackerCheck(true);
            } else
            {
                // MessageBox.Show("Current layout does not support autotracking.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        
    }
}