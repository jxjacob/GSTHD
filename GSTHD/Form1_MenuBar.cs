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

namespace GSTHD
{
    public class Form1_MenuBar : Panel
    {
        private struct MenuItems
        {
            // Layout
            public ToolStripMenuItem OpenLayout;
            public ToolStripMenuItem OpenPlaces;
            public ToolStripMenuItem Reset;
            public ToolStripMenuItem SaveState;
            public ToolStripMenuItem LoadState;
            // public ToolStripMenuItem LoadLayout;
            public ToolStripMenuItem ShowMenuBar;
            public ToolStripMenuItem BroadcastView;

            // Options
            // Scroll Wheel
            public ToolStripMenuItem InvertScrollWheel;
            public ToolStripMenuItem Wraparound;

            // Drag & Drop
            public ToolStripMenuItem DragButton;
            public ToolStripMenuItem AutocheckDragButton;

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

            // Memory Engine
            public ToolStripMenuItem SelectEmulator;
            public ToolStripMenuItem ConnectToEmulator;
            public ToolStripMenuItem VerifyConnection;
        }

        private readonly Dictionary<Settings.DragButtonOption, string> DragButtonNames = new Dictionary<Settings.DragButtonOption, string>
        {
            { Settings.DragButtonOption.None, "None" },
            { Settings.DragButtonOption.Left, "Left Click" },
            { Settings.DragButtonOption.Middle, "Middle Click" },
            { Settings.DragButtonOption.Right, "Right Click" },
            { Settings.DragButtonOption.LeftAndRight, "Left + Right Click" },
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
            { Settings.SelectEmulatorOption.Bizhawk, "Bizhawk (NOT WORKING)" },
        };

        Form1 Form;
        Settings Settings;
        MenuStrip MenuStrip;
        MenuItems Items;
        Dictionary<Settings.DragButtonOption, ToolStripMenuItem> DragButtonOptions;
        Dictionary<Settings.DragButtonOption, ToolStripMenuItem> AutocheckDragButtonOptions;
        Dictionary<Settings.SongMarkerBehaviourOption, ToolStripMenuItem> SongMarkerBehaviourOptions;
        Dictionary<Settings.SelectEmulatorOption, ToolStripMenuItem> SelectEmulatorOptions;
        Dictionary<KnownColor, ToolStripMenuItem> LastWothColorOptions;
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

                //Items.LoadLayout = new ToolStripMenuItem("Load Layout", null, new EventHandler(menuBar_LoadLayout));
                //layoutMenu.DropDownItems.Add(Items.LoadLayout);

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

            }
            MenuStrip.Items.Add(layoutMenu);

            var optionMenu = new ToolStripMenuItem("Options");
            {
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
                optionMenu.DropDownItems.Add(scrollWheelSubMenu);

                var dragDropSubMenu = new ToolStripMenuItem("Drag && Drop");
                {
                    DragButtonOptions = new Dictionary<Settings.DragButtonOption, ToolStripMenuItem>();
                    AutocheckDragButtonOptions = new Dictionary<Settings.DragButtonOption, ToolStripMenuItem>();

                    int i = 0;
                    foreach (var button in DragButtonNames)
                    {
                        DragButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetDragButton)));
                        AutocheckDragButtonOptions.Add(button.Key, new ToolStripMenuItem(button.Value, null, new EventHandler(menuBar_SetAutocheckDragButton)));
                        i++;
                    }
                    
                    Items.DragButton = new ToolStripMenuItem("Drag Button", null, DragButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.DragButton);

                    Items.AutocheckDragButton = new ToolStripMenuItem("Autocheck Drag Button", null, AutocheckDragButtonOptions.Values.ToArray());
                    dragDropSubMenu.DropDownItems.Add(Items.AutocheckDragButton);
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

                    var firstColorId = 28;
                    var lastColorId = 167;

                    for (int i = firstColorId; i <= lastColorId; i++)
                    {
                        var color = (KnownColor)i;
                        LastWothColorOptions.Add(color, new ToolStripMenuItem(color.ToString(), null, new EventHandler(menuBar_SetLastWothColor)));
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
            }
            MenuStrip.Items.Add(optionMenu);


            //TODO: make it so that this menu doesnt appear if the layout doesnt actually support autotracking
            var MemoryMenu = new ToolStripMenuItem("Memory Engine");
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

                Items.ConnectToEmulator = new ToolStripMenuItem("Connect to Emulator", null, new EventHandler(menuBar_ConnectToEmulator))
                {
                    
                };
                MemoryMenu.DropDownItems.Add(Items.ConnectToEmulator);

                Items.VerifyConnection = new ToolStripMenuItem("Verify Connect (DEBUG)", null, new EventHandler(menuBar_ConnectToEmulator))
                {

                };
                MemoryMenu.DropDownItems.Add(Items.VerifyConnection);
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

            DragButtonOptions[Settings.DragButton].Checked = true;
            AutocheckDragButtonOptions[Settings.AutocheckDragButton].Checked = true;

            Items.MoveLocation.Checked = Settings.MoveLocationToSong;
            //Items.Autocheck.Checked = Settings.AutoCheckSongs;
            SongMarkerBehaviourOptions[Settings.SongMarkerBehaviour].Checked = true;

            Items.EnableDuplicateWoth.Checked = Settings.EnableDuplicateWoth;
            Items.EnableLastWoth.Checked = Settings.EnableLastWoth;
            LastWothColorOptions[Settings.LastWothColor].Checked = true;

            Items.EnableBarrenColors.Checked = Settings.EnableBarrenColors;
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
            filedia.InitialDirectory= Application.StartupPath;
            filedia.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            filedia.Multiselect = false;
            // put that filename into settings' ActiveLayout
            if (filedia.ShowDialog() == DialogResult.OK)
            {
                if (filedia.FileName.Contains(filedia.InitialDirectory))
                {
                    Settings.ActiveLayout = filedia.FileName.Substring((filedia.InitialDirectory.Length + 1));
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
            if (Form.CurrentLayout.App_Settings.EnableBroadcast)
            {
                if (Items.BroadcastView.Checked)
                {
                    Form2 f2 = new Form2();
                    f2.Show();
                    Form.UpdateAll();
                }
                else if (Application.OpenForms["GSTHD_DK64 Broadcast View"] != null)
                {
                    Application.OpenForms["GSTHD_DK64 Broadcast View"].Close();
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

        //private void menuBar_LoadLayout(object sender, EventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

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

        public void menuBar_ConnectToEmulator(object sender, EventArgs e)
        {
            // connect to emulator as speficied through the other setting
            var result = AttachToEmulators.attachToProject64();
            //Debug.WriteLine("diddy moves? " + Memory.ReadInt32(theprogram, 0xDFE40000 + 0x759260));
            if (result != null)
            {
                if (result.Item1 != null)
                {
                    Form.SetAutotracker(result.Item1, result.Item2);
                    MessageBox.Show("Connection to PJ64 sucessful");
                }
            }
            //uint offset = 0xDFE40000;
            //Debug.WriteLine("diddy moves: "+ Memory.ReadInt8(theprogram, offset + 0x7FC9AD));
            //Debug.WriteLine("diddy slam: " + Memory.ReadInt8(theprogram, offset + 0x7FC9AC));
            //for (uint i = 0; i < 128; i++)
            //{
            //    Debug.WriteLine("checking at " + i + ":" + Memory.ReadInt8(theprogram, offset + 0x7FC9A0 + i));
            //}

        }

        public void menuBar_VerifyConnection(object sender, EventArgs e)
        {
            // spit out so much info to make sure shit does infact connect
            // will be removed eventually

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
