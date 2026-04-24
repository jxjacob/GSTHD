using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace GSTHD;

public partial class BroadcastWindow : Window, GSTForms
{
    Dictionary<string, string> ListPlacesWithTag = new();
    Dictionary<string, string> ListKeycodesWithTag = new();
    SortedSet<string> ListSometimesHintsSuggestions = [];

    public Layout CurrentLayout { get; set; }
    public Canvas LayoutContent { get; set; }
    
    public Color BackColor { get; set; }

    Settings Settings;
    
    public BroadcastWindow()
    {
        InitializeComponent();
    }
    

    private void Form2_FormClosing(object? sender, WindowClosingEventArgs? e)
    {
        // call back to MainWindow and tell its menubar to toggle back
        MainWindow mainwindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime)?.MainWindow as MainWindow;
        mainwindow?.ToggleMenuBroadcast();
    }
    
    private void LoadAll(object sender, RoutedEventArgs e)
    {
        var assembly = Assembly.GetExecutingAssembly().GetName();
        this.Title = "GSTHD Broadcast View";
        CanMaximize = false;
        LayoutContent = Layout;

        LoadSettings();

        LoadLayout();

        this.Closing += Form2_FormClosing;
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
    //    // only from main view, cuz the broadcast isnt made aware of changes from MainWindow's menu bar
    //    Settings = Settings.Read();
    //    Debug.WriteLine($"start s {s.SpoilerEmptyColour}");
    //    Debug.WriteLine($"start settings {Settings.SpoilerEmptyColour}");
    //}

    public void SetLayoutColor(Color color)
    {
        BackColor = color;
        Layout.Background = new SolidColorBrush(color);
    }

    private void LoadLayout()
    {
        Layout.Children.Clear();
        CurrentLayout = new Layout();
        CurrentLayout.LoadLayout(Layout, Settings, ListSometimesHintsSuggestions, ListPlacesWithTag, ListKeycodesWithTag, this);
        Width = Layout.Width;
        Height = Layout.Height;
    }

    public void UpdateLayoutFromSettings()
    {
        CurrentLayout.UpdateFromSettings();
    }
    

    public void Reset(object sender)
    {
        Layout.Children.Clear();
        Reload();
        Process.GetCurrentProcess().Refresh();
    }
}

