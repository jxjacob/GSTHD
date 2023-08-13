using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    public static class ControlExtensions
    {
        public static void ClearAndDispose(this Control ctrl)
        {
            while (ctrl.Controls.Count != 0)
            {
                ClearAndDispose(ctrl.Controls[0]);
            }
            if (ctrl is GossipStone gs)
            {
                gs.NukeTimer();
            }
            if (ctrl is PanelWothBarren pw)
            {
                foreach (WotH item in pw.ListWotH)
                {
                    foreach (Control stone in item.listGossipStone)
                    {
                        ClearAndDispose(stone);
                    }
                }
            }
            ctrl.Controls.Clear();
            ctrl.Dispose();
        }

    }
}
