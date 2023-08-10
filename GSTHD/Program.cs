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
            //dembunging
            //try
            //{
                Application.Run(new Form1());
            //} catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Source + ":\n" + ex.Message, "GSTHD", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }
    }
}
