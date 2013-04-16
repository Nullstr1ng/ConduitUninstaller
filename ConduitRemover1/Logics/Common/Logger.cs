using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
//using System.Collections.Concurrent;
using System.Diagnostics;

namespace ConduitRemover.Logics.Common
{
    public class Logger
    {
        BackgroundWorker bgWorker = new BackgroundWorker();
        Queue<string> q = new Queue<string>();

        string _log_filename = "log";
        public string LogFilename
        {
            get { return this._log_filename; }
            set { this._log_filename = value; }
        }

        public Logger()
        {
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += bgWorker_DoWork;
            bgWorker.RunWorkerCompleted += bgWorker_RunWorkerCompleted;

            string log = "LOG";
            string month = DateTime.Today.Month.ToString();
            string day = DateTime.Today.Day.ToString();
            string year = DateTime.Today.Year.ToString();

            string path = AppDomain.CurrentDomain.BaseDirectory;

            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "ConduitRemover");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, log);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, year);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, month);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            path = Path.Combine(path, day);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            LogFilename = Path.Combine(path, string.Format("log_{0}{1}{2}.txt", month, day, year));
        }
        static readonly Logger _i = new Logger();
        public static Logger i { get { return _i; } }
        public void Hey() { }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log();
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string log = string.Empty;
            bool tried = true; //q.TryDequeue(out log);
            log = q.Dequeue();

            if (tried)
            {
                string log_format = "{0} -----------------------------------\r\n{1}\r\n>";
                log_format = string.Format(log_format, DateTime.Now.ToString("MM/dd/yyyy hh:mm"), log);

                Debug.WriteLine(log_format);

                using (TextWriter writer = File.AppendText(LogFilename))
                {
                    writer.WriteLine(log_format);
                }
            }
        }

        public void AddLog(string log)
        {
            q.Enqueue(log);
            Log();
        }

        void Log()
        {
            if (bgWorker.IsBusy == false)
            {
                if (q.Count != 0)
                {
                    bgWorker.RunWorkerAsync();
                }
            }
        }
    }
}
