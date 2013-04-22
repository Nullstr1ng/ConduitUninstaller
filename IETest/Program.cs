using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace IETest
{
    class Program
    {
        public static void AddLog(string log)
        {
            Console.WriteLine(log);
        }

        static void Main(string[] args)
        {
            RemoveToolbar2();
            RemoveSearchScope();
            RestoreDefaultStartupPage();

            Console.WriteLine("done");
            Console.ReadLine();
        }

        public static void RemoveToolbar2()
        {
            try
            {
                string[] keys = {
                                @"SOFTWARE\Microsoft\Internet Explorer\Toolbar",
                                @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Toolbar",
                            };

                string clsid = string.Empty;

                foreach (string key in keys)
                {
                    AddLog("trying to open key " + key);
                    RegistryKey reg = null;
                    reg = Registry.LocalMachine.OpenSubKey(key, true);

                    if (reg == null)
                    {
                        AddLog("key " + key + " returns null.. continue with the next key.");
                        continue;
                    }

                    AddLog("enumerating ValueNames");
                    List<string> nvp = new List<string>(reg.GetValueNames());
                    string name = string.Empty;
                    foreach (string entry in nvp)
                    {
                        AddLog("reading " + entry + " value");
                        string value = string.Empty;
                        {
                            value = reg.GetValue(entry).ToString();
                        }
                        AddLog(entry + " value: " + value);

                        AddLog("is value equals to 'InternetHelper3 Toolbar'? " + (value = "InternetHelper3 Toolbar").ToString());
                        if (value == "InternetHelper3 Toolbar")
                        {
                            name = entry;
                            clsid = entry;

                            AddLog("deleting this toolbar now");
                            //if (!string.IsNullOrEmpty(name))
                            {
                                reg.DeleteValue(name);
                            }

                            break;
                        }
                    }

                    AddLog("delete toolbar entry in HKCR\\CLSID");
                    reg = Registry.ClassesRoot.OpenSubKey("CLSID", true);
                    try
                    {
                        reg.DeleteSubKeyTree(clsid);
                    }
                    catch (Exception ex)
                    {
                        AddLog("something went wrong while deleting the toolbar entry in HKCR\\CLSID\\" + clsid);
                        AddLog("it said: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog("something went wrong while removing toolbar in RemoveToolbar() method");
                AddLog("it said: " + ex.Message);
            }
        }

        public static void RemoveSearchScope()
        {
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

                    AddLog("trying to open key " + key);

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
                        AddLog("key " + key + " returns null.. continue with the next key.");
                        continue;
                    }

                    AddLog("enumerating subkey names");
                    List<string> nvp = new List<string>(reg.GetSubKeyNames());
                    string name = string.Empty;
                    foreach (string entry in nvp)
                    {
                        AddLog("trying to open subkey " + entry);
                        RegistryKey searchscope = reg.OpenSubKey(entry, true);

                        if (searchscope == null)
                        {
                            AddLog("key " + key + " returns null.. continue with the next subkey.");
                            continue;
                        }

                        string value = searchscope.GetValue("DisplayName").ToString();
                        AddLog("Displayname value: " + value);

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
            }
            catch (Exception ex)
            {
                AddLog("something went wrong while removing search scopes in RemoveSearchScope() method");
                AddLog("it said: " + ex.Message);
            }
        }

        public static void RestoreDefaultStartupPage()
        {
            try
            {
                string[] keys = {
                                @"SOFTWARE\Microsoft\Internet Explorer\Main",
                                @"SOFTWARE\Wow6432Node\Microsoft\Internet Explorer\Main",
                            };

                foreach (string key in keys)
                {
                    AddLog("trying to open key " + key);
                    RegistryKey reg = null;
                    reg = Registry.CurrentUser.OpenSubKey(key, true);

                    if (reg == null)
                    {
                        AddLog("key " + key + " returns null.. continue with the next key.");
                        continue;
                    }

                    string defultpage = Registry_GetDefaultPage();
                    AddLog("Restoring back startup page to its default value: " + defultpage);
                    reg.SetValue("Start Page", defultpage);
                }
            }
            catch (Exception ex)
            {
                AddLog("something went wrong while restoring back the original start page in RestoreDefaultStartupPage() method");
                AddLog("it said: " + ex.Message);
            }
        }

        public static string Registry_GetDefaultPage()
        {
            return "http://www.google.com";
        }
    }
}
