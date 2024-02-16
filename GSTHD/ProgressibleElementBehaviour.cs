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

        protected bool modDown = false;

        public ProgressibleElementBehaviour(ProgressibleElement<T> element, Settings settings)
        {
            Element = element;
            Settings = settings;
        }

        public void KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"{e.Modifiers} - {e.KeyCode}");
            if ((Settings.ExtraActionButton == Settings.ExtraActionModButton.Control && e.Modifiers == Keys.Control) ||
                (Settings.ExtraActionButton == Settings.ExtraActionModButton.Shift && e.Modifiers == Keys.Shift) ||
                (Settings.ExtraActionButton == Settings.ExtraActionModButton.Alt && e.Modifiers == Keys.Alt) ||
                (Settings.ExtraActionButton == Settings.ExtraActionModButton.MouseButton1 && e.KeyCode == Keys.XButton1) ||
                (Settings.ExtraActionButton == Settings.ExtraActionModButton.MouseButton2 && e.KeyCode == Keys.XButton2))
            {
                Debug.WriteLine("mod downing");
                modDown = true;
            } else
            {
                Debug.WriteLine("fail");
            }
        }

        public void KeyUp(object sender, KeyEventArgs e)
        {
            Debug.WriteLine("mod upping");
            modDown = false;
        }

        public bool DetermineMouseInput(MouseEventArgs e, Settings.ExtraActionModButton ea)
        {
            // might move this to the MouseDetermination struct, depending on how i go about the Control clicking
            if (e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.Left ||
                e.Button == MouseButtons.Middle && ea == Settings.ExtraActionModButton.Middle ||
                e.Button == MouseButtons.Right && ea == Settings.ExtraActionModButton.Right ||
                e.Button == MouseButtons.Left && ea == Settings.ExtraActionModButton.DoubleLeft && e.Clicks > 1)
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
            if (modDown)
            {
                Element.ToggleCheck();
                return;
            }
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
