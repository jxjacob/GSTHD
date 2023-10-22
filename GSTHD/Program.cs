using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GSTHD
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            //dembunging
            //Form1 F1 = new Form1();
            //try
            //{
            //    Application.Run(F1);
            //} catch (OutOfMemoryException)
            //{
            //    F1.SaveState(true);
            //    MessageBox.Show("GSTHD has run into a fatal memory issue and will now close.\nYour tracker state has been saved to the GSTHD directory can be reloaded later.\nWe apologize for the inconvenience.", "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    Application.Exit();
            //}
        }
    }
}
