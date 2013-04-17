using ConduitRemover.Logics;
using ConduitRemover.Logics.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace ChromeTest
{
    class Program
    {
        static iStatus _stat;

        private static string _DefaultProfile = string.Empty;
        static string DefaultProfile
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

        static void Main(string[] args)
        {
            _stat = new TheStatus();

            DefaultProfile = AppDomain.CurrentDomain.BaseDirectory;
            CleanSettings();
            RemoveSearchEngine();

            Console.WriteLine("done...............");
            Console.ReadLine();
        }

        static string Registry_GetDefaultPage()
        {
            return "http://www.google.com/";
        }

        static void CleanSettings()
        {
            _stat.Status = "Cleaning settings";

            Console.WriteLine("Opening Preferences file");
            string pref = Path.Combine(DefaultProfile, "Preferences.txt");
            string _pref = string.Empty;

            if (!File.Exists(pref)) { return; }

            using (StreamReader reader = new StreamReader(pref))
            {
                _pref = reader.ReadToEnd();
            }

            // REFORMAT JSON.DATA
            JObject json = JObject.Parse(_pref);

            Console.WriteLine("REFORMAT JSON.DATA");
            {
                _pref = json.ToString();

                // create temporary copy
                using (StreamWriter writer = new StreamWriter(pref + "_reformatted.txt"))
                {
                    writer.Write(_pref);
                }
            }
            Console.WriteLine("done");

            #region // udpate extension token
            Console.WriteLine("UPDATE extension token");
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
                            Console.WriteLine("BLACKLIST InternetHelper3 Toolbar extension");
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

                    Console.WriteLine("udpate newtab override");
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
            Console.WriteLine("done");
            #endregion

            #region // update session token
            var session = json["session"];
            if (session != null)
            {
                Console.WriteLine("update session token");
                session a = new session()
                {
                    urls_to_restore_on_startup = new List<string>()
                    {
                        Registry_GetDefaultPage()
                    }
                };
                var b = JObject.FromObject(a);
                session.Replace(b);
                Console.WriteLine("done");
            }
            #endregion

            #region // update homepage token
            {
                var homepage = json["homepage"];
                if (homepage != null)
                {
                    Console.WriteLine("update homepage token");
                    //var a = JObject.FromObject(Registry_GetDefaultPage());
                    json["homepage"].Replace(Registry_GetDefaultPage());
                    Console.WriteLine("done");
                }
            }
            #endregion

            // UPDATE
            Console.WriteLine("Updating Preferences file");
            {
                using (StreamWriter writer = new StreamWriter(pref + "_new.txt"))
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

        static void RemoveSearchEngine()
        {
            _stat.Status = "Removing Conduit Search Engine";

            Console.WriteLine("Connecting to Web Data SQLite database");
            using (SQLiteConnection conn = new SQLiteConnection())
            {
                SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = Path.Combine(DefaultProfile, "Web Data");

                conn.ConnectionString = builder.ConnectionString;
                conn.Open();

                using (SQLiteCommand comm = new SQLiteCommand())
                {
                    comm.Connection = conn;

                    Console.WriteLine("Deleting Conduit from keywords table");
                    comm.CommandText = "delete from keywords where short_name='Conduit'";
                    comm.ExecuteNonQuery();

                    try
                    {
                        Console.WriteLine("Deleting Conduit from keywords_backup table");
                        comm.CommandText = "delete from keywords_backup where short_name='Conduit'";
                        comm.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed deleting stuff in keywords_backup table");
                    }

                    Console.WriteLine("Updating default search engine");
                    comm.CommandText = "update meta set value='2' where key='Default Search Provider ID'";
                    comm.ExecuteNonQuery();

                    Console.WriteLine("Updating default search engine backup");
                    comm.CommandText = "update meta set value='2' where key='Default Search Provider ID Backup'";
                    comm.ExecuteNonQuery();
                }
            }
        }

        class TheStatus : iStatus
        {
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
                    Console.WriteLine(_status);
                }
            }


            public int MaxWork
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

            public int CurrentWorkIndex
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
        }
    }
}
