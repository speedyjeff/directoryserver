using System;
using System.Collections.Generic;
using System.Text;

namespace directoryserver
{
    class Options
    {
        public string Directory;
        public int Port;
        public string Protocol;
        public bool ShowHelp;

        public Options()
        {
            Directory = ".";
            Port = 8000;
            Protocol = "HTTP";
            ShowHelp = false;
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
                    if (i < args.Length) options.Directory = args[i];
                }
                else if (string.Equals(args[i], "-port", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i < args.Length)
                    {
                        if (Int32.TryParse(args[i], out int port)) options.Port = port;
                    }
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
