using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Odrabiamy_Utility
{
    static class Program
    {
        static bool tried = false;
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!tried)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }
            try
            {
                WinFormsUtil.WFUtil.UpdateWindow("https://github.com/dommilosz/Odrabiamy-Utility/releases/latest");
                Application.Run(new Utility());
            }
            catch 
            {
                if (File.Exists(Application.StartupPath + "/DLL/OU.exe"))
                {
                    Process.Start(Application.StartupPath + "/DLL/OU.exe");
                    return;
                }
                WinFormsUtil.WFUtil.DLLWindow(@"https://github.com/dommilosz/Odrabiamy-Utility/releases/download/Service/CefSharp.zip"); 
                
            }
            if (!tried)
            {
                tried = true;
                Main();
            }
        }
    }
}
