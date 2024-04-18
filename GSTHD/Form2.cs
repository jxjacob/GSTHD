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
    public partial class Form2 : Form, GSTForms
    {
        Dictionary<string, string> ListPlacesWithTag = new Dictionary<string, string>();
        Dictionary<string, string> ListKeycodesWithTag = new Dictionary<string, string>();
        SortedSet<string> ListSometimesHintsSuggestions = new SortedSet<string>();

        public Layout CurrentLayout { get; set; }
        public Panel LayoutContent { get; set; }

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

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
        // call back to form1 and tell its menubar to toggle back
        if (Application.OpenForms[0] is Form1)
            {
                Form1 mainwindow = Application.OpenForms[0] as Form1;
                mainwindow.ToggleMenuBroadcast();
            }
        }


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
            this.FormClosing += new FormClosingEventHandler(Form2_FormClosing);
            //this.KeyDown += changeCollectedSkulls;
        }

        public void Reload()
        {
            //LoadSettings();
            LoadLayout();
        }

        public void LoadSettings()
        {
            Settings = Settings.Read();

            
            ListPlacesWithTag.Clear();
            ListKeycodesWithTag.Clear();

        }

        //public void ForceLoadSettings(Settings s)
        //{
        //    // only from main view, cuz the broadcast isnt made aware of changes from form1's menu bar
        //    Settings = Settings.Read();
        //    Debug.WriteLine($"start s {s.SpoilerEmptyColour}");
        //    Debug.WriteLine($"start settings {Settings.SpoilerEmptyColour}");
        //}


        private void LoadLayout()
        {
            Controls.Clear();
            if (LayoutContent != null) LayoutContent.Dispose();
            LayoutContent = new Panel();
            CurrentLayout = new Layout();
            CurrentLayout.LoadLayout(LayoutContent, Settings, ListSometimesHintsSuggestions, ListPlacesWithTag, ListKeycodesWithTag, this);
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
