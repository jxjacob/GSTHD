using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    public static class MouseDetermination
    {

        public static bool DetermineBasicMouseInput(MouseEventArgs e, Settings.BasicActionButtonOption ba)
        {
            if (e.Button == MouseButtons.Left && ba == Settings.BasicActionButtonOption.Left ||
                e.Button == MouseButtons.Right && ba == Settings.BasicActionButtonOption.Right ||
                e.Button == MouseButtons.Middle && ba == Settings.BasicActionButtonOption.Middle ||
                e.Button == MouseButtons.XButton1 && ba == Settings.BasicActionButtonOption.MouseButton1 ||
                e.Button == MouseButtons.XButton2 && ba == Settings.BasicActionButtonOption.MouseButton2 )
            {
                return true;
            }
            return false;
        }
    }
}
