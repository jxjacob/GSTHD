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

        PictureBox pbox_collectedSkulls;

        Settings Settings;
        
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
            this.Text = $"{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title} v{assembly.Version.Major}.{assembly.Version.Minor}";
            this.AcceptButton = null;
            this.MaximizeBox = false;

            LoadSettings();
            
            MenuBar = new Form1_MenuBar(this, Settings);

            LoadLayout();
            SetMenuBar();

            this.KeyPreview = true;
            //this.KeyDown += changeCollectedSkulls;
        }

        private void Reload()
        {
            LoadSettings();
            LoadLayout();
            SetMenuBar();
            if (this.CurrentLayout.App_Settings.EnableBroadcast && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null) {
                ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).Reload();
            } else if (!this.CurrentLayout.App_Settings.EnableBroadcast && Application.OpenForms["GSTHD_DK64 Broadcast View"] != null) {
                ((Form2)Application.OpenForms["GSTHD_DK64 Broadcast View"]).Close();
            }
        }

        public void LoadSettings()
        {
            Settings = Settings.Read();

            ListPlaces.Clear();
            ListPlaces.Add("");
            ListPlacesWithTag.Clear();
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

        private void LoadLayout()
        {
            Controls.Clear();
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
            Reload();
            Process.GetCurrentProcess().Refresh();
        }

        public void SaveState()
        {
            JObject thejson = new JObject();

            foreach (Item x in this.Controls[0].Controls.OfType<Item>())
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
                                thestring += "~";
                            }
                            thestring += y.ToString();
                        }
                        thejson.Add(x.Name, thestring);
                    } else if (x.ListWotH.Count > 0)
                    {
                        string thestring = "";
                        foreach (WotHState y in x.GetWotHs())
                        {
                            if (thestring.Length > 0)
                            {
                                thestring += "~";
                            }
                            thestring += y.ToString();
                        }
                        thejson.Add(x.Name, thestring);
                    }
                }
            }



            //open file to write to
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "json files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save state to JSON file";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, thejson.ToString());
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
                //all of the fucking things
                JObject loadedjson = JObject.Parse(File.ReadAllText(filedia.FileName));
                foreach (JProperty x in (JToken)loadedjson)
                {
                    Control found = this.Controls.Find(x.Name, true)[0];
                    if (found is Item)
                    {
                        int conv = (int)x.Value;
                        ((Item)found).SetState(conv);
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
                        GossipStoneState newstate = new GossipStoneState()
                        {
                            HoldsImage = Boolean.Parse(words[0]),
                            HeldImageName = words[1],
                            ImageIndex = int.Parse(words[2]),
                        };
                        ((GossipStone)found).SetState(newstate);
                    }
                    else if (found is PanelWothBarren) 
                    {
                        if (((PanelWothBarren)found).isWotH){
                            ((PanelWothBarren)found).SetWotH((string)x.Value);
                        } else
                        {
                            ((PanelWothBarren)found).SetBarren((string)x.Value);
                        }


                        
                    }
                }
            }
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
            }
        }
    }
}
