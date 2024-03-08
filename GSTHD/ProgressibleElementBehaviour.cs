using System;
using System.Diagnostics;
using System.Windows.Forms;

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

        public bool DetermineMouseInput(MouseEventArgs e, Settings.ExtraActionModButton ea)
        {
            // might move this to the MouseDetermination struct, depending on how i go about the Control clicking
            if (e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.Left ||
                e.Button == MouseButtons.Middle && ea == Settings.ExtraActionModButton.Middle ||
                e.Button == MouseButtons.Right && ea == Settings.ExtraActionModButton.Right ||
                e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.DoubleLeft && e.Clicks > 1 ||
                e.Button == MouseButtons.XButton1 && ea == Settings.ExtraActionModButton.MouseButton1 ||
                e.Button == MouseButtons.XButton2 && ea == Settings.ExtraActionModButton.MouseButton2 ||
                e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.Control && Form.ModifierKeys == Keys.Control ||
                e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.Shift && Form.ModifierKeys == Keys.Shift ||
                e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.Alt && Form.ModifierKeys == Keys.Alt)
            {
                return true;
            } else { return false; }
        }


        public void Mouse_ClickDown(object sender, MouseEventArgs e)
        {
            // due to DoubleClick being an option, the Checkmark Item action must be checked right
            if (DetermineMouseInput(e, ea:Settings.ExtraActionButton)) Element.ToggleCheck();
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.IncrementActionButton)) Mouse_LeftClickDown(sender, e);
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.DecrementActionButton)) Mouse_RightClickDown(sender, e);
            else if (MouseDetermination.DetermineBasicMouseInput(e, Settings.ResetActionButton)) Mouse_MiddleClickDown(sender, e);


        }

        public void Mouse_LeftClickDown(object sender, MouseEventArgs e)
        {
            Element.IncrementState();
        }

        public void Mouse_MiddleClickDown(object sender, MouseEventArgs e)
        {
            Element.ResetState();
        }

        public void Mouse_RightClickDown(object sender, MouseEventArgs e)
        {
            Element.DecrementState();
        }
    }
}
