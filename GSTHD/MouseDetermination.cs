using Avalonia.Input;

namespace GSTHD
{
    public static class MouseDetermination
    {

        public static bool DetermineBasicMouseInput(PointerEventArgs e, Settings.BasicActionButtonOption ba)
        {
            return e.Properties.IsLeftButtonPressed && ba == Settings.BasicActionButtonOption.Left ||
                   e.Properties.IsMiddleButtonPressed && ba == Settings.BasicActionButtonOption.Middle ||
                   e.Properties.IsRightButtonPressed && ba == Settings.BasicActionButtonOption.Right ||
                   e.Properties.IsXButton1Pressed && ba == Settings.BasicActionButtonOption.MouseButton1 ||
                   e.Properties.IsXButton2Pressed && ba == Settings.BasicActionButtonOption.MouseButton2 ||
                   e.Properties.IsLeftButtonPressed && ba == Settings.BasicActionButtonOption.Control && e.KeyModifiers == KeyModifiers.Control ||
                   e.Properties.IsLeftButtonPressed && ba == Settings.BasicActionButtonOption.Shift && e.KeyModifiers == KeyModifiers.Shift  ||
                   e.Properties.IsLeftButtonPressed && ba == Settings.BasicActionButtonOption.Alt && e.KeyModifiers == KeyModifiers.Alt ;
        }
    }
}
