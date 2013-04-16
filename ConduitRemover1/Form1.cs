using ConduitRemover.Logics;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        void firefox_DoneRemoving(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnShown(EventArgs e)
        {
            foreach (Process p in Process.GetProcesses())
            {
                try
                {
                    Debug.WriteLine(p.MainModule.FileName);
                }
                catch (Exception ex)
                {
                }
            }

            base.OnShown(e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.ExitThread();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            btnInstall.Enabled = false;

            // initialize
            checkBox0.Checked = true;
            checkBox0.Enabled = true;
            Application.DoEvents();

            // Checking Conduit installation
            {
                label2.Enabled = true;

                // check firefox
                checkBox2.Enabled = true;
                Application.DoEvents();
                if (Firefox.I.IsConduitInstalled())
                {
                    checkBox2.Checked = true;
                    Installer.I.CreateUninstaller(true, false, false);
                    Application.DoEvents();
                }
                else
                {
                    label3.Visible = true;
                }

                // check chrome
                checkBox3.Enabled = true;
                Application.DoEvents();
                if (Chrome.I.IsConduitInstalled())
                {
                    checkBox3.Checked = true;
                    Installer.I.CreateUninstaller(false, true, false);
                    Application.DoEvents();
                }
                else
                {
                    label4.Visible = true;
                }

                // check internet explorer
                checkBox1.Enabled = true;
                Application.DoEvents();
                if (InternetExplorer.I.IsConduitInstalled())
                {
                    checkBox1.Checked = true;
                    Installer.I.CreateUninstaller(false, false, true);
                    Application.DoEvents();
                }
                else
                {
                    label5.Visible = true;
                }
            }

            // done
            checkBox4.Checked = true;
            checkBox4.Enabled = true;
            Application.DoEvents();
        }
    }
}
