using ConduitRemover.Logics.Remover;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConduitRemover
{
    public partial class PrepareInstall : Form
    {
        public PrepareInstall()
        {
            InitializeComponent();
            InitializeControlEvents();
            InitializeControls();
        }

        void InitializeControls()
        {
        }

        void InitializeControlEvents()
        {
            this.Shown += PrepareInstall_Shown;
        }

        void PrepareInstall_Shown(object sender, EventArgs e)
        {
            
        }

        protected override void OnShown(EventArgs e)
        {
            string ie_default_page = InternetExplorer.I.GetDefaultPage();
            string ff_default_page = Firefox.I.GetDefaultPage();
            string gc_default_page = Chrome.I.GetDefaultPage();

            string SoftwareKey = "SOFTWARE";
            string main = string.Empty;

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            main = SoftwareKey; // +"\\" + "AirSoftware";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(main, true);
            key = key.CreateSubKey("AirSoftware");
            key = key.CreateSubKey("InternetHelper");
            key.SetValue("ie_default_page", ie_default_page);
            key.SetValue("ff_default_page", ff_default_page);
            key.SetValue("gc_default_page", gc_default_page);
            key.SetValue("version", Application.ProductVersion.ToString());

            //
            Application.ExitThread();

            base.OnShown(e);
        }

        public int GetOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }
    }
}
