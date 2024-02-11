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
            modDown = false;
        }

        public void Mouse_ClickDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (e.Clicks > 1)
                    {
                        Element.ToggleCheck();
                    } else
                    {
                        Mouse_LeftClickDown(sender, e);
                    }
                    break;
                case MouseButtons.Middle:
                    Mouse_MiddleClickDown(sender, e);
                    break;
                case MouseButtons.Right:
                    Mouse_RightClickDown(sender, e);
                    break;
            }
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
