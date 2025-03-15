using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SEB.Server
{
    public class RequestHandler
    {
        public string? Method { get; private set; }
        public string? Path { get; private set; }
        public string? Version { get; private set; }
        public string? Body { get; private set; }

        public void HandleRequest(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);

            var requestLine = reader.ReadLine();

            if(requestLine == null)
            {
                Console.WriteLine("Received an empty request.");
                return;
            }

            var parts = requestLine.Split(' ');
            Method = parts[0]; // GET, POST
            Path = parts[1];   // /users
            Version = parts[2]; // HTTP/1.1

            int contentLength = 0;
            string? line;

            while((line = reader.ReadLine()) != null)
            {
                if(line.Length == 0) break; // emtpy line indicates the end of the HTTP-headers
                
                var headerParts = line.Split(':');
                var headerName = headerParts[0];
                var headerValue = headerParts[1].Trim(); // Trim because eg Content-Type: application/json

                if(headerName == "Content-Length")
                {
                    contentLength = int.Parse(headerValue);
                }

                StringBuilder requestBody = new StringBuilder();
                if(contentLength > 0)
                {
                    char[] chars = new char[1024]; // Create buffer
                    int bytesReadTotal = 0;
                    while(bytesReadTotal < contentLength)
                    {
                        var bytesRead = reader.Read(chars, 0, chars.Length);
                        bytesReadTotal += bytesRead;
                        if(bytesRead == 0) { break; } // no more data available
                        requestBody.Append(chars, 0, bytesRead);
                    }
                }
                Body = requestBody.ToString();

            }
        }
    }
}