using ConduitRemover.Logics.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;

namespace ConduitRemover.Logics.Remover
{
    public class Firefox : iRemover
    {
        iStatus _stat;
        public event OnDoneRemovingHandler DoneRemoving;

        private string _DefaultProfile = string.Empty;
        public string DefaultProfile
        {
            get { return _DefaultProfile; }
            set
            {
                if (_DefaultProfile != value)
                {
                    _DefaultProfile = value;
                }
            }
        }

        public Firefox()
        {
            
        }
        static readonly Firefox i = new Firefox();
        public static Firefox I { get { return i; } }

        public void InitStatus(iStatus stat)
        {
            _stat = stat;
            _stat.MaxWork = 4;
            _stat.CurrentWorkIndex = 0;
        }

        public string GetDefaultProfile()
        {
            string ret = string.Empty;

            string appdata_roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string profilepath = Path.Combine(appdata_roaming, "Mozilla\\Firefox");
            string profilefile = Path.Combine(profilepath, "profiles.ini");

            if (File.Exists(profilefile))
            {
                Logger.i.AddLog("Reading ini file: " + profilefile);
                string setting = string.Empty;
                using (StreamReader reader = new StreamReader(profilefile))
                {
                    setting = reader.ReadToEnd();
                }
                string[] perline = setting.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                Logger.i.AddLog("Checking profile path");
                string profile_folder = string.Empty;

                //var a = perline.Where(x => x.Contains("Path=")).ToList();
                string path = string.Empty;

                foreach (string a in perline)
                {
                    if (a.Contains("Path="))
                    {
                        path = a;
                    }
                }

                //if (a.Count > 0)
                {
                    profile_folder = path.Substring(path.IndexOf("=") + 1);
                    ret = Path.Combine(profilepath, profile_folder.Replace("/", "\\"));

                    Logger.i.AddLog("Current firefox profile path: " + ret);
                }
            }

            return ret;
        }

        public bool IsConduitInstalled()
        {
            bool ret = false;

            Logger.i.AddLog("Checking conduit installation in Firefox");

            string default_profile = GetDefaultProfile();
            string extension_folder = Path.Combine(default_profile, "extensions");
            List<string> files = IOHelper.I.GetFilesRecursive(extension_folder, new List<string>() { ".rdf" });

            foreach (string rdf_file in files)
            {
                FileInfo fi = new FileInfo(rdf_file);

                if (fi.Name.ToLower() == "install.rdf")
                {
                    Logger.i.AddLog("Found: " + fi.FullName);
                    string install_rdf = string.Empty;
                    using (StreamReader reader = new StreamReader(fi.FullName))
                    {
                        install_rdf = reader.ReadToEnd();
                    }

                    Logger.i.AddLog("Checking if from conduit...");
                    if (install_rdf.Contains("conduit"))
                    {
                        Logger.i.AddLog("Conduit found! Flagging this extension " + fi.Directory.FullName);

                        ret = true;
                    }
                }
            }

            Logger.i.AddLog("DONE - Checking conduit installation in Firefox");

            return ret;
        }

        public void StartCleaning()
        {
            // make sure to kill all running firefox browsers
            this._stat.Status = "Checking Firefox profile";
            
            this.DefaultProfile = GetDefaultProfile();

            if (Directory.Exists(this.DefaultProfile))
            {
                Logger.i.AddLog("Staring to clean firefox for conduit extension, plugin, and default browser");

                this.RemoveExtension();
                this.RemoveSearchEngine();
                this.CleanSettings();

                if (DoneRemoving != null)
                {
                    this._stat.Status = "Done!";
                    DoneRemoving(this, new EventArgs());
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("profile diretory does not exists");
            }
        }

        public void RemoveExtension()
        {
            this._stat.Status = "Removing extension";

            string extension_folder = Path.Combine(this.DefaultProfile, "extensions");

            if (Directory.Exists(extension_folder))
            {
                string install_rdf = string.Empty;

                Logger.i.AddLog("looking for install.rdf files in extensions directory");
                List<string> files = IOHelper.I.GetFilesRecursive(extension_folder, new List<string>() { ".rdf" });

                List<string> FlaggedExtension = new List<string>();

                foreach (string rdf_file in files)
                {
                    FileInfo fi = new FileInfo(rdf_file);

                    if (fi.Name.ToLower() == "install.rdf")
                    {
                        Logger.i.AddLog("Found: " + fi.FullName);
                        using (StreamReader reader = new StreamReader(fi.FullName))
                        {
                            install_rdf = reader.ReadToEnd();
                        }

                        Logger.i.AddLog("Checking if from conduit...");
                        if (install_rdf.Contains("conduit"))
                        {
                            Logger.i.AddLog("Conduit found! Flagging this extension " + fi.Directory.FullName);

                            FlaggedExtension.Add(fi.Directory.FullName);
                        }
                    }
                }

                Logger.i.AddLog("Delete all conduit files");
                files.Clear();
                foreach (string folder in FlaggedExtension)
                {
                    files = IOHelper.I.GetFilesRecursive(folder, new List<string>());
                    Logger.i.AddLog("Deleting extensions: " + folder);
                    foreach (string file in files)
                    {
                        //Logger.i.AddLog("Deleting files: " + Path.GetFileName(file));
                        // TODO: delete actual files
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex)
                        {
                            Logger.i.AddLog("Unable to delete file: " + file + "\r\nError said: " + ex.Message);
                        }
                    }
                }
            }

            Logger.i.AddLog("Extensions removed");
        }

        public void RemoveSearchEngine()
        {
            this._stat.Status = "Removing Conduit Search Engine";

            Logger.i.AddLog("Removing conduit search engine in searchplugins folder");
            string searchengine = Path.Combine(this.DefaultProfile, "searchplugins");
            if (Directory.Exists(searchengine))
            {
                string conduit = Path.Combine(searchengine, "conduit.xml");

                if (File.Exists(conduit))
                {
                    Logger.i.AddLog("Deleting conduit.xml");
                    File.Delete(conduit);
                    Logger.i.AddLog("conduit.xml removed");
                }
            }
            Logger.i.AddLog("Done removing search engine");
        }

        public void CleanSettings()
        {
            this._stat.Status = "Cleaning settings";

            Logger.i.AddLog("Cleaning prefs.js");

            string prefs_js = Path.Combine(this.DefaultProfile, "prefs.js");

            if (File.Exists(prefs_js))
            {
                Logger.i.AddLog("Reading prefs.js");
                using (StreamReader reader = new StreamReader(prefs_js))
                {
                    prefs_js = reader.ReadToEnd();
                }

                List<string> prefs = new List<string>();
                prefs.AddRange(prefs_js.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

                Logger.i.AddLog("Restore smartbar.originalHomepage value");
                string original_homepage = string.Empty;
                original_homepage = prefs.Find(a => a.Contains("smartbar.originalHomepage"));
                if (original_homepage != null)
                {
                    //original_homepage = GetUserPrefsValue("smartbar.originalHomepage", original_homepage);
                    original_homepage = Registry_GetDefaultPage();

                    string browser_startup_homepage = string.Empty;
                    browser_startup_homepage = prefs.Find(a => a.Contains("\"browser.startup.homepage\""));
                    browser_startup_homepage = SetUserPrefsValue("browser.startup.homepage", original_homepage, browser_startup_homepage);
                    
                    int index = prefs.FindIndex(a => a.Contains("\"browser.startup.homepage\""));
                    prefs[index] = browser_startup_homepage;
                }

                Logger.i.AddLog("Restore original browser.search.selectedEngine value");
                string smartbar_originalSearchEngine = prefs.Find(a => a.Contains("\"smartbar.originalSearchEngine\""));
                if (smartbar_originalSearchEngine != null)
                {
                    string smartbar_originalSearchEngine_value = GetUserPrefsValue("smartbar.originalSearchEngine", smartbar_originalSearchEngine);
                    //if (smartbar_originalSearchEngine_value != string.Empty)
                    {
                        string browser_search_selectedEngine = prefs.Find(a => a.Contains("\"browser.search.selectedEngine\""));
                        string browser_search_selectedEngine_value = SetUserPrefsValue("browser.search.selectedEngine", smartbar_originalSearchEngine_value, browser_search_selectedEngine);

                        int index = prefs.FindIndex(a => a.Contains("\"browser.search.selectedEngine\""));
                        prefs[index] = browser_search_selectedEngine_value;
                    }
                }

                Logger.i.AddLog("Remove all Conduit settings");
                List<string> remove = new List<string>();
                foreach (string p in prefs)
                {
                    if (p.Contains("user_pref(\"CT"))
                    {
                        remove.Add(p);
                    }
                    else if (p.ToLower().Contains("user_pref(\"smartbar"))
                    {
                        remove.Add(p);
                    }
                    else if (p.Contains("user_pref(\"browser.search.defaultthis.engineName\""))
                    {
                        remove.Add(p);
                    }
                    else if (p.Contains("user_pref(\"browser.search.defaulturl\""))
                    {
                        remove.Add(p);
                    }
                    else if (p.Contains("user_pref(\"keyword.URL\""))
                    {
                        remove.Add(p);
                    }
                }
                foreach (string r in remove)
                {
                    prefs.Remove(r);
                }

                // save new prefs.js
                prefs_js = Path.Combine(this.DefaultProfile, "prefs.js");
                //System.Windows.Forms.MessageBox.Show(this.DefaultProfile);
                //System.Windows.Forms.MessageBox.Show(prefs_js);bup
                Logger.i.AddLog("Writing new prefs.js " + prefs_js);
                using (StreamWriter writer = new StreamWriter(prefs_js))
                {
                    writer.Write(string.Join("\r\n", prefs.ToArray()));
                }
            }

            Logger.i.AddLog("Done cleaning prefs.js");
        }
        
        string GetUserPrefsValue(string name, string userpref)
        {
            string ret = string.Empty;

            // user_pref("smartbar.originalHomepage", "about:home");

            string user_pref = "user_pref(\"{0}\", \"{1}\");";
            user_pref = userpref;
            user_pref = user_pref.Replace("user_pref(\"" + name + "\", \"", string.Empty);
            user_pref = user_pref.Replace("\");", string.Empty);

            ret = user_pref;

            return ret;
        }

        string SetUserPrefsValue(string name, string value, string userpref)
        {
            string ret = string.Empty;

            // user_pref("browser.startup.homepage", "about:blank");

            string user_pref = "user_pref(\"{0}\", \"{1}\");";
            user_pref = userpref;
            user_pref = user_pref.Replace("user_pref(\"" + name + "\", \"", string.Empty);
            string user_pref_value = user_pref.Replace("\");", string.Empty);
            user_pref = userpref;
            user_pref = user_pref.Replace(user_pref_value, value);

            ret = user_pref;

            return ret;
        }

        public string GetDefaultPage()
        {
            string ret = string.Empty;

            string defaultprofile = GetDefaultProfile();

            string prefs_js = Path.Combine(defaultprofile, "prefs.js");

            if (File.Exists(prefs_js))
            {
                Logger.i.AddLog("Reading prefs.js");
                using (StreamReader reader = new StreamReader(prefs_js))
                {
                    prefs_js = reader.ReadToEnd();
                }

                List<string> prefs = new List<string>();
                prefs.AddRange(prefs_js.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));

                foreach (string pref in prefs)
                {
                    if (pref.Contains("user_pref(\"browser.startup.homepage\""))
                    {
                        ret = GetUserPrefsValue("browser.startup.homepage", pref);

                        break;
                    }
                }
            }

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
            ret = key.GetValue("ff_default_page").ToString();

            return ret;
        }

        public int GetOSArchitecture()
        {
            string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
        }
    }
}
