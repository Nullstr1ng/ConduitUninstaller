using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConduitRemover.Logics.Common
{
    public class IOHelper
    {
        public IOHelper()
        {
        }
        static readonly IOHelper i = new IOHelper();
        public static IOHelper I { get { return i; } }

        public List<string> GetFilesRecursive(string b, List<string> allowedextension)
        {
            List<string> result = new List<string>();
            Stack<string> stack = new Stack<string>();

            stack.Push(b);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();

                try
                {
                    //result.AddRange(Directory.GetFiles(dir, "*.*"));

                    foreach (string dn in Directory.GetDirectories(dir))
                    {
                        stack.Push(dn);
                        DirectoryInfo di = new DirectoryInfo(dn);
                    }

                    foreach (string fn in Directory.GetFiles(dir, "*.*"))
                    {
                        FileInfo fi = new FileInfo(fn);
                        if (allowedextension.Count == 0)
                        {
                            result.Add(fi.FullName);
                        }
                        else
                        {
                            if (allowedextension.Contains(fi.Extension.ToLower()))
                            {
                                result.Add(fi.FullName);
                            }
                        }
                    }
                }
                catch
                {
                    ;
                }
            }

            return result;
        }
    }
}
