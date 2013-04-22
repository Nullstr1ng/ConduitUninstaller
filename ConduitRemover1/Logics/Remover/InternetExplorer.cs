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

            try
            {
                string[] keys = {
                                @"SOFTWARE\Microsoft\Internet Explorer\Toolbar",
                                @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Toolbar",
                            };

                string clsid = string.Empty;

                foreach (string key in keys)
                {
                    Logger.i.AddLog("trying to open key " + key);
                    RegistryKey reg = null;
                    reg = Registry.LocalMachine.OpenSubKey(key, true);

                    if (reg == null)
                    {
                        Logger.i.AddLog("key " + key + " returns null.. continue with the next key.");
                        continue;
                    }

                    Logger.i.AddLog("enumerating ValueNames");
                    List<string> nvp = new List<string>(reg.GetValueNames());
                    string name = string.Empty;
                    foreach (string entry in nvp)
                    {
                        Logger.i.AddLog("reading " + entry + " value");
                        string value = string.Empty;
                        {
                            value = reg.GetValue(entry).ToString();
                        }
                        Logger.i.AddLog(entry + " value: " + value);

                        Logger.i.AddLog("is value equals to 'InternetHelper3 Toolbar'? " + (value = "InternetHelper3 Toolbar").ToString());
                        if (value == "InternetHelper3 Toolbar")
                        {
                            name = entry;
                            clsid = entry;

                            Logger.i.AddLog("deleting this toolbar now");
                            //if (!string.IsNullOrEmpty(name))
                            {
                                reg.DeleteValue(name);
                            }

                            break;
                        }
                    }

                    Logger.i.AddLog("delete toolbar entry in HKCR\\CLSID");
                    reg = Registry.ClassesRoot.OpenSubKey("CLSID", true);
                    try
                    {
                        reg.DeleteSubKeyTree(clsid);
                    }
                    catch (Exception ex)
                    {
                        Logger.i.AddLog("something went wrong while deleting the toolbar entry in HKCR\\CLSID\\" + clsid);
                        Logger.i.AddLog("it said: " + ex.Message);
                    }
                }

                Logger.i.AddLog("done cleaning Toolbars");
            }
            catch (Exception ex)
            {
                Logger.i.AddLog("something went wrong while removing toolbar in RemoveToolbar() method");
                Logger.i.AddLog("it said: " + ex.Message);
            }
        }

        public void RemoveSearchEngine()
        {
            _stat.Status = "Removing InternetHelper search engine";

            try
            {
                string[] keys = {
                                @"HKCU\Software\Microsoft\Internet Explorer\SearchScopes",
                                @"HKCU\Software\Wow6432Node\Microsoft\Internet Explorer\SearchScopes",
                                @"HKLM\Software\Microsoft\Internet Explorer\SearchScopes",
                                @"HKLM\Software\Wow6432Node\Microsoft\Internet Explorer\SearchScopes"
                            };

                foreach (string key in keys)
                {
                    RegistryKey reg = Registry.CurrentUser;

                    Logger.i.AddLog("trying to open key " + key);

                    if (key.Substring(0, 4) == "HKCU")
                    {
                        reg = Registry.CurrentUser.OpenSubKey(key.Replace("HKCU\\", string.Empty), true);
                    }
                    else if (key.Substring(0, 4) == "HKLM")
                    {
                        reg = Registry.LocalMachine.OpenSubKey(key.Replace("HKLM\\", string.Empty), true);
                    }

                    if (reg == null)
                    {
                        Logger.i.AddLog("key " + key + " returns null.. continue with the next key.");
                        continue;
                    }

                    Logger.i.AddLog("enumerating subkey names");
                    List<string> nvp = new List<string>(reg.GetSubKeyNames());
                    string name = string.Empty;
                    foreach (string entry in nvp)
                    {
                        Logger.i.AddLog("trying to open subkey " + entry);
                        RegistryKey searchscope = reg.OpenSubKey(entry, true);

                        if (searchscope == null)
                        {
                            Logger.i.AddLog("key " + key + " returns null.. continue with the next subkey.");
                            continue;
                        }

                        string value = searchscope.GetValue("DisplayName").ToString();
                        Logger.i.AddLog("Displayname value: " + value);

                        if (value.Contains("InternetHelper3"))
                        {
                            reg.DeleteSubKeyTree(entry);
                        }
                        else if (value.Contains("Bing"))
                        {
                            reg.SetValue("DefaultScope", entry);
                        }
                    }
                }

                Logger.i.AddLog("done cleaning search scopes");
            }
            catch (Exception ex)
            {
                Logger.i.AddLog("something went wrong while removing search scopes in RemoveSearchScope() method");
                Logger.i.AddLog("it said: " + ex.Message);
            }
        }

        public void CleanSettings()
        {
            _stat.Status = "Setting default home page";

            try
            {
                string[] keys = {
                                @"SOFTWARE\Microsoft\Internet Explorer\Main",
                                @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Main",
                            };

                foreach (string key in keys)
                {
                    Logger.i.AddLog("trying to open key " + key);
                    RegistryKey reg = null;
                    reg = Registry.CurrentUser.OpenSubKey(key, true);

                    if (reg == null)
                    {
                        Logger.i.AddLog("key " + key + " returns null.. continue with the next key.");
                        continue;
                    }

                    string defultpage = Registry_GetDefaultPage();
                    Logger.i.AddLog("Restoring back startup page to its default value: " + defultpage);
                    reg.SetValue("Start Page", defultpage);
                }

                Logger.i.AddLog("done cleaning start page");
            }
            catch (Exception ex)
            {
                Logger.i.AddLog("something went wrong while restoring back the original start page in RestoreDefaultStartupPage() method");
                Logger.i.AddLog("it said: " + ex.Message);
            }
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
