using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;

namespace ConduitRemover.Logics
{
    public class Installer
    {
        string UninstallKey = string.Empty;
        List<Model_Uninstall> Uninstallers = new List<Model_Uninstall>();

        public string chrome_guid = "CT{5061D699-D1F1-4FB9-AF66-70F32E7B7C52}";
        public string firefox_guid = "CT{88D9978A-22D8-4C88-AD94-6B7D6B1DE5F2}";
        public string iexplore_guid = "CT{73A30644-C555-4E6D-8C3A-A356D7483DA8}";

        public Installer()
        {
            //CreateUninstaller();
        }
        static readonly Installer i = new Installer();
        public static Installer I { get { return i; } }

        public void CreateUninstaller(bool firefox, bool chrome, bool iexplore)
        {
            FileVersionInfo fvi = Process.GetCurrentProcess().MainModule.FileVersionInfo;

            if (chrome)
            {
                Uninstallers.Add(new Model_Uninstall()
                {
                    GUID = chrome_guid,
                    DisplayName = "Conduit Uninstaller for Chrome",
                    DisplayVersion = fvi.ProductVersion,
                    EstimatedSize = 119,
                    InstallLocation = Application.StartupPath,
                    Publisher = "Red Brick Media",
                    UninstallString = Application.ExecutablePath + " -uninstall_ch",
                    VersionMajor = fvi.ProductMajorPart,
                    VersionMinor = fvi.ProductMinorPart,
                    InstallDate = DateTime.Today.ToString("MMddyyyy"),
                    DisplayIcon = Application.ExecutablePath + ",0"
                });
            }

            if (firefox)
            {
                Uninstallers.Add(new Model_Uninstall()
                {
                    GUID = firefox_guid,
                    DisplayName = "Conduit Uninstaller for Firefox",
                    DisplayVersion = fvi.ProductVersion,
                    EstimatedSize = 119,
                    InstallLocation = Application.StartupPath,
                    Publisher = "Red Brick Media",
                    UninstallString = Application.ExecutablePath + " -uninstall_ff",
                    VersionMajor = fvi.ProductMajorPart,
                    VersionMinor = fvi.ProductMinorPart,
                    InstallDate = DateTime.Today.ToString("MMddyyyy"),
                    DisplayIcon = Application.ExecutablePath + ",0"
                });
            }

            if (iexplore)
            {
                Uninstallers.Add(new Model_Uninstall()
                {
                    GUID = iexplore_guid,
                    DisplayName = "Conduit Uninstaller for Internet Explorer",
                    DisplayVersion = fvi.ProductVersion,
                    EstimatedSize = 119,
                    InstallLocation = Application.StartupPath,
                    Publisher = "Red Brick Media",
                    UninstallString = Application.ExecutablePath + " -uninstall_ie",
                    VersionMajor = fvi.ProductMajorPart,
                    VersionMinor = fvi.ProductMinorPart,
                    InstallDate = DateTime.Today.ToString("MMddyyyy"),
                    DisplayIcon = Application.ExecutablePath + ",0"
                });
            }

            //string uninstall_key = CreateNewRegKey();
            //CreateUninstallInformation(uninstall_key);

            foreach (Model_Uninstall u in this.Uninstallers)
            {
                u.RegKey = CreateNewRegKey(u.GUID);
                CreateUninstallInformation(u);
            }
        }

        string CreateNewRegKey(string guid)
        {
            string SoftwareKey = "SOFTWARE";

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            UninstallKey = SoftwareKey + "\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            UninstallKey = UninstallKey + "\\" + guid;

            // {HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\UninstallSOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{5061D699-D1F1-4FB9-AF66-70F32E7B7C52}}

            RegistryKey regkey = Registry.LocalMachine.OpenSubKey(UninstallKey);

            if (regkey == null)
            {
                Registry.LocalMachine.CreateSubKey(UninstallKey);
            }

            return UninstallKey;
        }

        public void RemoveRegKey(string guid)
        {
            string SoftwareKey = "SOFTWARE";

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            UninstallKey = SoftwareKey + "\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            UninstallKey = UninstallKey + "\\" + guid;

            // {HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\UninstallSOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{5061D699-D1F1-4FB9-AF66-70F32E7B7C52}}

            Registry.LocalMachine.DeleteSubKey(UninstallKey);
        }

        void CreateUninstallInformation(Model_Uninstall u)
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(u.RegKey, true);

            reg.SetValue("DisplayName", u.DisplayName, RegistryValueKind.String);
            reg.SetValue("DisplayVersion", u.DisplayVersion, RegistryValueKind.String);
            reg.SetValue("EstimatedSize", u.EstimatedSize, RegistryValueKind.DWord);
            reg.SetValue("InstallLocation", u.InstallLocation, RegistryValueKind.String);
            reg.SetValue("Publisher", u.Publisher, RegistryValueKind.String);
            reg.SetValue("UninstallString", u.UninstallString, RegistryValueKind.String);
            reg.SetValue("VersionMajor", u.VersionMajor, RegistryValueKind.DWord);
            reg.SetValue("VersionMinor", u.VersionMinor, RegistryValueKind.DWord);
            reg.SetValue("InstallDate", u.InstallDate, RegistryValueKind.String);
            reg.SetValue("DisplayIcon", u.DisplayIcon, RegistryValueKind.String);
        }

        public int GetOSArchitecture()
        {
            string pa =
                Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) ||
                     String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }
    }

    public class Model_Uninstall
    {
        private string _DisplayIcon = string.Empty;
        public string DisplayIcon
        {
            get { return _DisplayIcon; }
            set
            {
                if (_DisplayIcon != value)
                {
                    _DisplayIcon = value;
                }
            }
        }
        private string _InstallDate = string.Empty;
        public string InstallDate
        {
            get { return _InstallDate; }
            set
            {
                if (_InstallDate != value)
                {
                    _InstallDate = value;
                }
            }
        }
        private string _RegKey = string.Empty;
        public string RegKey
        {
            get { return _RegKey; }
            set
            {
                if (_RegKey != value)
                {
                    _RegKey = value;
                }
            }
        }
        private string _GUID = string.Empty;
        public string GUID
        {
            get { return _GUID; }
            set
            {
                if (_GUID != value)
                {
                    _GUID = value;
                }
            }
        }
        private int _VersionMinor = 0;
        public int VersionMinor
        {
            get { return _VersionMinor; }
            set
            {
                if (_VersionMinor != value)
                {
                    _VersionMinor = value;
                }
            }
        }
        private int _VersionMajor = 0;
        public int VersionMajor
        {
            get { return _VersionMajor; }
            set
            {
                if (_VersionMajor != value)
                {
                    _VersionMajor = value;
                }
            }
        }
        private string _UninstallString = string.Empty;
        public string UninstallString
        {
            get { return _UninstallString; }
            set
            {
                if (_UninstallString != value)
                {
                    _UninstallString = value;
                }
            }
        }
        private string _Publisher = string.Empty;
        public string Publisher
        {
            get { return _Publisher; }
            set
            {
                if (_Publisher != value)
                {
                    _Publisher = value;
                }
            }
        }
        private string _InstallLocation = string.Empty;
        public string InstallLocation
        {
            get { return _InstallLocation; }
            set
            {
                if (_InstallLocation != value)
                {
                    _InstallLocation = value;
                }
            }
        }
        private int _EstimatedSize = 0;
        public int EstimatedSize
        {
            get { return _EstimatedSize; }
            set
            {
                if (_EstimatedSize != value)
                {
                    _EstimatedSize = value;
                }
            }
        }
        private string _DisplayVersion = string.Empty;
        public string DisplayVersion
        {
            get { return _DisplayVersion; }
            set
            {
                if (_DisplayVersion != value)
                {
                    _DisplayVersion = value;
                }
            }
        }
        private string _DisplayName = string.Empty;
        public string DisplayName
        {
            get { return _DisplayName; }
            set
            {
                if (_DisplayName != value)
                {
                    _DisplayName = value;
                }
            }
        }
    }
}
