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
        public static bool onlycheckerror = true;
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
            try { WinFormsUtil.WFUtil.UpdateWindow("https://github.com/dommilosz/Odrabiamy-Utility/releases/latest", @"https://api.github.com/repos/dommilosz/Odrabiamy-Utility/releases/latest"); } catch { }
            try
            {
                Application.Run(new Utility());
            }
            catch (Exception ex)
            {
                if (Utility.isDllError)
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
                else
                {
                    throw ex;
                }
            }
        }
    }
}
