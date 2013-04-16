using ConduitRemover.Logics.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConduitRemover.Logics.Remover
{
    public class InternetExplorer : iRemover
    {
        iStatus _stat;

        public InternetExplorer()
        {
        }
        static internal InternetExplorer _i = new InternetExplorer();
        public static InternetExplorer I { get { return _i; } }

        public void InitStatus(iStatus stat)
        {
            _stat = stat;
            _stat.MaxWork = 5;
            _stat.CurrentWorkIndex = 0;
        }

        public bool IsConduitInstalled()
        {
            bool ret = false;

            Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + "Checking if conduit is installed in Internet Explorer");

            string SoftwareKey = "SOFTWARE";
            string search_scope = string.Empty;

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            search_scope = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\SearchScopes";

            Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + "Opening key: " + search_scope);

            RegistryKey reg = null;

            reg = Registry.CurrentUser.OpenSubKey(search_scope, true);

            if (reg == null)
            {
                Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + search_scope + " return's null even though the system is at 64bit. Using the regular key now");
                SoftwareKey = "SOFTWARE";
                search_scope = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\SearchScopes";
                Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + "Trying to open the key: " + search_scope);
                reg = Registry.CurrentUser.OpenSubKey(search_scope, true);
            }

            if (reg != null)
            {
                List<string> search_scopes_guid = new List<string>(reg.GetSubKeyNames());
                string bing_search_scope_guid = string.Empty;

                Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + "Enumerating search scope guid");
                foreach (string se in search_scopes_guid)
                {
                    Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + "opening search_scope: " + se);
                    RegistryKey reg2 = reg.OpenSubKey(se, true);

                    if (reg2.GetValue("DisplayName").ToString().Contains("InternetHelper"))
                    {
                        Logger.i.AddLog(this.ToString() + ".IsConduitInstalled()" + "> " + "Conduit search engine found " + se);
                        ret = true;

                        break;
                    }
                }
            }
            else
            {
                Logger.i.AddLog(search_scope + ": returns NULL");
                ret = false;
            }

            return ret;
        }

        public string DefaultProfile
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GetDefaultProfile()
        {
            return string.Empty;
        }

        public void StartCleaning()
        {
            this.RemoveToolbar();
            this.RemoveSearchEngine();
            this.CleanSettings();
        }

        public void RemoveToolbar()
        {
            _stat.Status = "Removing InternetHelper Toolbar";

            string SoftwareKey = "SOFTWARE";
            string search_scope = string.Empty;

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            search_scope = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\Toolbar";

            RegistryKey reg = null;

            reg = Registry.LocalMachine.OpenSubKey(search_scope, true);

            if (reg == null)
            {
                Logger.i.AddLog(this.ToString() + ".RemoveToolbar()" + "> " + search_scope + " return's null even though the system is at 64bit. Using the regular key now");
                SoftwareKey = "SOFTWARE";
                search_scope = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\SearchScopes";
                Logger.i.AddLog(this.ToString() + ".RemoveToolbar()" + "> " + "Trying to open the key: " + search_scope);
                reg = Registry.LocalMachine.OpenSubKey(search_scope, true);
            }

            if (reg != null)
            {
                List<string> nvp = new List<string>(reg.GetValueNames());
                string name_value = string.Empty;

                Logger.i.AddLog(this.ToString() + ".RemoveToolbar()" + "> " + "Enumerating NVP");
                foreach (string name in nvp)
                {
                    string value = reg.GetValue(name).ToString();
                    Logger.i.AddLog(this.ToString() + ".RemoveToolbar()" + "> " + "Name: " + name + ", value: " + value);

                    if (value.Contains("InternetHelper"))
                    {
                        name_value = name;
                    }
                }

                Logger.i.AddLog(this.ToString() + ".RemoveToolbar()" + "> " + "Deleting value: " + name_value);
                if (reg.GetValue(name_value) != null)
                {
                    reg.DeleteValue(name_value);
                }
            }
        }

        public void RemoveSearchEngine()
        {
            _stat.Status = "Removing InternetHelper search engine";

            string SoftwareKey = "SOFTWARE";
            string search_scope = string.Empty;

            //int osarch = GetOSArchitecture();

            //if (osarch == 64)
            //{
            //    SoftwareKey = "SOFTWARE\\Wow6432Node";
            //}

            search_scope = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\SearchScopes";

            //Logger.i.AddLog("Opening key: " + search_scope);
            //RegistryKey reg = Registry.CurrentUser.OpenSubKey(search_scope, true);

            //
            RegistryKey reg = null;

            reg = Registry.CurrentUser.OpenSubKey(search_scope, true);

            if (reg == null)
            {
                Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + search_scope + " return's null even though the system is at 64bit. Using the regular key now");
                SoftwareKey = "SOFTWARE";
                search_scope = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\SearchScopes";
                Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Trying to open the key: " + search_scope);
                reg = Registry.CurrentUser.OpenSubKey(search_scope, true);
            }
            //

            if (reg != null)
            {
                List<string> search_scopes_guid = new List<string>(reg.GetSubKeyNames());
                string bing_search_scope_guid = string.Empty;

                Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Enumerating search scope guid");
                foreach (string se in search_scopes_guid)
                {
                    Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "opening search_scope: " + se);
                    RegistryKey reg2 = reg.OpenSubKey(se, true);

                    if (reg2.GetValue("DisplayName").ToString().Contains("InternetHelper"))
                    {
                        Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Deleting search scope: " + se);
                        //if(
                        reg.DeleteSubKey(se);

                        Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Deleting search scope in LocalMachine: " + se);
                        RegistryKey reg3 = Registry.CurrentUser.OpenSubKey(search_scope, true);
                        if(reg3.OpenSubKey(se) != null)
                        {
                            reg3.DeleteSubKey(se);
                        }

                    }
                    else if (reg2.GetValue("URL").ToString().Contains("bing"))
                    {
                        Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Bing search scope found");
                        bing_search_scope_guid = se;
                    }
                }

                Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Setting default search scope: " + bing_search_scope_guid);
                reg.SetValue("DefaultScope", bing_search_scope_guid);

                Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + "Done RemoveSearchEngine()");
            }
            else
            {
                Logger.i.AddLog(this.ToString() + ".RemoveSearchEngine()" + "> " + search_scope + " is null!");
            }
        }

        public void CleanSettings()
        {
            _stat.Status = "Setting default home page";

            string SoftwareKey = "SOFTWARE";
            string main = string.Empty;

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            main = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\Main";

            Logger.i.AddLog("Opening subkey: " + main);
            RegistryKey reg = Registry.CurrentUser.OpenSubKey(main, true);

            string start_page = Registry_GetDefaultPage();
            Logger.i.AddLog("Setting default start page: " + start_page);
            reg.SetValue("Start Page", start_page, RegistryValueKind.String); //"http://go.microsoft.com/fwlink/?LinkId=54896");

            Logger.i.AddLog("Done CleanSettings()");
        }

        public int GetOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }

        public void RemoveExtension()
        {
            
        }

        public string GetDefaultPage()
        {
            string ret = string.Empty;

            string SoftwareKey = "SOFTWARE";
            string main = string.Empty;

            int osarch = GetOSArchitecture();

            //if (osarch == 64)
            //{
            //    SoftwareKey = "SOFTWARE\\Wow6432Node";
            //}

            main = SoftwareKey + "\\" + "Microsoft\\Internet Explorer\\Main";

            RegistryKey key = Registry.CurrentUser.OpenSubKey(main);

            object startpage = key.GetValue("Start Page");

            ret = startpage.ToString();

            return ret;
        }

        public string Registry_GetDefaultPage()
        {
            string ret = string.Empty;
            string SoftwareKey = "SOFTWARE";
            string main = string.Empty;

            int osarch = GetOSArchitecture();

            if (osarch == 64)
            {
                SoftwareKey = "SOFTWARE\\Wow6432Node";
            }

            main = SoftwareKey + "\\" + "AirSoftware\\InternetHelper";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(main, true);
            ret = key.GetValue("ie_default_page").ToString();

            return ret;
        }
    }
}
