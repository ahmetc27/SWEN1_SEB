using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using SEB.Models;

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

            if(requestLine == null)
            {
                Console.WriteLine("Received an empty request.");
                return;
            }

            Console.WriteLine($"Request: {requestLine}");

            // Parse the request line
            var parts = requestLine.Split(' ');
            var method = parts[0]; // e.g., GET, POST
            var path = parts[1];   // e.g., /, /users
            var version = parts[2]; // e.g., HTTP/1.1

            // Read headers
            int contentLength = 0;
            string? line;
            while((line = reader.ReadLine()) != null)
            {
                if(line.Length == 0) break; // // emtpy line indicates the end of the HTTP-headers

                // Parse the header
                var headerParts = line.Split(':');
                var headerName = headerParts[0];
                var headerValue = headerParts[1].Trim();
                Console.WriteLine($"{headerName}: {headerValue}");

                if(headerName == "Content-Length")
                {
                    contentLength = int.Parse(headerValue);
                }
            }

            // Body
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

            Console.WriteLine($"Body: {requestBody.ToString()}");

            // Handle different paths and methods
            string responseBody;
            if(path == "/users" && method == "GET")
            {
                responseBody = "<html><body>TEST GET!</body></html>";
            }
            else if(path == "/users" && method == "POST")
            {
                // Deserialize JSON request body
                var user = JsonSerializer.Deserialize<User>(requestBody.ToString());
                /*
                JsonSerializer.Deserialize<User>(requestBody.ToString()) wandelt in folgendes Objekt um:
                User user = new User { Username = "Ahmet", Password = "123" };
                */
                if(user != null)
                {
                    Console.WriteLine($"Registered user: {user.Username}");
                }
                else
                {
                    Console.WriteLine("Failed to register user: request body is invalid.");
                }

                // Serialize JSON response
                responseBody = JsonSerializer.Serialize(new { message = "User registered", user });
                /*
                {
                    "message": "User registered",
                    "user": {
                        "Username": "Ahmet",
                        "Password": "123"
                    }
                }
                */
                // use new because message is new
            }
            else
            {
                responseBody = "<html><body>404 Not Found</body></html>";
            }


            // Send a basic response
            writer.WriteLine("HTTP/1.1 200 OK");
            //writer.WriteLine("Content-Type: text/plain");
            writer.WriteLine(); // End of headers
            writer.WriteLine(responseBody);
        }
    }
}