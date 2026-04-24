using Avalonia.Input;

namespace GSTHD
{
    public interface ProgressibleElement<T>
    {
        void IncrementState();
        void DecrementState();
        void ResetState();
        void ToggleCheck();
    }

    public class ProgressibleElementBehaviour<T>
    {
        protected ProgressibleElement<T> Element;
        protected Settings Settings;

        public ProgressibleElementBehaviour(ProgressibleElement<T> element, Settings settings)
        {
            Element = element;
            Settings = settings;
        }

        public bool DetermineMouseInput(PointerPressedEventArgs e, Settings.ExtraActionModButton ea)
        {
            // might move this to the MouseDetermination struct, depending on how i go about the Control clicking
            return e.Properties.IsLeftButtonPressed && ea == Settings.ExtraActionModButton.Left ||
                   e.Properties.IsMiddleButtonPressed && ea == Settings.ExtraActionModButton.Middle ||
                   e.Properties.IsRightButtonPressed && ea == Settings.ExtraActionModButton.Right ||
                   e.Properties.IsLeftButtonPressed && ea == Settings.ExtraActionModButton.DoubleLeft && e.ClickCount > 1 ||
                   e.Properties.IsXButton1Pressed && ea == Settings.ExtraActionModButton.MouseButton1 ||
                   e.Properties.IsXButton2Pressed && ea == Settings.ExtraActionModButton.MouseButton2 ||
                   e.Properties.IsLeftButtonPressed && ea == Settings.ExtraActionModButton.Control && e.KeyModifiers == KeyModifiers.Control ||
                   e.Properties.IsLeftButtonPressed && ea == Settings.ExtraActionModButton.Shift && e.KeyModifiers == KeyModifiers.Shift ||
                   e.Properties.IsLeftButtonPressed && ea == Settings.ExtraActionModButton.Alt && e.KeyModifiers == KeyModifiers.Alt;
        }


        public void Mouse_ClickDown(object? sender, PointerPressedEventArgs e)
        {
            // due to DoubleClick being an option, the Checkmark Item action must be checked right
            if (DetermineMouseInput(e, ea:Settings.ExtraActionButton)) Element.ToggleCheck();
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton)) Mouse_LeftClickDown(sender, e);
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton)) Mouse_RightClickDown(sender, e);
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton)) Mouse_MiddleClickDown(sender, e);


        }

        public void Mouse_LeftClickDown(object? sender, PointerPressedEventArgs e)
        {
            Element.IncrementState();
        }

        public void Mouse_MiddleClickDown(object? sender, PointerPressedEventArgs e)
        {
            Element.ResetState();
        }

        public void Mouse_RightClickDown(object? sender, PointerPressedEventArgs e)
        {
            Element.DecrementState();
        }
    }
}
