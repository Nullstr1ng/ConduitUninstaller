/*
 * search engine location
 * http://productforums.google.com/forum/#!topic/chrome/Y4mF14pmXGQ
 * http://productforums.google.com/forum/#!category-topic/chrome/give-feature-feedback-and-suggestions/ElfXYc48cCo
 */
using ConduitRemover.Logics.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data.SQLite;
using Microsoft.Win32;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConduitRemover.Logics.Remover
{
    public class Chrome
    {
        iStatus _stat;
        public event OnDoneRemovingHandler DoneRemoving;

        public Chrome()
        {
        }
        static readonly Chrome i = new Chrome();
        public static Chrome I { get { return i; } }

        public void InitStatus(iStatus stat)
        {
            _stat = stat;
            _stat.MaxWork = 1;
            _stat.CurrentWorkIndex = 0;
        }

        public void GetProfiles()
        {
            this._stat.Status = "Checking Chrome profiles";

            Logger.i.AddLog("Checking google chrome profiles");
            string app_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string google_user_data_folder = Path.Combine(app_local, "Google\\Chrome\\User Data");

            List<string> files = IOHelper.I.GetFilesRecursive(google_user_data_folder, new List<string>());
            List<string> ProfileFolders = new List<string>();

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Name == "Preferences")
                {
                    Logger.i.AddLog("Google Chrome Profile found: " + fi.Directory.FullName);
                    ProfileFolders.Add(fi.Directory.FullName);
                    _stat.MaxWork += 3;
                }
            }

            foreach (string prof in ProfileFolders)
            {
                ChromeProfiles cp = new ChromeProfiles();
                cp.DefaultProfile = prof;
                cp.InitStatus(this._stat);
                cp.StartCleaning();
            }

            if (DoneRemoving != null)
            {
                DoneRemoving(this, new EventArgs());
            }
        }

        public bool IsConduitInstalled()
        {
            bool ret = false;

            Logger.i.AddLog("Checking if conduit is installed in Chrome");
            Logger.i.AddLog("Checking google chrome profiles");
            string app_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string google_user_data_folder = Path.Combine(app_local, "Google\\Chrome\\User Data");

            List<string> files = IOHelper.I.GetFilesRecursive(google_user_data_folder, new List<string>());

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Name == "Preferences")
                {
                    string _pref = string.Empty;

                    using (StreamReader reader = new StreamReader(fi.FullName))
                    {
                        _pref = reader.ReadToEnd();
                    }

                    if (_pref.ToLower().Contains("search.conduit.com"))
                    {
                        ret = true;
                    }
                }
            }
            Logger.i.AddLog("DONE - Checking if conduit is installed in Chrome");

            return ret;
        }

        public string GetDefaultPage()
        {
            string ret = string.Empty;

            string app_local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string google_user_data_folder = Path.Combine(app_local, "Google\\Chrome\\User Data");

            List<string> files = IOHelper.I.GetFilesRecursive(google_user_data_folder, new List<string>());
            List<string> ProfileFolders = new List<string>();

            string default_profile = string.Empty;

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);

                if (fi.Name == "Preferences")
                {
                    if (fi.Directory.Name == "Default")
                    {
                        default_profile = fi.Directory.FullName;
                        break;
                    }

                    //ProfileFolders.Add(fi.Directory.FullName);
                }
            }

            //foreach (string prof in ProfileFolders)
            //{
            //    if (prof == "Default")
            //    {
            //        default_profile = prof;
            //        break;
            //    }
            //}

            string pref = Path.Combine(default_profile, "Preferences");
            string pref_contents = string.Empty;

            using (StreamReader reader = new StreamReader(pref))
            {
                pref_contents = reader.ReadToEnd();
            }

            string[] content_array = pref_contents.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (string line in content_array)
            {
                if (line.Contains("\"urls_to_restore_on_startup\":"))
                {
                    string a = line; // "\"urls_to_restore_on_startup\": [ \"http://pttech.azurewebsites.net/\", \"http://www.google.com/\", \"http://www.yahoo.com\" ]";
                    string b = a.Replace("\"urls_to_restore_on_startup\":", string.Empty);
                    b = b.Trim();

                    ret = b;

                    break;
                }
            }

            return ret;
        }

        public class ChromeProfiles : iRemover
        {
            iStatus _stat;

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

            public string GetDefaultProfile()
            {
                throw new NotImplementedException();
            }

            public void StartCleaning()
            {
                RemoveExtension();
                RemoveSearchEngine();
                CleanSettings();
            }

            public void InitStatus(iStatus stat)
            {
                _stat = stat;
            }

            public void RemoveExtension()
            {
                
            }

            public void RemoveSearchEngine()
            {
                this._stat.Status = "Removing Conduit Search Engine";

                Logger.i.AddLog("Connecting to Web Data SQLite database");
                using (SQLiteConnection conn = new SQLiteConnection())
                {
                    SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
                    builder.DataSource = Path.Combine(this.DefaultProfile, "Web Data");

                    conn.ConnectionString = builder.ConnectionString;
                    conn.Open();

                    using (SQLiteCommand comm = new SQLiteCommand())
                    {
                        comm.Connection = conn;

                        Logger.i.AddLog("Deleting Conduit from keywords table");
                        comm.CommandText = "delete from keywords where short_name='Conduit'";
                        comm.ExecuteNonQuery();

                        try
                        {
                            Logger.i.AddLog("Deleting Conduit from keywords_backup table");
                            comm.CommandText = "delete from keywords_backup where short_name='Conduit'";
                            comm.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Logger.i.AddLog("Failed deleting stuff in keywords_backup table");
                        }

                        Logger.i.AddLog("Updating default search engine");
                        comm.CommandText = "update meta set value='2' where key='Default Search Provider ID'";
                        comm.ExecuteNonQuery();

                        Logger.i.AddLog("Updating default search engine backup");
                        comm.CommandText = "update meta set value='2' where key='Default Search Provider ID Backup'";
                        comm.ExecuteNonQuery();

                        //SQLiteDataReader reader = comm.ExecuteReader();

                        //if (reader.HasRows)
                        //{
                        //    while (reader.Read())
                        //    {
                        //        Debug.WriteLine("short name: " + reader.GetString(reader.GetOrdinal("short_name")));
                        //    }
                        //}
                    }
                }
            }

            public void CleanSettings()
            {
                _stat.Status = "Cleaning settings";

                Logger.i.AddLog("Opening Preferences file");
                string pref = Path.Combine(DefaultProfile, "Preferences");
                string _pref = string.Empty;

                if (!File.Exists(pref)) { return; }

                using (StreamReader reader = new StreamReader(pref))
                {
                    _pref = reader.ReadToEnd();
                }

                // REFORMAT JSON.DATA
                JObject json = JObject.Parse(_pref);

                Logger.i.AddLog("REFORMAT JSON.DATA");
                {
                    _pref = json.ToString();

                    // create temporary copy
                    using (StreamWriter writer = new StreamWriter(pref + "_reformatted.txt"))
                    {
                        writer.Write(_pref);
                    }
                }
                Logger.i.AddLog("done");

                #region // udpate extension token
                Logger.i.AddLog("UPDATE extension token");
                {
                    // just make sure to go through each token
                    var extensions = json["extensions"];
                    if (extensions != null)
                    {
                        // extension settings
                        {
                            var settings = extensions["settings"];
                            if (settings != null)
                            {
                                Logger.i.AddLog("BLACKLIST InternetHelper3 Toolbar extension");
                                {
                                    var token = settings["pnjnnnhampgflieglcelomcofocioegp"];
                                    if (token != null)
                                    {
                                        // conn.Remove(); since we can't do Removal.. Then just blacklist it
                                        pnjnnnhampgflieglcelomcofocioegp a = new pnjnnnhampgflieglcelomcofocioegp()
                                        {
                                            blacklist = true
                                        };
                                        var b = JObject.FromObject(a);
                                        token.Replace(b);
                                    }
                                }
                            }
                        }

                        Logger.i.AddLog("udpate newtab override");
                        {
                            var token = extensions["chrome_url_overrides"];
                            if (token != null)
                            {
                                chrome_url_overrides a = new chrome_url_overrides()
                                {
                                    bookmarks = new List<string>()
                                {
                                    "chrome-extension://eemcgdkfndhakfknompkggombfjjjeno/main.html"
                                }
                                };
                                var b = JObject.FromObject(a);
                                token.Replace(b);
                            }
                        }
                    }
                }
                Logger.i.AddLog("done");
                #endregion

                #region // update session token
                var session = json["session"];
                if (session != null)
                {
                    Logger.i.AddLog("update session token");
                    session a = new session()
                    {
                        urls_to_restore_on_startup = new List<string>()
                    {
                        Registry_GetDefaultPage()
                    }
                    };
                    var b = JObject.FromObject(a);
                    session.Replace(b);
                    Logger.i.AddLog("done");
                }
                #endregion

                #region // update homepage token
                {
                    var homepage = json["homepage"];
                    if (homepage != null)
                    {
                        Logger.i.AddLog("update homepage token");
                        //var a = JObject.FromObject(Registry_GetDefaultPage());
                        json["homepage"].Replace(Registry_GetDefaultPage());
                        Logger.i.AddLog("done");
                    }
                }
                #endregion

                // temporary.. remove it soon!
                Logger.i.AddLog("Updating Preferences file");
                {
                    using (StreamWriter writer = new StreamWriter(pref + "_new.txt"))
                    {
                        writer.Write(json.ToString());
                    }
                }

                // UPDATE
                Logger.i.AddLog("Updating Preferences file");
                {
                    using (StreamWriter writer = new StreamWriter(pref))
                    {
                        writer.Write(json.ToString());
                    }
                }
            }

            #region some Chrome's preference objects
            class pnjnnnhampgflieglcelomcofocioegp
            {
                private bool _blacklist = true;
                public bool blacklist
                {
                    get { return _blacklist; }
                    set
                    {
                        if (_blacklist != value)
                        {
                            _blacklist = value;
                        }
                    }
                }
            }

            class chrome_url_overrides
            {
                private List<string> _bookmarks = new List<string>();
                public List<string> bookmarks
                {
                    get { return _bookmarks; }
                    set
                    {
                        if (_bookmarks != value)
                        {
                            _bookmarks = value;
                        }
                    }
                }
            }

            class session
            {
                private int _restore_on_startup = 5;
                public int restore_on_startup
                {
                    get { return _restore_on_startup; }
                    set
                    {
                        if (_restore_on_startup != value)
                        {
                            _restore_on_startup = value;
                        }
                    }
                }

                private bool _restore_on_startup_migrated = true;
                public bool restore_on_startup_migrated
                {
                    get { return _restore_on_startup_migrated; }
                    set
                    {
                        if (_restore_on_startup_migrated != value)
                        {
                            _restore_on_startup_migrated = value;
                        }
                    }
                }

                private List<string> _urls_to_restore_on_startup = new List<string>();
                public List<string> urls_to_restore_on_startup
                {
                    get { return _urls_to_restore_on_startup; }
                    set
                    {
                        if (_urls_to_restore_on_startup != value)
                        {
                            _urls_to_restore_on_startup = value;
                        }
                    }
                }
            }
            #endregion

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
                ret = key.GetValue("gc_default_page").ToString();

                return ret;
            }

            public int GetOSArchitecture()
            {
                string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
                return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
            }
        }
    }
}
