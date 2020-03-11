using CefSharp;
using CefSharp.WinForms;
using ScreenShotDemo;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WinFormsUtil;

namespace Odrabiamy_Utility
{
    public partial class Utility : Form
    {
        public static ChromiumWebBrowser browser;
        public static ChromiumWebBrowser navbrowser;
        public static bool isDllError = true;
        List<Answer> answers = new List<Answer>();
        bool exit = false;
        public Utility()
        {
            CefSettings settings = new CefSettings();
            isDllError = false;
            string path = Application.StartupPath + "/DATA/CACHE";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            settings.CachePath = path;
            settings.PersistSessionCookies = true;
            //Initialize Cef with the provided settings
            Cef.Initialize(settings);

            InitializeComponent();
            InitializeBrowsers();
            try
            {
                var lines = File.ReadAllLines(Application.StartupPath + "/DATA/sc.save").ToList();
                for (int i = 0; i < lines.Count; i += 3)
                {
                    string url = lines[i];
                    string name = lines[i + 1];
                    string image = lines[i + 2];
                    AddAns(url, name, image);
                }
            }
            catch
            {

            }
        }

        private void InitializeBrowsers(bool postinit = false)
        {


            ThreadStart ts = new ThreadStart(Init);
            Thread t = new Thread(ts);
            t.Start();
            void Init()
            {
                browser = new ChromiumWebBrowser("odrabiamy.pl");
                navbrowser = new ChromiumWebBrowser("odrabiamy.pl");
            }
            while (browser == null && navbrowser == null) Application.DoEvents();
            panel3.Controls.Add(navbrowser);
            navbrowser.BringToFront();
            navbrowser.Size = new Size(100, 100);
            navbrowser.Dock = DockStyle.None;
            panel1.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
            browser.BringToFront();
            browser.AddressChanged += Browser_AddressChanged;
            timer1.Enabled = true;
            RenderTimer.Enabled = true;
            SlowRenderTimer.Enabled = true;
            SuperSlowRenderTimer.Enabled = true;

        }

        public void Browser_AddressChanged(object sender, CefSharp.AddressChangedEventArgs e)
        {
            navbrowser.Load("https://odrabiamy.pl/blad");
        }

        private void BACK_Click(object sender, EventArgs e)
        {
            if (browser.CanGoBack) browser.GetBrowser().GoBack();
        }

        private void FORWARD_Click(object sender, EventArgs e)
        {
            if (browser.CanGoForward) browser.GetBrowser().GoForward();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            Render();
        }

        private void SlowRenderTimer_Tick(object sender, EventArgs e)
        {
            RenderSlow();
        }

        public void Render()
        {
            BACK.Enabled = browser.CanGoBack;
            FORWARD.Enabled = browser.CanGoForward;
            itemToolStripMenuItem.Visible = listBox1.SelectedIndex >= 0;
            if (new Rectangle(panel3.Location, panel3.Size).Contains(PointToClient(Control.MousePosition)) && panel3.ContainsFocus)
            {
                if (panel3.Size != new Size(panel3.Size.Width, 350)) panel3.Size = new Size(100, 350);
            }
            else { if (panel3.Size != new Size(panel3.Size.Width, 76)) { panel3.Size = new Size(100, 76); browser.Focus(); } }
            if (!navbrowser.GetBrowser().MainFrame.Url.Contains("blad")) { browser.Load(navbrowser.GetBrowser().MainFrame.Url); navbrowser.Load("https://odrabiamy.pl/blad"); browser.Focus(); }
            DoScript("document.getElementsByClassName(\"search-form\")[0].scrollIntoView()", navbrowser);
            int addx = 0;
            if (panel3.Width < 1350)
            {
                addx = 1350 - panel3.Width;
            }
            int width = panel3.Width + addx;
            navbrowser.Size = new Size(width, panel3.Height);
            float p2 = 100;
            p2 = panel3.Width / 1345f * 100;
            float p = p2 * 0.9f;
            p = (float)Math.Floor(p);
            List<string> scripts = new List<string>();
            //scripts.Add($"document.getElementsByClassName(\"container\")[0].style.fontSize = \"{p}%\"");
            //scripts.Add($"document.getElementsByClassName(\"btn btn-primary\")[0].style.fontSize = \"{p}%\"");
            scripts.Add($"document.getElementsByClassName(\"container\")[0].style.marginRight = \"auto\"");
            scripts.Add($"document.getElementsByClassName(\"container\")[0].style.marginLeft = \"unset\"");
            scripts.Add($"document.getElementsByClassName(\"container\")[0].style.maxWidth = \"{panel3.Width - 100}px\"");
            scripts.Add($"document.getElementsByClassName(\"container\")[0].style.Width = \"{panel3.Width - 100}px\"");
            DoScript(scripts, navbrowser);
            if (browser.Address.Contains("blad")) browser.GetBrowser().GoBack();
        }

        public void RenderSlow()
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(answers[listBox1.SelectedIndex].preview);
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                pictureBox1.Image = image;
            }
            catch { }
            string url = browser.GetBrowser().MainFrame.Url;
            this.Text = $"Odrabiamy Utility  --  {url}";
            List<string> url_parts = url.Split('/').ToList();
            string HTML = GetHTML();
            int i1 = HTML.IndexOf("<div class=\"container-content\">");
            int i2 = HTML.IndexOf("<div class=\"container-info\">");
            string[] navigation_txts = new string[] { "-", "-", "-" };
            if (i1 != i2 && i1 > 0 && i2 > 0 && browser.IsBrowserInitialized)
            {
                foreach (var item in url_parts)
                {
                    if (item.Contains("ksiazka-")) navigation_txts[0] = item.Replace("ksiazka-", "");
                }
                foreach (var item in url_parts)
                {
                    if (item.Contains("strona-")) navigation_txts[2] = item.Replace("strona-", "");
                }
                foreach (var item in url_parts)
                {
                    if (item.Contains("zadanie-")) navigation_txts[1] = item.Replace("zadanie-", "");
                }
                SAVE_ANSWER.ForeColor = Color.Green;
            }
            else
            {
                SAVE_ANSWER.ForeColor = Color.Gold;
            }
            List<string> scripts = new List<string>();
            scripts.Add("document.getElementsByClassName(\"page-nav\")[0].children[0].className");
            scripts.Add("document.getElementsByClassName(\"page-nav\")[0].children[1].innerText");
            scripts.Add("document.getElementsByClassName(\"page-nav\")[0].children[2].className");
            scripts.Add("document.getElementsByClassName(\"exercise-label current\")[0].parentElement.href");
            var output = DoScript(scripts);
            toolStripMenuItem4.Enabled = false;
            if (!output[0].Contains("disable") && output[0].Contains("exercise-navigator")) toolStripMenuItem4.Enabled = true;
            nextPageToolStripMenuItem.Enabled = false;
            if (!output[2].Contains("disable") && output[2].Contains("exercise-navigator")) nextPageToolStripMenuItem.Enabled = true;

            bool valid = true;
            try { Convert.ToInt32(output[1]); } catch { valid = false; }

            if (output[1].Length > 0 && output[1].Length < 5 && valid)
            {
                navigation_txts[2] = output[1];
                if (output[3].Length > 10 && output[3].Contains("zadanie-"))
                {
                    var tmp = output[3].Split('/');
                    navigation_txts[1] = tmp[6].Replace("zadanie-", "");
                }
            }
            toolStripMenuItem4.Visible = true; nextPageToolStripMenuItem.Visible = true;
            if (navigation_txts[2] == "-") { toolStripMenuItem4.Visible = false; nextPageToolStripMenuItem.Visible = false; }
            txt_Book.Text = navigation_txts[0];
            txt_Ex.Text = navigation_txts[1];
            txt_Page.Text = navigation_txts[2];
            string navhtml = GetHTML(navbrowser);
            if (navhtml.Contains("Przejdź do Odrabiamy"))
            {
                label2.BringToFront();
                label2.Text = "Accept RODO";
            }
            else
            {
                label2.SendToBack();
            }
        }
        public void RenderVerySlow()
        {
            List<string> navscripts = new List<string>();
            navscripts.Add("document.body.style.visibility = \"hidden\"");
            navscripts.Add("document.body.style.overflow = \"hidden\"");
            navscripts.Add("document.getElementsByClassName(\"search-form\")[0].style.visibility = \"visible\"");
            navscripts.Add("document.body.style.backgroundColor = \"darkgray\"");
            navscripts.Add("document.getElementsByClassName(\"search-form\")[0].scrollIntoView()");
            navscripts.Add("document.getElementsByClassName(\"search-form\")[0].clientHeight");
            var navoutput = DoScript(navscripts, navbrowser);
        }

        private void HOME_Click(object sender, EventArgs e)
        {
            browser.Load("odrabiamy.pl");
        }

        private void SAVE_ANSWER_Click(object sender, EventArgs e)
        {
            string name = browser.GetBrowser().MainFrame.Url.Replace("https://odrabiamy.pl/", "Home/");
            string HTML = GetHTML();
            AddAns(browser.GetBrowser().MainFrame.Url, name, GetPreview());
            tabControl1.SelectedTab = tabControl1.TabPages[1];
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void AddAns(string res, string name, string img)
        {
            Answer a = new Answer(res, name, img);
            answers.Add(a);
            RefreshScreenshots();
        }
        public void RefreshScreenshots()
        {
            var lv = listBox1.Items;
            lv.Clear();
            foreach (var item in answers)
            {
                lv.Add(item.name);
            }
        }
        public string GetPreview()
        {
            ScreenCapture sc = new ScreenCapture();
            Bitmap b = (Bitmap)sc.CaptureScreen();
            {
                Rectangle cropRect = new Rectangle(browser.PointToScreen(Point.Empty), browser.Size);
                Bitmap src = b;
                Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                     cropRect,
                                     GraphicsUnit.Pixel);
                }
                b = target;
            }
            using (Image image = b)
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, ImageFormat.Tiff);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        public string GetHTML(ChromiumWebBrowser b = null)
        {
            string html = "#null";
            if (browser.IsBrowserInitialized)
                html = DoScript(@"document.getElementsByTagName('html')[0].innerHTML", b);
            return html;
        }
        public void SetHTML(string html, ChromiumWebBrowser b = null)
        {
            DoScript($"document.documentElement.innerHTML = '{html}'", b);
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            browser.GetBrowser().Reload();
            navbrowser.Load("https://odrabiamy.pl/blad");
        }

        public string DoScript(string script, ChromiumWebBrowser b = null)
        {
            if (b == null) b = browser;
            var dosrc = b.GetBrowser().MainFrame.EvaluateScriptAsync(script);
            script = script.Replace('\n', ' ');
            string html = "#null";

            if (dosrc.Result.Message != null)
                html = dosrc.Result.Message;

            if (dosrc.Result != null && dosrc.Result.Result != null)
                html = dosrc.Result.Result.ToString();

            return html;

        }
        public List<string> DoScript(List<string> scripts, ChromiumWebBrowser b = null)
        {
            List<string> output = new List<string>();
            foreach (var item in scripts)
            {
                output.Add(DoScript(item, b));
            }
            return output;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderSlow();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            answers.RemoveAt(listBox1.SelectedIndex);
            RefreshScreenshots();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            answers[listBox1.SelectedIndex] = new Answer(answers[listBox1.SelectedIndex].source, WFUtil.SingleInput("Rename", "Enter a new name for screenshot", answers[listBox1.SelectedIndex].name), answers[listBox1.SelectedIndex].preview);
            RefreshScreenshots();
        }

        private void Utility_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!exit)
            {
                List<string> lines = new List<string>();
                foreach (var item in answers)
                {
                    lines.Add(item.source);
                    lines.Add(item.name);
                    lines.Add(item.preview);
                }

                File.WriteAllLines(Application.StartupPath + "/DATA/sc.save", lines.ToArray());
                WFUtil.SingleProgress("Saving", "Saving screenshots to file", 100); exit = true;
            }
            Application.Exit();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            answers.Clear();
            RefreshScreenshots();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do You Want To Reload ScreenShots From File?", "RELOAD?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                answers.Clear();
                try
                {
                    var lines = File.ReadAllLines(Application.StartupPath + "/DATA/sc.save").ToList();
                    for (int i = 0; i < lines.Count; i += 3)
                    {
                        string url = lines[i];
                        string name = lines[i + 1];
                        string image = lines[i + 2];
                        AddAns(url, name, image);
                    }
                }
                catch
                {

                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> lines = new List<string>();
            foreach (var item in answers)
            {
                lines.Add(item.source);
                lines.Add(item.name);
                lines.Add(item.preview);
            }

            File.WriteAllLines(Application.StartupPath + "/DATA/sc.save", lines.ToArray());
            WFUtil.SingleProgress("Saving", "Saving screenshots to file", 100);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (!navbrowser.IsBrowserInitialized || !browser.IsBrowserInitialized || !navbrowser.GetBrowser().HasDocument) { Application.DoEvents(); }
            RenderTimer.Start(); ;
            SlowRenderTimer.Start();
            SuperSlowRenderTimer.Start();

        }

        private void SuperSlowRenderTimer_Tick(object sender, EventArgs e)
        {
            RenderVerySlow();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (txt_Page.Text != "-")
            {
                int page = Convert.ToInt32(txt_Page.Text);
                string url = browser.GetBrowser().MainFrame.Url;
                if (!url.Contains("strona-")) return;
                url = url.Replace($"strona-{page}", $"strona-{page - 1}");
                browser.Load(url);
            }
        }

        private void nextPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txt_Page.Text != "-")
            {
                int page = Convert.ToInt32(txt_Page.Text);
                string url = browser.GetBrowser().MainFrame.Url;
                if (!url.Contains("strona-")) return;
                url = url.Replace($"strona-{page}", $"strona-{page + 1}");
                browser.Load(url);
            }
        }

        private void scriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(DoScript(WFUtil.SingleInput("Script", "Enter Script to Execute", "")), "Script Results");
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Application.StartupPath + "/DATA/Export"))
                Directory.CreateDirectory(Application.StartupPath + "/DATA/Export");
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (!Directory.Exists(Application.StartupPath + $"/DATA/Export/{unixTimestamp}"))
                Directory.CreateDirectory(Application.StartupPath + $"/DATA/Export/{unixTimestamp}");
            foreach (var item in answers)
            {
                string imageText = item.preview;
                string name = item.name.Replace('/', '-');
                name = name.Replace(@"\"[0], '-');
                name = name.Replace(':', '-');
                string path = Application.StartupPath + $"/DATA/Export/{unixTimestamp}/{name}.png";
                if (File.Exists(path)) path += $" - {unixTimestamp}.png";
                Image img;
                byte[] bitmapData = new byte[imageText.Length];
                bitmapData = Convert.FromBase64String(imageText);

                using (var streamBitmap = new MemoryStream(bitmapData))
                {
                    using (img = Image.FromStream(streamBitmap))
                    {
                        img.Save(path);
                    }
                }

            }
            string path2 = Application.StartupPath + $"/DATA/Export";
            foreach (var item in Directory.GetDirectories(path2))
            {
                if (Directory.GetFiles(item).Length < 1)
                {
                    Directory.Delete(item);
                }
                if (item.Contains(".Latest---"))
                {
                    Directory.Move(item, item.Replace(".Latest---", ""));
                }
            }
            Directory.Move(path2 + "/" + unixTimestamp, path2 + "/" + ".Latest---" + unixTimestamp);
            MessageBox.Show($"EXPORTED to {path2}", "EXPORTED");
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }
    }
}



public class Answer
{
    public string source = "";
    public string name = "";
    public string preview;
    public Answer(string html, string name, string img)
    {
        source = html;
        this.name = name;
        preview = img;
    }
}


namespace ScreenShotDemo
{
    /// <summary>
    /// Provides functions to capture the entire screen, or a particular window, and save it to a file.
    /// </summary>
    public class ScreenCapture
    {
        /// <summary>
        /// Creates an Image object containing a screen shot of the entire desktop
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }
        /// <summary>
        /// Captures a screen shot of the entire desktop, and saves it to a file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }
    }
}
