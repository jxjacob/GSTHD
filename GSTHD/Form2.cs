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
    public partial class Form2 : Form
    {
        Dictionary<string, string> ListPlacesWithTag = new Dictionary<string, string>();
        SortedSet<string> ListPlaces = new SortedSet<string>();
        SortedSet<string> ListSometimesHintsSuggestions = new SortedSet<string>();

        Layout CurrentLayout;
        public Panel LayoutContent;

        PictureBox pbox_collectedSkulls;

        Settings Settings;
        
        public Form2()
        {
            InitializeComponent();
        }


        private void Form2_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Control && e.KeyCode == Keys.R)
            //{
            //    this.Controls.Clear();
            //    e.Handled = true;
            //    e.SuppressKeyPress = true;
            //    this.Form2_Load(sender, new EventArgs());
            //}

            /*
            if(e.KeyCode == Keys.F2)
            {
                var window = new Editor(CurrentLayout);
                window.Show();
            //}
            */
        }

        //private void Form2_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    Form1_MenuBar
        //}

        private void LoadAll(object sender, EventArgs e)
        {
            var assembly = Assembly.GetEntryAssembly().GetName();
            this.Text = "GSTHD_DK64 Broadcast View";
            this.Name = "GSTHD_DK64 Broadcast View";
            this.AcceptButton = null;
            this.MaximizeBox = false;

            LoadSettings();

            LoadLayout();

            this.KeyPreview = true;
            //this.KeyDown += changeCollectedSkulls;
        }

        public void Reload()
        {
            LoadSettings();
            LoadLayout();
        }

        private void LoadSettings()
        {
            ListPlaces.Clear();
            ListPlaces.Add("");
            ListPlacesWithTag.Clear();
            JObject json_places = JObject.Parse(File.ReadAllText(@"oot_places.json"));
            foreach (var property in json_places)
            {
                ListPlaces.Add(property.Key.ToString());
                ListPlacesWithTag.Add(property.Key, property.Value.ToString());
            }

            ListSometimesHintsSuggestions.Clear();
            JObject json_hints = JObject.Parse(File.ReadAllText(@"sometimes_hints.json"));
            foreach (var categorie in json_hints)
            {
                foreach (var hint in categorie.Value)
                {
                    ListSometimesHintsSuggestions.Add(hint.ToString());
                }
            }

            Settings = Settings.Read();
        }


        private void LoadLayout()
        {
            Controls.Clear();
            LayoutContent = new Panel();
            CurrentLayout = new Layout();
            CurrentLayout.LoadBroadcastLayout(LayoutContent, Settings, ListSometimesHintsSuggestions, ListPlacesWithTag, this);
            Size = new Size(LayoutContent.Size.Width, LayoutContent.Size.Height);
            LayoutContent.Dock = DockStyle.Top;
            Controls.Add(LayoutContent);
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
    }
}
