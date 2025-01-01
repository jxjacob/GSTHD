using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    internal interface PanelHint
    {
        void UpdateFromSettings();

        string Name { get; set; }
        int PlacedOrder { get; set; }

        LabelExtended LabelPlace { get; set; }
    }
}
