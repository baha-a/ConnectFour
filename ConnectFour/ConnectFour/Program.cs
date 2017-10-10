using System;
using System.Windows.Forms;

namespace ConnectFour
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Console.SetOut(System.IO.File.CreateText("c:\\e.txt"));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
