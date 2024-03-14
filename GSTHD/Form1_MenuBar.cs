using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static GSTHD.Settings;

namespace GSTHD
{
    public class Form1_MenuBar : Panel
    {
        private struct MenuItems
        {
            // Layout
            public ToolStripMenuItem OpenLayout;
            public ToolStripMenuItem OpenPlaces;
            public ToolStripMenuItem OpenSpoilerHint;
            public ToolStripMenuItem Reset;
            public ToolStripMenuItem SaveState;
            public ToolStripMenuItem LoadState;
            public ToolStripMenuItem ShowMenuBar;
            public ToolStripMenuItem BroadcastView;
            //public ToolStripMenuItem ZoomIn;
            //public ToolStripMenuItem ZoomOut;
            //public ToolStripMenuItem ZoomReset;

            // Options
            // Mouse Controls
            public ToolStripMenuItem IncrementWraparound;
            public ToolStripMenuItem IncrementButton;
            public ToolStripMenuItem DecrementButton;
            public ToolStripMenuItem ResetButton;
            public ToolStripMenuItem ExtraButton;
            public ToolStripMenuItem DragButton;
            public ToolStripMenuItem AutocheckDragButton;
            public ToolStripMenuItem InvertScrollWheel;
            public ToolStripMenuItem Wraparound;

            // Song Markers
            public ToolStripMenuItem MoveLocation;
            // public ToolStripMenuItem Autocheck;
            public ToolStripMenuItem Behaviour;

            // WotH
            public ToolStripMenuItem EnableLastWoth;
            public ToolStripMenuItem LastWothColor;
            public ToolStripMenuItem EnableDuplicateWoth;

            // Barren
            public ToolStripMenuItem EnableBarrenColors;

            // Spoiler Hints
            public ToolStripMenuItem SpoilerLevelOrder;
            public ToolStripMenuItem SpoilerHideStarting;
            public ToolStripMenuItem SpoilerPointColor;
            public ToolStripMenuItem SpoilerWothColor;
            public ToolStripMenuItem SpoilerEmptyColor;
            public ToolStripMenuItem SpoilerKindaEmptyColor;
            public ToolStripMenuItem CellOverrideCheckMark;

            // Gossip Stone Stuff
            public ToolStripMenuItem OverrideHeldImage;
            public ToolStripMenuItem StoneOverrideCheckMark;
            public ToolStripMenuItem CycleLength;
            public ToolStripMenuItem ForceGossipCycles;

            //Autosaving
            public ToolStripMenuItem EnableAutosaves;
            public ToolStripMenuItem DeleteOldAutosaves;

            // Memory Engine
            public ToolStripMenuItem SelectEmulator;
            public ToolStripMenuItem ConnectToEmulator;
            public ToolStripMenuItem SubtractItem;
        }

        private readonly Dictionary<Settings.DragButtonOption, string> DragButtonNames = new Dictionary<Settings.DragButtonOption, string>
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

        private readonly Dictionary<Settings.ExtraActionModButton, string> ExtraButtonNames = new Dictionary<Settings.ExtraActionModButton, string>
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

        private readonly Dictionary<Settings.BasicActionButtonOption, string> BasicButtonNames = new Dictionary<Settings.BasicActionButtonOption, string>
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

        private readonly Dictionary<Settings.SongMarkerBehaviourOption, string> SongMarkerBehaviourNames = new Dictionary<Settings.SongMarkerBehaviourOption, string>
        {
            { Settings.SongMarkerBehaviourOption.None, "None" },
            { Settings.SongMarkerBehaviourOption.CheckOnly, "Click to Check" },
            { Settings.SongMarkerBehaviourOption.DropOnly, "Drop Items/Songs onto" },
            { Settings.SongMarkerBehaviourOption.DragAndDrop, "Full Drag && Drop" },
            { Settings.SongMarkerBehaviourOption.DropAndCheck, "Drop Items/Songs onto, Click to Check" },
            { Settings.SongMarkerBehaviourOption.Full, "Full Drag && Drop, Click to Check" },
        };

        private readonly Dictionary<Settings.SelectEmulatorOption, string> SelectEmulatorNames = new Dictionary<Settings.SelectEmulatorOption, string>
        {
            { Settings.SelectEmulatorOption.Project64, "Project64 3.0.1" },
            { Settings.SelectEmulatorOption.Bizhawk, "Bizhawk-DK64" },
            { Settings.SelectEmulatorOption.RMG, "Rosalie's Mupen GUI" },
            { Settings.SelectEmulatorOption.simple64, "simple64" }
        };

        private readonly Dictionary<Settings.SpoilerOrderOption, string> SpoilerOrderNames = new Dictionary<Settings.SpoilerOrderOption, string>
        {
            { Settings.SpoilerOrderOption.Numerical, "Numerical (1-7)" },
            { Settings.SpoilerOrderOption.Chronological, "Chronological (Japes-Castle)" },
        };

        Form1 Form;
        Settings Settings;
        MenuStrip MenuStrip;
        MenuItems Items;
        Dictionary<Settings.BasicActionButtonOption, ToolStripMenuItem> IncrementButtonOptions;
        Dictionary<Settings.BasicActionButtonOption, ToolStripMenuItem> DecrementButtonOptions;
        Dictionary<Settings.BasicActionButtonOption, ToolStripMenuItem> ResetButtonOptions;
        Dictionary<Settings.DragButtonOption, ToolStripMenuItem> DragButtonOptions;
        Dictionary<Settings.DragButtonOption, ToolStripMenuItem> AutocheckDragButtonOptions;
        Dictionary<Settings.ExtraActionModButton, ToolStripMenuItem> ExtraButtonOptions;
        Dictionary<Settings.SongMarkerBehaviourOption, ToolStripMenuItem> SongMarkerBehaviourOptions;
        Dictionary<Settings.SelectEmulatorOption, ToolStripMenuItem> SelectEmulatorOptions;
        Dictionary<Settings.SpoilerOrderOption, ToolStripMenuItem> SpoilerOrderOptions;
        Dictionary<KnownColor, ToolStripMenuItem> LastWothColorOptions;
        Dictionary<KnownColor, ToolStripMenuItem> SpoilerPointColorOptions;
        Dictionary<KnownColor, ToolStripMenuItem> SpoilerWothColorOptions;
        Dictionary<KnownColor, ToolStripMenuItem> SpoilerEmptyColorOptions;
        Dictionary<KnownColor, ToolStripMenuItem> SpoilerKindaEmptyColorOptions;
        Dictionary<double, ToolStripMenuItem> GossipeCycleLengthOptions;
        Size SavedSize;

        public Form1_MenuBar(Form1 form, Settings settings)
        {
            Form = form;
            Settings = settings;

            MakeMenu();

            Size = MenuStrip.Size;
            SavedSize = MenuStrip.Size;

            ReadSettings();

            Controls.Add(MenuStrip);
        }

        private void MakeMenu()
        {
            MenuStrip = new MenuStrip();
            Items = new MenuItems();

            var layoutMenu = new ToolStripMenuItem("Layout");
            {
                Items.OpenLayout = new ToolStripMenuItem("Open Layout", null, new EventHandler(menuBar_OpenLayout))
                {
                    ShortcutKeys = Keys.Control | Keys.O,
                    ShowShortcutKeys = true,
                };
                layoutMenu.DropDownItems.Add(Items.OpenLayout);

                Items.OpenPlaces = new ToolStripMenuItem("Open Places", null, new EventHandler(menuBar_OpenPlaces))
                {
                    ShortcutKeys = Keys.Control | Keys.Shift | Keys.O,
                    ShowShortcutKeys = true,
                };
                layoutMenu.DropDownItems.Add(Items.OpenPlaces);

                Items.OpenSpoilerHint = new ToolStripMenuItem("Open Spoiler Log", null, new EventHandler(menuBar_OpenSpoilerHint))
                {
                };
                layoutMenu.DropDownItems.Add(Items.OpenSpoilerHint);

                Items.Reset = new ToolStripMenuItem("Reset", null, new EventHandler(menuBar_Reset))
                {
                    ShortcutKeys = Keys.Control | Keys.R,
                    ShowShortcutKeys = true,
                };
                layoutMenu.DropDownItems.Add(Items.Reset);

                layoutMenu.DropDownItems.Add("-");

                Items.SaveState = new ToolStripMenuItem("Save Tracker State", null, new EventHandler(menuBar_SaveState))
                {
                    //ShortcutKeys = Keys.Control | Keys.Shift | Keys.S,
                    //ShowShortcutKeys = true,
                };

                layoutMenu.DropDownItems.Add(Items.SaveState);

                Items.LoadState = new ToolStripMenuItem("Load Tracker State", null, new EventHandler(menuBar_LoadState))
                {
                    //ShortcutKeys = Keys.Control | Keys.Shift | Keys.O,
                    //ShowShortcutKeys = true,
                };

                layoutMenu.DropDownItems.Add(Items.LoadState);

                layoutMenu.DropDownItems.Add("-");

                Items.ShowMenuBar = new ToolStripMenuItem("Show Menu Bar", null, new EventHandler(menuBar_Enable))
                {
                    ShortcutKeys = Keys.F10,
                    ShowShortcutKeys = true,
                    CheckOnClick = true,
                };
                layoutMenu.DropDownItems.Add(Items.ShowMenuBar);


                Items.BroadcastView = new ToolStripMenuItem("Broadcast View", null, new EventHandler(menuBar_Broadcast))
                {
                    ShortcutKeys = Keys.F2,
                    ShowShortcutKeys = true,
                    CheckOnClick = true,
                };
                layoutMenu.DropDownItems.Add(Items.BroadcastView);

                //layoutMenu.DropDownItems.Add("-");

                //Items.ZoomIn = new ToolStripMenuItem("Zoom In", null, new EventHandler(menuBar_ZoomIn))
                //{
                //    ShortcutKeys = Keys.Control | Keys.Oemplus,
                //    ShowShortcutKeys = true,
                //    ShortcutKeyDisplayString = "Ctrl++"
                //};
                //layoutMenu.DropDownItems.Add(Items.ZoomIn);

                //Items.ZoomOut = new ToolStripMenuItem("Zoom Out", null, new EventHandler(menuBar_ZoomOut))
                //{
                //    ShortcutKeys = Keys.Control | Keys.OemMinus,
                //    ShowShortcutKeys = true,
                //    ShortcutKeyDisplayString = "Ctrl+-"
                //};
                //layoutMenu.DropDownItems.Add(Items.ZoomOut);

                //Items.ZoomReset = new ToolStripMenuItem("Reset Zoom", null, new EventHandler(menuBar_ZoomReset))
                //{
                //    ShortcutKeys = Keys.Control | Keys.D0,
                //    ShowShortcutKeys = true,
                //};
                //layoutMenu.DropDownItems.Add(Items.ZoomReset);


            }
            MenuStrip.Items.Add(layoutMenu);

            var optionMenu = new ToolStripMenuItem("Options");
            {

                var dragDropSubMenu = new ToolStripMenuItem("Mouse Controls");
                {
                    Items.IncrementWraparound = new ToolStripMenuItem("Increment/Decrement Wraparound", null, new EventHandler(menuBar_ToggleIncrementWraparound))
                    {
                        CheckOnClick = true,
                    };
                    dragDropSubMenu.DropDownItems.Add(Items.IncrementWraparound);

                    IncrementButtonOptions = new Dictionary<Settings.BasicActionButtonOption, ToolStripMenuItem>();
                    DecrementButtonOptions = new Dictionary<Settings.BasicActionButtonOption, ToolStripMenuItem>();
                    ResetButtonOptions = new Dictionary<Settings.BasicActionButtonOption, ToolStripMenuItem>();
                    foreach (var button in BasicButtonNames)
                    {
                        IncrementButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetIncrementButton)));
                        DecrementButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetDecrementButton)));
                        ResetButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetResetButton)));
                    }
                    Items.IncrementButton = new ToolStripMenuItem("\"Increment Item\" Button", null, IncrementButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.IncrementButton);

                    Items.DecrementButton = new ToolStripMenuItem("\"Decrement Item\" Button", null, DecrementButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.DecrementButton);

                    Items.ResetButton = new ToolStripMenuItem("\"Reset Item\" Button", null, ResetButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.ResetButton);





                    ExtraButtonOptions = new Dictionary<Settings.ExtraActionModButton, ToolStripMenuItem>();
                    foreach (var button in ExtraButtonNames)
                    {
                        ExtraButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetExtraButton)));
                    }
                    Items.ExtraButton = new ToolStripMenuItem("\"Checkmark Item\" Button", null, ExtraButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.ExtraButton);



                    DragButtonOptions = new Dictionary<Settings.DragButtonOption, ToolStripMenuItem>();
                    AutocheckDragButtonOptions = new Dictionary<Settings.DragButtonOption, ToolStripMenuItem>();

                    foreach (var button in DragButtonNames)
                    {
                        DragButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetDragButton)));
                        AutocheckDragButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetAutocheckDragButton)));
                    }
                    
                    Items.DragButton = new ToolStripMenuItem("Drag Button", null, DragButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.DragButton);

                    Items.AutocheckDragButton = new ToolStripMenuItem("Autocheck Drag Button", null, AutocheckDragButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.AutocheckDragButton);

                    



                    var scrollWheelSubMenu = new ToolStripMenuItem("Scroll Wheel");
                    {
                        Items.InvertScrollWheel = new ToolStripMenuItem("Invert Scroll Wheel", null, new EventHandler(menuBar_ToggleInvertScrollWheel))
                        {
                            CheckOnClick = true,
                        };
                        scrollWheelSubMenu.DropDownItems.Add(Items.InvertScrollWheel);

                        Items.Wraparound = new ToolStripMenuItem("Wraparound Dungeon Names", null, new EventHandler(menuBar_ToggleWraparound))
                        {
                            CheckOnClick = true,
                        };
                        scrollWheelSubMenu.DropDownItems.Add(Items.Wraparound);
                    }
                    dragDropSubMenu.DropDownItems.Add(scrollWheelSubMenu);


                }
                optionMenu.DropDownItems.Add(dragDropSubMenu);

                var songMarkersSubMenu = new ToolStripMenuItem("Song Markers");
                {
                    Items.MoveLocation = new ToolStripMenuItem("Move Location to Song", null, new EventHandler(menuBar_ToggleMoveLocation))
                    {
                        CheckOnClick = true,
                    };
                    songMarkersSubMenu.DropDownItems.Add(Items.MoveLocation);

                    //Items.Autocheck = new ToolStripMenuItem("Autocheck Songs", null, new EventHandler(menuBar_ToggleAutocheckSongs))
                    //{
                    //    CheckOnClick = true,
                    //};
                    //songMarkersSubMenu.DropDownItems.Add(Items.Autocheck);

                    SongMarkerBehaviourOptions = new Dictionary<Settings.SongMarkerBehaviourOption, ToolStripMenuItem>();

                    int i = 0;
                    foreach (var behaviour in SongMarkerBehaviourNames)
                    {
                        SongMarkerBehaviourOptions.Add(behaviour.Key, new ToolStripMenuItem(behaviour.Value, null, new EventHandler(menuBar_SetSongMarkerBehaviour)));
                        i++;
                    }

                    Items.Behaviour = new ToolStripMenuItem("Default Song Marker Behaviour", null, SongMarkerBehaviourOptions.Values.ToArray());
                    songMarkersSubMenu.DropDownItems.Add(Items.Behaviour);
                }
                optionMenu.DropDownItems.Add(songMarkersSubMenu);

                ToolStripMenuItem wothSubMenu = new ToolStripMenuItem("WotH");
                {
                    Items.EnableLastWoth = new ToolStripMenuItem("Enable Last WotH", null, new EventHandler(menuBar_ToggleEnableLastWotH))
                    {
                        CheckOnClick = true,
                    };
                    wothSubMenu.DropDownItems.Add(Items.EnableLastWoth);


                    LastWothColorOptions = new Dictionary<KnownColor, ToolStripMenuItem>();
                    SpoilerPointColorOptions = new Dictionary<KnownColor, ToolStripMenuItem>();
                    SpoilerWothColorOptions = new Dictionary<KnownColor, ToolStripMenuItem>();
                    SpoilerEmptyColorOptions = new Dictionary<KnownColor, ToolStripMenuItem>();
                    SpoilerKindaEmptyColorOptions = new Dictionary<KnownColor, ToolStripMenuItem>();

                    var firstColorId = 28;
                    var lastColorId = 167;

                    for (int i = firstColorId; i <= lastColorId; i++)
                    {
                        var color = (KnownColor)i;
                        LastWothColorOptions.Add(color, new ToolStripMenuItem(color.ToString(), null, new EventHandler(menuBar_SetLastWothColor)));
                        SpoilerPointColorOptions.Add(color, new ToolStripMenuItem(color.ToString(), null, new EventHandler(menuBar_SetSpoilerPointColor)));
                        SpoilerWothColorOptions.Add(color, new ToolStripMenuItem(color.ToString(), null, new EventHandler(menuBar_SetSpoilerWothColor)));
                        SpoilerEmptyColorOptions.Add(color, new ToolStripMenuItem(color.ToString(), null, new EventHandler(menuBar_SetSpoilerEmptyColor)));
                        SpoilerKindaEmptyColorOptions.Add(color, new ToolStripMenuItem(color.ToString(), null, new EventHandler(menuBar_SetSpoilerKindaEmptyColor)));
                        i++;
                    }

                    Items.LastWothColor = new ToolStripMenuItem("Last WotH Color", null, LastWothColorOptions.Values.ToArray());
                    wothSubMenu.DropDownItems.Add(Items.LastWothColor);

                    Items.EnableDuplicateWoth = new ToolStripMenuItem("Allow Duplicate WotH Entries", null, new EventHandler(menuBar_ToggleEnableDuplicateWotH))
                    {
                        CheckOnClick = true,
                    };
                    wothSubMenu.DropDownItems.Add(Items.EnableDuplicateWoth);
                }
                optionMenu.DropDownItems.Add(wothSubMenu);

                ToolStripMenuItem barrenSubMenu = new ToolStripMenuItem("Barren");
                {
                    Items.EnableBarrenColors = new ToolStripMenuItem("Enable Barren Colors", null, new EventHandler(menuBar_ToggleEnableBarrenColors))
                    {
                        CheckOnClick = true,
                    };
                    barrenSubMenu.DropDownItems.Add(Items.EnableBarrenColors);
                }
                optionMenu.DropDownItems.Add(barrenSubMenu);
                //-------------------
                ToolStripMenuItem gossipSubMenu = new ToolStripMenuItem("Gossip Stones");
                {
                    Items.OverrideHeldImage = new ToolStripMenuItem("Allow Override of Held Image", null, new EventHandler(menuBar_ToggleOverrideHeldImage))
                    {
                        CheckOnClick = true,
                    };
                    gossipSubMenu.DropDownItems.Add(Items.OverrideHeldImage);

                    Items.StoneOverrideCheckMark = new ToolStripMenuItem("Ignore Incoming Checkmarks", null, new EventHandler(menuBar_ToggleOverrideStoneCheckmark))
                    {
                        CheckOnClick = true,
                    };
                    gossipSubMenu.DropDownItems.Add(Items.StoneOverrideCheckMark);

                    GossipeCycleLengthOptions = new Dictionary<double, ToolStripMenuItem>();
                    
                    for (double i = 0.25; i <= 2.0; i+=0.25)
                    {
                        GossipeCycleLengthOptions.Add(i, new ToolStripMenuItem(i.ToString()+" sec", null, new EventHandler(menuBar_SetGossipCycleLength)));
                    }

                    Items.CycleLength = new ToolStripMenuItem("Cycle Delay", null, GossipeCycleLengthOptions.Values.ToArray());
                    gossipSubMenu.DropDownItems.Add(Items.CycleLength);

                    Items.ForceGossipCycles = new ToolStripMenuItem("Force All Stones to Cycle", null, new EventHandler(menuBar_ToggleForceGossipCycles))
                    {
                        CheckOnClick = true,
                    };
                    gossipSubMenu.DropDownItems.Add(Items.ForceGossipCycles);
                }
                optionMenu.DropDownItems.Add(gossipSubMenu);
                //-------------
                ToolStripMenuItem spoilerSubMenu = new ToolStripMenuItem("DK64 Spoiler Hints");
                {

                    // level order
                    SpoilerOrderOptions = new Dictionary<Settings.SpoilerOrderOption, ToolStripMenuItem>();
                    int i = 0;
                    foreach (var button in SpoilerOrderNames)
                    {
                        SpoilerOrderOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetSpoilerLevelOrder)));
                        i++;
                    }
                    Items.SpoilerLevelOrder = new ToolStripMenuItem("Level Order", null, SpoilerOrderOptions.Values.ToArray());
                    spoilerSubMenu.DropDownItems.Add(Items.SpoilerLevelOrder);

                    Items.SpoilerHideStarting = new ToolStripMenuItem("Hide Starting Moves", null, new EventHandler(menuBar_ToggleEnableHideStarting))
                    {
                        CheckOnClick = true,
                    };
                    spoilerSubMenu.DropDownItems.Add(Items.SpoilerHideStarting);

                    Items.CellOverrideCheckMark = new ToolStripMenuItem("Ignore Incoming Checkmarks", null, new EventHandler(menuBar_ToggleOverrideCellCheckmark))
                    {
                        CheckOnClick = true,
                    };
                    spoilerSubMenu.DropDownItems.Add(Items.CellOverrideCheckMark);

                    // point colour
                    Items.SpoilerPointColor = new ToolStripMenuItem("Point Number Color", null, SpoilerPointColorOptions.Values.ToArray());
                    spoilerSubMenu.DropDownItems.Add(Items.SpoilerPointColor);
                    // kinda empty colour
                    Items.SpoilerKindaEmptyColor = new ToolStripMenuItem("\"0 Points Left (w/ Faded)\" Color", null, SpoilerKindaEmptyColorOptions.Values.ToArray());
                    spoilerSubMenu.DropDownItems.Add(Items.SpoilerKindaEmptyColor);
                    // empty colour
                    Items.SpoilerEmptyColor = new ToolStripMenuItem("\"0 Points Left\" Color", null, SpoilerEmptyColorOptions.Values.ToArray());
                    spoilerSubMenu.DropDownItems.Add(Items.SpoilerEmptyColor);
                    // woth number colour
                    Items.SpoilerWothColor = new ToolStripMenuItem("WotH Number Color", null, SpoilerWothColorOptions.Values.ToArray());
                    spoilerSubMenu.DropDownItems.Add(Items.SpoilerWothColor);

                }
                optionMenu.DropDownItems.Add(spoilerSubMenu);
                //--------------------------------
                ToolStripMenuItem AutosaveSubMenu = new ToolStripMenuItem("Autosaves");
                {
                    Items.EnableAutosaves = new ToolStripMenuItem("Enable Autosaving", null, new EventHandler(menuBar_ToggleEnableAutosaves))
                    {
                        CheckOnClick = true,
                    };
                    AutosaveSubMenu.DropDownItems.Add(Items.EnableAutosaves);


                    Items.DeleteOldAutosaves = new ToolStripMenuItem("Automatically Delete Old Autosaves", null, new EventHandler(menuBar_ToggleDeleteOldAutosaves))
                    {
                        CheckOnClick = true,
                    };
                    AutosaveSubMenu.DropDownItems.Add(Items.DeleteOldAutosaves);
                }
                optionMenu.DropDownItems.Add(AutosaveSubMenu);
                //-------------------
            }
            MenuStrip.Items.Add(optionMenu);

            var MemoryMenu = new ToolStripMenuItem("Autotracker");
            {
                SelectEmulatorOptions = new Dictionary<Settings.SelectEmulatorOption, ToolStripMenuItem>();

                int i = 0;
                foreach (var button in SelectEmulatorNames)
                {
                    SelectEmulatorOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetSelectEmulator)));
                    i++;
                }

                Items.SelectEmulator = new ToolStripMenuItem("Select Emulator", null, SelectEmulatorOptions.Values.ToArray());
                MemoryMenu.DropDownItems.Add(Items.SelectEmulator);

                Items.SubtractItem = new ToolStripMenuItem("Subtract from Collectables", null, new EventHandler(menuBar_ToggleSubtractItem))
                {
                    CheckOnClick = true,
                };
                MemoryMenu.DropDownItems.Add(Items.SubtractItem);

                Items.ConnectToEmulator = new ToolStripMenuItem("Connect to Emulator", null, new EventHandler(menuBar_ConnectToEmulator))
                {
                    
                };
                MemoryMenu.DropDownItems.Add(Items.ConnectToEmulator);
            }
            MenuStrip.Items.Add(MemoryMenu);

        }

        public void ReadSettings()
        {
            Items.ShowMenuBar.Checked = Settings.ShowMenuBar;
            Enabled = Settings.ShowMenuBar;
            if (Enabled)
                menuBar_Show();
            else
                menuBar_Hide();

            Items.InvertScrollWheel.Checked = Settings.InvertScrollWheel;
            Items.Wraparound.Checked = Settings.WraparoundDungeonNames;

            Items.IncrementWraparound.Checked = Settings.WraparoundItems;
            DragButtonOptions[Settings.DragButton].Checked = true;
            AutocheckDragButtonOptions[Settings.AutocheckDragButton].Checked = true;
            ExtraButtonOptions[Settings.ExtraActionButton].Checked = true;
            IncrementButtonOptions[Settings.IncrementActionButton].Checked = true;
            DecrementButtonOptions[Settings.DecrementActionButton].Checked = true;
            ResetButtonOptions[Settings.ResetActionButton].Checked = true;

            Items.MoveLocation.Checked = Settings.MoveLocationToSong;
            //Items.Autocheck.Checked = Settings.AutoCheckSongs;
            SongMarkerBehaviourOptions[Settings.SongMarkerBehaviour].Checked = true;

            Items.EnableDuplicateWoth.Checked = Settings.EnableDuplicateWoth;
            Items.EnableLastWoth.Checked = Settings.EnableLastWoth;
            LastWothColorOptions[Settings.LastWothColor].Checked = true;

            Items.EnableBarrenColors.Checked = Settings.EnableBarrenColors;

            Items.OverrideHeldImage.Checked = Settings.OverrideHeldImage;
            Items.CellOverrideCheckMark.Checked = Settings.CellOverrideCheckMark;
            Items.StoneOverrideCheckMark.Checked = Settings.StoneOverrideCheckMark;
            Items.ForceGossipCycles.Checked = Settings.ForceGossipCycles;

            Items.EnableAutosaves.Checked = Settings.EnableAutosave;
            Items.DeleteOldAutosaves.Checked = Settings.DeleteOldAutosaves;
            try
            {
            GossipeCycleLengthOptions[Settings.GossipCycleTime].Checked = true;
            } catch (Exception)
            {

            }

            SpoilerOrderOptions[Settings.SpoilerOrder].Checked = true;
            Items.SpoilerHideStarting.Checked = Settings.HideStarting;
            SpoilerPointColorOptions[Settings.SpoilerPointColour].Checked = true;
            SpoilerWothColorOptions[Settings.SpoilerWOTHColour].Checked = true;
            SpoilerEmptyColorOptions[Settings.SpoilerEmptyColour].Checked = true;
            SpoilerKindaEmptyColorOptions[Settings.SpoilerKindaEmptyColour].Checked = true;

            SelectEmulatorOptions[Settings.SelectEmulator].Checked = true;
            Items.SubtractItem.Checked = Settings.SubtractItems;
        }

        public void SetRenderer()
        {
            MenuStrip.BackColor = Form.BackColor;

            ProfessionalColorTable theme;
            if (MenuStrip.BackColor.GetBrightness() < 0.5)
            {
                MenuStrip.ForeColor = Color.White;
                theme = new Form1_MenuBar_ColorTable_DarkTheme(MenuStrip.BackColor);
            }
            else
            {
                MenuStrip.ForeColor = Color.Black;
                theme = new Form1_MenuBar_ColorTable_LightTheme(MenuStrip.BackColor);
            }

            MenuStrip.Renderer = new ToolStripProfessionalRenderer(theme);
        }

        public void menuBar_OpenLayout(object sender, EventArgs e)
        {
            // open file dialog for jsons
            OpenFileDialog filedia = new OpenFileDialog();
            filedia.Title = "Open GST Layout file";
            filedia.InitialDirectory= Application.StartupPath + "\\Layouts";
            filedia.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            filedia.Multiselect = false;
            // put that filename into settings' ActiveLayout
            if (filedia.ShowDialog() == DialogResult.OK)
            {
                if (filedia.FileName.Contains(Application.StartupPath))
                {
                    Settings.ActiveLayout = filedia.FileName.Substring((Application.StartupPath.Length + 1));
                } else
                {
                    Settings.ActiveLayout = filedia.FileName.ToString();
                }
                Settings.Write();
                Form.Reset(sender);
            }
            
        }

        public void menuBar_OpenPlaces(object sender, EventArgs e)
        {
            // open file dialog for jsons
            OpenFileDialog filedia = new OpenFileDialog();
            filedia.Title = "Open GST Places file";
            filedia.InitialDirectory = Application.StartupPath;
            filedia.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            filedia.Multiselect = false;
            // put that filename into settings' ActivePlaces
            if (filedia.ShowDialog() == DialogResult.OK)
            {
                if (filedia.FileName.Contains(filedia.InitialDirectory))
                {
                    Settings.ActivePlaces = filedia.FileName.Substring((filedia.InitialDirectory.Length + 1));
                }
                else
                {
                    Settings.ActivePlaces = filedia.FileName.ToString();
                }
                Settings.Write();
                Form.LoadSettings();
            }
        }

        public void menuBar_OpenSpoilerHint(object sender, EventArgs e)
        {
            // open file dialog for jsons
            OpenFileDialog filedia = new OpenFileDialog();
            filedia.Title = "Open GDK64 Spoiler log";
            filedia.InitialDirectory = Application.StartupPath;
            filedia.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            filedia.Multiselect = false;
            // put that filename into settings' ActivePlaces
            if (filedia.ShowDialog() == DialogResult.OK)
            {
                foreach (Control thing in Form.Controls[0].Controls)
                {
                    if (thing is SpoilerPanel panel)
                    {
                        panel.ImportFromJson(filedia.FileName);
                        return;
                    }
                }
                MessageBox.Show("Could not find Spoiler Hint object on the current layout.\nMake sure the layout you are using supports Spoiler Hints before opening a spoiler log.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void menuBar_SaveState(object sender, EventArgs e)
        {
            Form.SaveState();
        }

        public void menuBar_LoadState(object sender, EventArgs e)
        {
            Form.LoadState();
        }

        public void menuBar_Reset(object sender, EventArgs e)
        {
            Form.Reset(sender);
        }

        public void menuBar_Enable(object sender, EventArgs e)
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
            Items.ShowMenuBar.Checked = Enabled;
            Settings.Write();
        }

        public void menuBar_Broadcast(object sender, EventArgs e)
        {
            if (Form.CurrentLayout.App_Settings.EnableBroadcast && Form.CurrentLayout.App_Settings.BroadcastFile != null)
            {
                if (Items.BroadcastView.Checked)
                {
                    Form2 f2 = new Form2();
                    f2.Show();
                    Form.UpdateAll();
                }
                else if (Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    Application.OpenForms["GSTHD_DK64 Broadcast View"].ClearAndDispose();
                    Items.BroadcastView.Checked = false;
                }
            } 
            else
            {
                Items.BroadcastView.Checked = false;
            }
            
            
        }

        public void menuBar_toggleBroadcast()
        {
            if (Items.BroadcastView.Checked) {
               Items.BroadcastView.Checked = false;
             } else { Items.BroadcastView.Checked = true;}
        }

        public void menuBar_ZoomIn(object sender, EventArgs e)
        {
            Form.ZoomIn();
        }

        public void menuBar_ZoomOut(object sender, EventArgs e)
        {
            Form.ZoomOut();
        }

        public void menuBar_ZoomReset(object sender, EventArgs e)
        {
            Form.ZoomReset();
        }

        public void menuBar_Show()
        {
            Size = SavedSize;
            Form.Size = new Size(Form.Size.Width, Form.Size.Height + Size.Height);
            Form.Refresh();
        }

        public void menuBar_Hide()
        {
            Form.Size = new Size(Form.Size.Width, Form.Size.Height - Size.Height);
            Size = new Size(0, 0);
            Form.Refresh();
        }

        private void menuBar_ToggleInvertScrollWheel(object sender, EventArgs e)
        {
            // Items.InvertScrollWheel.Checked = !Items.InvertScrollWheel.Checked;
            Settings.InvertScrollWheel = Items.InvertScrollWheel.Checked;
            Settings.Write();
        }

        private void menuBar_ToggleWraparound(object sender, EventArgs e)
        {
            // Items.Wraparound.Checked = !Items.Wraparound.Checked;
            Settings.WraparoundDungeonNames = Items.Wraparound.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetDragButton(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            DragButtonOptions[Settings.DragButton].Checked = false;
            choice.Checked = true;

            var option = DragButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.DragButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetAutocheckDragButton(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            AutocheckDragButtonOptions[Settings.AutocheckDragButton].Checked = false;
            choice.Checked = true;

            var option = AutocheckDragButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.AutocheckDragButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetIncrementButton(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            IncrementButtonOptions[Settings.IncrementActionButton].Checked = false;
            choice.Checked = true;

            var option = IncrementButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.IncrementActionButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetDecrementButton(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            DecrementButtonOptions[Settings.DecrementActionButton].Checked = false;
            choice.Checked = true;

            var option = DecrementButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.DecrementActionButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetResetButton(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            ResetButtonOptions[Settings.ResetActionButton].Checked = false;
            choice.Checked = true;

            var option = ResetButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.ResetActionButton = option.Key;
            Settings.Write();
        }


        private void menuBar_SetExtraButton(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            ExtraButtonOptions[Settings.ExtraActionButton].Checked = false;
            choice.Checked = true;

            var option = ExtraButtonOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.ExtraActionButton = option.Key;
            Settings.Write();
        }

        private void menuBar_SetSelectEmulator(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SelectEmulatorOptions[Settings.SelectEmulator].Checked = false;
            choice.Checked = true;

            var option = SelectEmulatorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SelectEmulator = option.Key;
            Settings.Write();
        }

        private void menuBar_SetSpoilerLevelOrder(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SpoilerOrderOptions[Settings.SpoilerOrder].Checked = false;
            choice.Checked = true;

            var option = SpoilerOrderOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerOrder = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        //private void menuBar_ToggleAutocheckSongs(object sender, EventArgs e)
        //{
        //    Items.Autocheck.Checked = !Items.Autocheck.Checked;
        //    Settings.AutoCheckSongs = Items.Autocheck.Checked;
        //    Settings.Write();
        //}

        private void menuBar_ToggleMoveLocation(object sender, EventArgs e)
        {
            // Items.MoveLocation.Checked = !Items.MoveLocation.Checked;
            Settings.MoveLocationToSong = Items.MoveLocation.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSongMarkerBehaviour(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SongMarkerBehaviourOptions[Settings.SongMarkerBehaviour].Checked = false;
            choice.Checked = true;

            var option = SongMarkerBehaviourOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SongMarkerBehaviour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableDuplicateWotH(object sender, EventArgs e)
        {
            // Items.EnableLastWoth.Enabled = !Items.EnableLastWoth.Enabled;
            Settings.EnableDuplicateWoth = Items.EnableDuplicateWoth.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableLastWotH(object sender, EventArgs e)
        {
            // Items.EnableLastWoth.Enabled = !Items.EnableLastWoth.Enabled;
            Settings.EnableLastWoth = Items.EnableLastWoth.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableBarrenColors(object sender, EventArgs e)
        {
            // Items.EnableBarrenColors.Enabled = !Items.EnableBarrenColors.Enabled;
            Settings.EnableBarrenColors = Items.EnableBarrenColors.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableHideStarting(object sender, EventArgs e)
        {
            Settings.HideStarting = Items.SpoilerHideStarting.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleForceGossipCycles(object sender, EventArgs e)
        {
            Settings.ForceGossipCycles = Items.ForceGossipCycles.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideHeldImage(object sender, EventArgs e)
        {
            Settings.OverrideHeldImage = Items.OverrideHeldImage.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideStoneCheckmark(object sender, EventArgs e)
        {
            Settings.StoneOverrideCheckMark = Items.StoneOverrideCheckMark.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleOverrideCellCheckmark(object sender, EventArgs e)
        {
            Settings.CellOverrideCheckMark = Items.CellOverrideCheckMark.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }
        
        private void menuBar_ToggleIncrementWraparound(object sender, EventArgs e)
        {
            Settings.WraparoundItems = Items.IncrementWraparound.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleSubtractItem(object sender, EventArgs e)
        {
            Settings.SubtractItems = Items.SubtractItem.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleEnableAutosaves(object sender, EventArgs e)
        {
            Settings.EnableAutosave = Items.EnableAutosaves.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_ToggleDeleteOldAutosaves(object sender, EventArgs e)
        {
            Settings.DeleteOldAutosaves = Items.DeleteOldAutosaves.Checked;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetGossipCycleLength(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            try
            {
                GossipeCycleLengthOptions[Settings.GossipCycleTime].Checked = false;
            } catch (Exception)
            {

            }
            choice.Checked = true;

            var option = GossipeCycleLengthOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.GossipCycleTime = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetLastWothColor(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            LastWothColorOptions[Settings.LastWothColor].Checked = false;
            choice.Checked = true;

            var option = LastWothColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.LastWothColor = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerPointColor(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SpoilerPointColorOptions[Settings.SpoilerPointColour].Checked = false;
            choice.Checked = true;

            var option = SpoilerPointColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerPointColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerWothColor(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SpoilerWothColorOptions[Settings.SpoilerWOTHColour].Checked = false;
            choice.Checked = true;

            var option = SpoilerWothColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerWOTHColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerEmptyColor(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SpoilerEmptyColorOptions[Settings.SpoilerEmptyColour].Checked = false;
            choice.Checked = true;

            var option = SpoilerEmptyColorOptions.FirstOrDefault((x) => x.Value == choice);
            if (option.Value == null) throw new NotImplementedException();
            Settings.SpoilerEmptyColour = option.Key;
            Settings.Write();
            Form.UpdateLayoutFromSettings();
        }

        private void menuBar_SetSpoilerKindaEmptyColor(object sender, EventArgs e)
        {
            var choice = (ToolStripMenuItem)sender;

            SpoilerKindaEmptyColorOptions[Settings.SpoilerKindaEmptyColour].Checked = false;
            choice.Checked = true;

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
                // connect to emulator as speficied through the other setting
                switch (Settings.SelectEmulator.ToString())
                {
                    case "Project64":
                        var resultPJ = AttachToEmulators.attachToProject64(Form);
                        if (resultPJ != null)
                        {
                            if (resultPJ.Item1 != null)
                            {
                                Form.SetAutotracker(resultPJ.Item1, resultPJ.Item2);
                                MessageBox.Show("Connection to PJ64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Could not connect to PJ64\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case "Bizhawk":
                        var resultBH = AttachToEmulators.attachToBizhawk(Form);
                        if (resultBH != null)
                        {
                            if (resultBH.Item1 != null)
                            {
                                Form.SetAutotracker(resultBH.Item1, resultBH.Item2);
                                MessageBox.Show("Connection to Bizhawk-DK64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Could not connect to Bizhawk-DK64\nMake sure the game you want to track is loaded in the emulator before connecting.\nIf you are experiencing persistent issues, try switching to the _32 exe of GSTHD instead.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case "RMG":
                        var resultRMG = AttachToEmulators.attachToRMG(Form);
                        if (resultRMG != null)
                        {
                            if (resultRMG.Item1 != null)
                            {
                                Form.SetAutotracker(resultRMG.Item1, resultRMG.Item2);
                                MessageBox.Show("Connection to RMG sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Could not connect to RMG\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    case "simple64":
                        var results64 = AttachToEmulators.attachToSimple64(Form);
                        if (results64 != null)
                        {
                            if (results64.Item1 != null)
                            {
                                Form.SetAutotracker(results64.Item1, results64.Item2);
                                MessageBox.Show("Connection to simple64 sucessful\nTracking will begin once you enter the main game mode (not the title screen or main menu)");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Could not connect to simple64\nMake sure the game you want to track is loaded in the emulator before connecting.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        break;
                    default:
                        MessageBox.Show("No supported emulator selected.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;

                }
            } else
            {
                MessageBox.Show("Current layout does not support autotracking.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            

        }

        private class Form1_MenuBar_ColorTable_LightTheme : ProfessionalColorTable
        {
            private Color bgColor;

            public Form1_MenuBar_ColorTable_LightTheme(Color bgColor)
            {
                this.bgColor = bgColor;
            }
            public override Color MenuItemSelectedGradientBegin
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R - 50, bgColor.G - 50, bgColor.B - 50); }
            }
            public override Color MenuItemSelectedGradientEnd
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R - 50, bgColor.G - 50, bgColor.B - 50); }
            }

            public override Color MenuItemBorder
            {
                get { return Color.DimGray; }
            }
        }

        private class Form1_MenuBar_ColorTable_DarkTheme : ProfessionalColorTable
        {
            private Color bgColor;

            public Form1_MenuBar_ColorTable_DarkTheme(Color bgColor)
            {
                this.bgColor = bgColor;
            }

            public override Color MenuBorder
            {
                get { return Color.LightGray; }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R + 50, bgColor.G + 50, bgColor.B + 50); }
            }
            public override Color MenuItemSelectedGradientEnd
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R + 50, bgColor.G + 50, bgColor.B + 50); }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R + 50, bgColor.G + 50, bgColor.B + 50); }
            }
            public override Color MenuItemPressedGradientEnd
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R + 50, bgColor.G + 50, bgColor.B + 50); }
            }

            public override Color MenuStripGradientBegin
            {
                get { return Color.FromArgb(bgColor.A, bgColor.R + 50, bgColor.G + 50, bgColor.B + 200); }
            }

            public override Color MenuItemBorder
            {
                get { return Color.LightGray; }
            }
        }
    }
}
