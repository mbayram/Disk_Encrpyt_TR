using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            eula fr1 = new eula();
            Form1 fr2 = new Form1();
            fr1.ShowDialog();
            fr1.BringToFront();
            fr1.TopMost = true;

            if (fr1.btnok == true)
            {
                fr1.Close();
                fr2.ShowDialog();
            }
            else
            {
                // The user declined. Close the program.
                fr1.Close();
                return;
            }

        }
    }
}
