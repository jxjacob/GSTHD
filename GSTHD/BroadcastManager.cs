using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace GSTHD;

public abstract class BroadcastManager
{
    public static BroadcastWindow? Window;

    public static void SetBroadcast()
    {
        Window = (BroadcastWindow)Dispatcher.UIThread.Invoke(() =>
            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Windows
            .FirstOrDefault(x => x is BroadcastWindow));
    }

    public static void RemoveBroadcast()
    {
        Window = null;
    }
    
    public static BroadcastWindow? GetBroadcast()
    {
        return Window;
    }

    public static Control? GetControl(string? searchedName)
    {
        return Window?.LayoutContent.Children.FirstOrDefault(x => string.Equals(x.Name, searchedName, StringComparison.CurrentCultureIgnoreCase));
    }

    public static IEnumerable<Control>? GetControls(string? searchedName)
    {
        return Window?.LayoutContent.Children.Where(x => string.Equals(x.Name, searchedName, StringComparison.CurrentCultureIgnoreCase));
    }
}