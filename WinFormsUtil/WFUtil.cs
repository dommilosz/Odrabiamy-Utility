using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsUtil
{
    public static class WFUtil
    {
        public static string SingleInput(string title, string label, string value)
        {
            var ib = new InputBox();
            ib.label1.Text = label;
            ib.Text = title;
            ib.Output.Text = value;
            ib.Output.Visible = true;
            ib.button1.Visible = true;
            ib.label1.Visible = true;
            ib.ShowDialog();
            if (ib.Output.Text.Length > 0)
                return ib.Output.Text;
            return value;
        }
        public static string[] LoginInput(string title, string label, string value, string value2)
        {
            var ib = new InputBox();
            ib.Size = new System.Drawing.Size(ib.Width, ib.Height + 300);
            ib.label1.Text = label;
            ib.Text = title;
            ib.Output.Text = value;
            ib.Output.Visible = true;
            ib.button1.Visible = true;
            ib.label1.Visible = true;
            ib.PassInput.Visible = true;
            ib.PassInput.Text = value2;
            ib.PassInput.UseSystemPasswordChar = true;
            ib.ShowDialog();
            string[] ret = { ib.Output.Text, ib.PassInput.Text };
            if (ib.Output.Text.Length > 0&&ib.PassInput.Text.Length>0)
                return ret;
            return null;
        }
        public static void SingleProgress(string title, string label, int value)
        {
            var ib = new InputBox();
            ib.label1.Text = label;
            ib.Text = title;
            ib.label1.Visible = true;
            ib.progressBar1.Visible = true;
            ib.progressBar1.Value = value;
            if (value == 100) ib.timer1.Start();
            ib.ShowDialog();

        }
        public static Version GetVersion()
        {
            if (Application.ProductName == "DEV")
            {
                return null;
            }
            Version v = new Version(Application.ProductVersion);
            return v;
        }
        public static Version GetLatestVersion(string giturl, string apiurl)
        {
            string ver = UPDATES.CheckUpdates(giturl, apiurl);
            if (ver.Split(':')[1].Contains("T"))
            {
                return new Version(ver.Split(':')[2].Replace("]", ""));
            }
            return null;
        }
        public static bool IsUpToDate(string giturl, string apiurl)
        {
            GetLatestVersion(giturl, apiurl);
            return UPDATES.uptodate;
        }
        public static void Update(string giturl, string apiurl)
        {
            UPDATES.Update(giturl, apiurl);
        }
        public static void DownloadDLL(string giturl)
        {
            UPDATES.DownloadDLL(giturl);
        }
        public static void UpdateWindow(string giturl, string apiurl)
        {
            if (IsUpToDate(giturl, apiurl)) return;
            Updater u = new Updater();
            u.UpdateForm();
            u.ShowDialog();

        }
        public static void DLLWindow(string giturl)
        {
            Updater u = new Updater();
            u.DLLForm(giturl);
            u.ShowDialog();

        }
    }

    static class UPDATES
    {
        public static string newpatch = "";
        public static Uri batchURL;
        public static Uri update_url;
        public static bool uptodate = false;
        public static bool dev = false;
        public static Version installed;
        public static Version remote;
        public static int progress = 0;
        public static int BytesPSavg = 0;
        public static string apiurl = "";
        public static string appname = "Odrabiamy.Utility.exe";
        public static string token = "?client_id=9a3e58501214628adc6d&client_secret=9f30900fad7b567e1c59e931f0518cedc8aec68c";
        class GitHubRelease
        {
            [JsonProperty("tag_name")]
            public string Tag { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("published_at")]
            public string ReleaseTime { get; set; }

            [JsonProperty("body")]
            public string Description { get; set; }
        }
        public static string CheckUpdates(string giturl, string apiurl2)
        {
            apiurl = apiurl2;
            try
            {
                uptodate = true;
                dev = false;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent", "request");
                var json = wc.DownloadString(new Uri(apiurl + token));
                GitHubRelease latest = JsonConvert.DeserializeObject<GitHubRelease>(json);
                string latestVersion = latest.Tag,
                currentVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
                installed = System.Version.Parse(currentVersion);
                remote = System.Version.Parse(latestVersion);
                update_url = new Uri($"{giturl}/download/{appname}" + token);
                newpatch = Application.ExecutablePath.Replace(".exe", "_" + latest.Tag + ".exe");
                batchURL = new Uri(@"https://github.com/dommilosz/FTP-SCREEN-SHOT-CONSOLE/releases/download/SV/Update.bat" + token);
                if (Application.ProductVersion.Contains("DEV"))
                {
                    dev = true;
                    return $"[TRUE:T:{remote}:DEV]";
                }
                if (installed < remote)
                {
                    uptodate = false;
                    return $"[FALSE:T:{remote}]";
                }
                else return $"[TRUE:T:{remote}:LATEST]";
            }
            catch { }
            return "ERROR";
        }
        public static string Update(string giturl, string apiurl)
        {
            Int32 sec = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            progress = 0;
            BytesPSavg = 0;
            int fileindex = 0;
            CheckUpdates(giturl, apiurl);
            if (uptodate || dev)
            {
                return "[FALSE:LATEST]";
            }

            string exec = Application.ExecutablePath.Replace(Application.StartupPath, "");
            string execnew = newpatch.Replace(Application.StartupPath, "");
            exec = exec.TrimStart(@"\".ToCharArray()[0]);
            execnew = execnew.TrimStart(@"\".ToCharArray()[0]);
            WebClient w = new WebClient();
            w.DownloadProgressChanged += W_DownloadProgressChanged;
            w.DownloadFileCompleted += W_DownloadFileCompleted;
            w.DownloadFileAsync(update_url, newpatch);
            void W_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                progress = 100;
                switch (fileindex)
                {
                    case 0: { w.DownloadFileAsync(batchURL, Application.StartupPath + "Update.bat"); fileindex++; } break;
                    case 1:
                        {
                            string args = "\"" + execnew + "\" \"" + exec + "\"";
                            Thread.Sleep(2000);
                            Process.Start(Application.StartupPath + "Update.bat", args);
                            Application.Exit();
                        }
                        break;
                }

            }
            void W_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                Int32 sec2 = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                int duration = sec2 - sec;
                if (duration < 1) duration++;
                progress = e.ProgressPercentage;
                BytesPSavg = (Convert.ToInt32(e.BytesReceived)) / duration;
            }
            return "";
        }
        public static string DownloadDLL(string giturl)
        {
            int sec = DateTime.Now.Second;
            progress = 0;
            BytesPSavg = 0;

            WebClient w = new WebClient();
            w.DownloadProgressChanged += W_DownloadProgressChanged;
            w.DownloadFileCompleted += W_DownloadFileCompleted;
            w.DownloadFileAsync(new Uri(giturl), newpatch + "-DLL.dll");
            void W_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
            {
                string dir = Application.StartupPath + "/DLL";
                Directory.CreateDirectory(dir);
                progress = 100;
                ZipFile.ExtractToDirectory(newpatch + "-DLL.dll", dir);
                File.Delete(newpatch + "-DLL.dll");
                File.Copy(Application.ExecutablePath, dir + "/OU.exe");
            }
            void W_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                int sec2 = DateTime.Now.Second;
                int duration = sec2 - sec;
                progress = e.ProgressPercentage;
                BytesPSavg = (Convert.ToInt32(e.BytesReceived)) / duration;
            }
            return "";
        }
    }
}
