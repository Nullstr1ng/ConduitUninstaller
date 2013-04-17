using ConduitRemover.Logics;
using ConduitRemover.Logics.Common;
using ConduitRemover.Logics.Remover;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ConduitRemover
{
    public partial class UninstallerAll : Form, iStatus
    {
        private string _WindowSubTitle = string.Empty;
        public string WindowSubTitle
        {
            get { return _WindowSubTitle; }
            set
            {
                if (_WindowSubTitle != value)
                {
                    _WindowSubTitle = value;
                    lblStatus.Text = value;
                }
            }
        }

        public UninstallerAll()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (Chrome.I.IsConduitInstalled())
            {
                UninstallChrome();
            }

            MessageBox.Show("HOY!!!!!!");

            //if (Firefox.I.IsConduitInstalled())
            //{
            //    UninstallFirefox();
            //}

            //if (InternetExplorer.I.IsConduitInstalled())
            //{
            //    UninstallInternetExplorer();
            //}

            Application.ExitThread();
        }

        void UninstallChrome()
        {
            // make sure to terminate all Chrome running process
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName.Contains("chrome.exe"))
                    {
                        p.Kill();
                        p.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                }
            }

            bool safe_to_go = true;
            // check it again
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName.Contains("chrome.exe"))
                    {
                        safe_to_go = false;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            if (safe_to_go)
            {
                Chrome.I.InitStatus(this);
                Chrome.I.GetProfiles();
                //Installer.I.RemoveRegKey(Installer.I.chrome_guid);
                //MessageBox.Show("Conduit toolbar for Chrome is now removed", "Conduit Uninstaller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Application.ExitThread();
            }
        }

        void UninstallFirefox()
        {
            // make sure to terminate all Chrome running process
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName.Contains("firefox.exe"))
                    {
                        p.Kill();
                        p.WaitForExit();
                    }
                }
                catch (Exception ex)
                {
                }
            }

            bool safe_to_go = true;
            // check it again
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName.Contains("firefox.exe"))
                    {
                        safe_to_go = false;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            if (safe_to_go)
            {
                Firefox.I.InitStatus(this);
                Firefox.I.StartCleaning();
                //Installer.I.RemoveRegKey(Installer.I.firefox_guid);
                //MessageBox.Show("Conduit toolbar for Firefox is now removed", "Conduit Uninstaller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Application.ExitThread();
            }
        }

        void UninstallInternetExplorer()
        {
            // make sure to terminate all Chrome running process
            Logger.i.AddLog("Terminating Internet Explorer");

            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName.Contains("iexplore.exe"))
                    {
                        Logger.i.AddLog("Killing IE");
                        try
                        {
                            p.Kill();
                            p.WaitForExit();
                        }
                        catch (Exception ex)
                        {
                            Logger.i.AddLog("Can't terminate IE due to error: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

            bool safe_to_go = true;
            // check it again
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    if (p.MainModule.FileName.Contains("iexplore.exe"))
                    {
                        safe_to_go = false;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            if (safe_to_go)
            {
                InternetExplorer.I.InitStatus(this);
                InternetExplorer.I.StartCleaning();
                //Installer.I.RemoveRegKey(Installer.I.iexplore_guid);
                //MessageBox.Show("Conduit toolbar for Internet Explorer is now removed", "Conduit Uninstaller", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Application.ExitThread();
            }
        }

        string _status = string.Empty;
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                this.WindowSubTitle = value;
                this.CurrentWorkIndex++;
                Application.DoEvents();
            }
        }

        public int MaxWork
        {
            get
            {
                return progressBar1.Maximum;
            }
            set
            {
                progressBar1.Maximum = value;
            }
        }

        public int CurrentWorkIndex
        {
            get
            {
                return progressBar1.Value;
            }
            set
            {
                progressBar1.Value = value;
                Application.DoEvents();
            }
        }
    }
}
