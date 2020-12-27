using System;
using System.IO;
using System.Net;
using System.Text;

namespace directoryserver
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var options = Options.Parse(args);

            // show help
            if (options.ShowHelp)
            {
                Console.WriteLine("./directoryserver [-dir directory] [-port ####]");
                return 1;
            }

            // initialize
            var host = Dns.GetHostEntry("localhost");
            var ip = host.AddressList[0];
            var endpoint = new IPEndPoint(ip, options.Port);

            var http = new HttpListener();
            http.Prefixes.Add($"http://{endpoint}/");
            http.Start();

            Console.WriteLine($"Servering {options.Protocol} on {endpoint} at '{options.Directory}' ...");

            // async handle the incoming requests
            HandleIncoming(http, options);

            // wait
            Console.WriteLine("<ctrl-c> or <enter> to exit");
            Console.ReadLine();

            // exit
            http.Close();

            return 0;
        }

        private async static void HandleIncoming(HttpListener http, Options options)
        {
            while (http.IsListening)
            {
                // block to get request
                var context = await http.GetContextAsync();
                var contenttype = context.Request.AcceptTypes != null && context.Request.AcceptTypes.Length > 0 ? context.Request.AcceptTypes[0] : "";

                // log the incoming request
                Console.WriteLine($"{System.Threading.Thread.CurrentThread.ManagedThreadId} {DateTime.Now:o} \"{context.Request.HttpMethod} {contenttype} {context.Request.RawUrl} {options.Protocol}/{context.Request.ProtocolVersion}\" {context.Response.StatusCode}");

                // get content
                byte[] buffer;

                // text
                if (string.IsNullOrWhiteSpace(contenttype) ||
                    string.Equals(contenttype, "text/html") ||
                    string.Equals(contenttype, "*/*") ||
                    string.Equals(contenttype, "text/css"))
                {
                    // read file from disk
                    var path = "";
                    var responseString = "";
                    var filename = context.Request.RawUrl;

                    // default file
                    if (string.Equals(context.Request.RawUrl, "/"))
                    {
                        foreach(var file in new string[] { "index.html", "index.htm", "default.html", "default.htm" })
                        {
                            path = Path.Combine(options.Directory, file);
                            if (File.Exists(path))
                            {
                                filename = file;
                                break;
                            }
                        }
                    }

                    if (filename.Length > 1 && filename[0] == '/') filename = filename.Substring(1);

                    path = Path.Combine(options.Directory, filename);

                    // read from disk
                    if (File.Exists(path))
                    {
                        responseString = File.ReadAllText(path);
                    }
                    else
                    {
                        responseString = $"<HTML><BODY>File not found {path}</BODY></HTML>";
                        contenttype = "text/html";
                    }

                    // 
                    buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.ContentEncoding = Encoding.UTF8;
                    context.Response.ContentType = contenttype;
                }
                else
                {
                    throw new Exception($"Unknow content type : {contenttype}");
                }

                // write
                context.Response.ContentLength64 = buffer.Length;
                using (var output = context.Response.OutputStream)
                {
                    await output.WriteAsync(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
