using ConduitRemover.Logics.Common;
using ConduitRemover.Logics.Remover;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ConduitRemover
{
    static class Program
    {
        //public static Firefox firefox = new Firefox();
        //public static Chrome chrome = new Chrome();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.i.Hey();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            List<string> args = new List<string>(Environment.GetCommandLineArgs());
            //args.RemoveAt(0);

            if (args.Count > 1)
            {
                args[1] = args[1].ToLower();

                if (args[1] == "-uninstall_ff") // obsolete, unless required
                {
                    Application.Run(new UninstallerWindow()
                    {
                        Parameter = args[1],
                        WindowTitle = "Conduit Uninstaller for Firefox",
                        WindowSubTitle = "Uninstalling Conduit for Firefox"
                    });
                }
                else if (args[1] == "-uninstall_ch") // obsolete, unless required
                {
                    Application.Run(new UninstallerWindow()
                    {
                        Parameter = args[1],
                        WindowTitle = "Conduit Uninstaller for Chrome",
                        WindowSubTitle = "Uninstalling Conduit for Chrome"
                    });
                }
                else if (args[1] == "-uninstall_ie") // obsolete, unless required
                {
                    Application.Run(new UninstallerWindow()
                    {
                        Parameter = args[1],
                        WindowTitle = "Conduit Uninstaller for Internet Explorer",
                        WindowSubTitle = "Uninstalling Conduit for Internet Explorer"
                    });
                }
                else if (args[1] == "-prepareinstall")
                {
                    Application.Run(new PrepareInstall()
                        //{
                        //    Parameter = args[1],
                        //    WindowTitle = "Conduit Uninstaller for Internet Explorer",
                        //    WindowSubTitle = "Uninstalling Conduit for Internet Explorer"
                        //});
                    );
                }
            }
            else
            {
                //Application.Run(new Form1());
                Application.Run(new UninstallerAll());
            }
        }
    }
}
