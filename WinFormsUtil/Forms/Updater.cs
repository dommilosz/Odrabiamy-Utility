using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsUtil
{
    public partial class Updater : Form
    {
        string mode = "update";
        string dllpatch = "";
        public Updater()
        {
            InitializeComponent();
        }
        public void UpdateForm()
        {
            mode = "update";
            AddLine($"[UPDATE] UPDATE FOUND! {UPDATES.installed} >>>> {UPDATES.remote}");
            AddLine($"[UPDATE] CLICK \"DOWNLOAD\" TO DOWNLOAD LATEST VERSION");
            AddLine($"[DEBUG ] UPDATE PATCH : {UPDATES.update_url}");
        }
        public void DLLForm(string patch)
        {
            mode = "dll";
            AddLine($"[UPDATE] DLL NOT FOUND! {UPDATES.installed} ");
            AddLine($"[UPDATE] CLICK \"DOWNLOAD\" TO GET DLLs");
            AddLine($"[DEBUG ] UPDATE PATCH : {patch}");
            dllpatch = patch;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        public void AddLine(string line)
        {
            richTextBox1.AppendText(line + "\n");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StartDownload();
        }

        public void StartDownload()
        {
            button1.Enabled = false;
            button2.Enabled = false;
            switch (mode)
            {
                case "update": UPDATES.Update(UPDATES.update_url.AbsoluteUri);break;
                case "dll": UPDATES.DownloadDLL(dllpatch);break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(UPDATES.progress<=100)
            progressBar1.Value = UPDATES.progress;
            if (UPDATES.progress == 100) { button2.Enabled = true; this.Close(); }
        }

        private void Updater_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!button2.Enabled)
            e.Cancel = true;
        }
    }
}
