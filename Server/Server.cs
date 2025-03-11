using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SEB.Server
{
    public class Server
    {
        private readonly TcpListener _listener;
        public Server(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server started. Waiting for connections...");

            while(true)
            {
                // Wait for a client to connect
                var client = _listener.AcceptTcpClient();
                Console.WriteLine("New connection received!");

                // Handle the request
                HandleRequest(client);
            }
        }

        private void HandleRequest(TcpClient client)
        {
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream);
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            // Read the request line (e.g., "GET / HTTP/1.1")
            var requestLine = reader.ReadLine();
            Console.WriteLine($"Request: {requestLine}");

            // Send a basic response
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine("Content-Type: text/plain");
            writer.WriteLine(); // End of headers
            writer.WriteLine("Hello, World!");
        }
    }
}