using System;
using System.IO;

namespace directoryserver
{
    class Options
    {
        public string Directory;
        public int Port;
        public string Protocol;
        public bool ShowHelp;
        public bool ListenExternal;
        public bool ShutdownOnEnter;

        public Options()
        {
            Directory = ".";
            Port = 8000;
            Protocol = "HTTP";
            ShowHelp = false;
            ListenExternal = false;
            ShutdownOnEnter = true;
        }

        public static int DisplayHelp()
        {
            Console.WriteLine("./directoryserver [-dir directory] [-port ####] [-listen [local|external]] [-noshutdown]");
            return 1;
        }

        public static Options Parse(string[] args)
        {
            var options = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-help", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(args[i], "-?", StringComparison.OrdinalIgnoreCase))
                {
                    options.ShowHelp = true;
                }
                else if (string.Equals(args[i], "-dir", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i < args.Length) options.Directory = Path.GetFullPath(args[i]);
                }
                else if (string.Equals(args[i], "-port", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i < args.Length)
                    {
                        if (Int32.TryParse(args[i], out int port)) options.Port = port;
                    }
                }
                else if (string.Equals(args[i], "-listen", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i < args.Length)
                    {
                        if (string.Equals(args[i], "local", StringComparison.OrdinalIgnoreCase)) options.ListenExternal = false;
                        else if (string.Equals(args[i], "external", StringComparison.OrdinalIgnoreCase)) options.ListenExternal = true;
                    }
                }
                else if (string.Equals(args[i], "-noshutdown", StringComparison.OrdinalIgnoreCase))
                {
                    options.ShutdownOnEnter = false;
                }
                else
                {
                    Console.WriteLine($"Unknown command line parameter : {args[i]}");
                }
            }

            return options;
        }
    }
}
