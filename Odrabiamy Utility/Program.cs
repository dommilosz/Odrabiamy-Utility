using CefSharp.WinForms;
using System;
using System.Diagnostics;
using System.IO;
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
                ChromiumWebBrowser browser = new ChromiumWebBrowser("odrabiamy.pl");
                browser.Dispose();
            }
            catch
            {
                if (File.Exists(Application.StartupPath + "/DLL/OU.exe"))
                {
                    Process.Start(Application.StartupPath + "/DLL/OU.exe");
                    return;
                }
                WinFormsUtil.WFUtil.DLLWindow(@"https://github.com/dommilosz/Odrabiamy-Utility/releases/download/Service/CefSharp.zip");
                if (!tried)
                {
                    tried = true;
                    Main();
                }
            }
            WinFormsUtil.WFUtil.UpdateWindow("https://github.com/dommilosz/Odrabiamy-Utility/releases/latest", @"https://api.github.com/repos/dommilosz/Odrabiamy-Utility/releases/latest");
            Application.Run(new Utility());
        }
    }
}
