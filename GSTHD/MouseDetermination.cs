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
                e.Button == MouseButtons.Middle && ba == Settings.BasicActionButtonOption.Middle)
            {
                return true;
            }
            return false;
        }
    }
}
