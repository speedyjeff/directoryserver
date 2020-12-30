using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace directoryserver
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var options = Options.Parse(args);

            // show help
            if (options.ShowHelp) return Options.DisplayHelp();

            // initialize
            var host = options.ListenExternal ? Dns.GetHostEntry(Dns.GetHostName()) : Dns.GetHostEntry("localhost");
            IPAddress ip = null;

            // choose appropriate ip address
            foreach (var lip in host.AddressList)
            {
                // ipv4
                if (lip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = lip;
                    break;
                }
            }
            if (ip == null && host.AddressList.Length >= 1) ip = host.AddressList[0];
            if (ip == null) throw new Exception("Failed to get an ip address to listen too");

            // start http lisenter
            var endpoint = new IPEndPoint(ip, options.Port);
            var http = new HttpListener();
            http.Prefixes.Add($"{options.Protocol}://{endpoint}/");

            // start
            http.Start();
            Console.WriteLine($"Servering {options.Protocol} on {endpoint} at '{options.Directory}' ...");

            // async handle the incoming requests
            HandleIncoming(http, options);

            // wait
            Console.WriteLine($"<ctrl-c> {(options.ShutdownOnEnter ? "or <enter> " : "")}to exit");
            if (options.ShutdownOnEnter) Console.ReadLine();
            else while (true) Console.ReadLine();

            // exit
            http.Close();

            Console.WriteLine("shutting down");

            return 0;
        }

        private async static void HandleIncoming(HttpListener http, Options options)
        {
            while (http.IsListening)
            {
                try
                {
                    // block to get request
                    var context = await http.GetContextAsync();
                    var contenttype = context.Request.AcceptTypes != null && context.Request.AcceptTypes.Length > 0 ? context.Request.AcceptTypes[0] : "";

                    // log the incoming request
                    Console.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} {DateTime.Now:o} \"{context.Request.HttpMethod} {contenttype} {context.Request.RawUrl} {options.Protocol}/{context.Request.ProtocolVersion}\" {context.Response.StatusCode}");

                    // get path to the local file
                    var path = "";
                    var filename = context.Request.RawUrl;

                    // default file
                    if (string.Equals(context.Request.RawUrl, "/"))
                    {
                        foreach (var file in new string[] { "index.html", "index.htm", "default.html", "default.htm" })
                        {
                            path = Path.Combine(options.Directory, file);
                            if (File.Exists(path))
                            {
                                filename = file;
                                break;
                            }
                        }
                    }

                    // remove leading '/'
                    if (filename.Length > 1 && filename[0] == '/') filename = filename.Substring(1);

                    // normalize path
                    path = Path.GetFullPath(Path.Combine(options.Directory, filename));

                    // check if the path is absolute or trying to escape out of the original directory
                    if (!path.StartsWith(options.Directory)) path = "";

                    // read from disk
                    byte[] buffer;
                    if (File.Exists(path))
                    {
                        using (var stream = File.OpenRead(path))
                        {
                            buffer = new byte[stream.Length];
                            await stream.ReadAsync(buffer);
                        }
                    }
                    else
                    {
                        var responseString = $"<HTML><BODY>File not found {filename}</BODY></HTML>";
                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                        contenttype = "text/html";
                        context.Response.StatusCode = 404;
                    }

                    // write
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.ContentType = contenttype;
                    using (var output = context.Response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"exception : {e}");
                }
            }
        }
    }
}
