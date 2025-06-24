using System;
using System.Windows.Forms;

namespace ConfigTool
{
    public static class Program
    {
        public static Form1 Form1 { get; private set; }

        [STAThread]
        public static void Main()
        {
            Form1 = new Form1();
            Application.Run(Form1);
        }
    }
}