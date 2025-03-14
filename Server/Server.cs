using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using SEB.Models;
using SEB.Repository;

namespace SEB.Server
{
    public class Server
    {
        private readonly TcpListener _listener;
        //private List<User> _users = new List<User>();
        private readonly UserRepository _userRepo;
        public Server(int port, string connectionString)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _userRepo = new UserRepository(connectionString);
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
                if(line.Length == 0) break; // emtpy line indicates the end of the HTTP-headers

                // Parse the header
                var headerParts = line.Split(':');
                var headerName = headerParts[0];
                var headerValue = headerParts[1].Trim();
                Console.WriteLine($"Header: {headerName}: {headerValue}");

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

            if(requestBody.Length > 0) // if body is not null
            {
                Console.WriteLine($"Body: {requestBody.ToString()}");
            }

            // Handle different paths and methods
            string responseBody = string.Empty;
            if(path == "/users" && method == "GET")
            {
                var users = _userRepo.GetAllUsers(); // get users from database
                responseBody = JsonSerializer.Serialize(users);
            }
            else if(path == "/users" && method == "POST")
            {
                User? user = null;

                try
                {
                    user = JsonSerializer.Deserialize<User>(requestBody.ToString());

                    if(user != null)
                    {
                        _userRepo.Add(user);
                        responseBody = JsonSerializer.Serialize(user);
                    }
                }
                catch(JsonException ex)
                {
                    Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                    writer.WriteLine("HTTP/1.1 400 Bad Request");
                    writer.WriteLine();
                    responseBody = "Failed to register user";
                    writer.WriteLine(responseBody);
                    return;
                }
            }
            else if(path.StartsWith("/users/") && method == "DELETE")
            {
                string[] _parts = path.Split('/');
                if(_parts.Length == 3 && int.TryParse(_parts[2], out int Userid))
                {
                    _userRepo.Delete(Userid);
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine("Content-Type: text/plain");
                    writer.WriteLine();
                    writer.WriteLine($"User {Userid} deleted.");
                }
                else
                {
                    writer.WriteLine("HTTP/1.1 400 Bad Request");
                    writer.WriteLine();
                    writer.WriteLine("Invalid ID.");
                }

                /*User? user = JsonSerializer.Deserialize<User>(requestBody.ToString());
                if(user != null && user.Id != null)
                {
                    _userRepo.Delete(user);
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine();
                    writer.WriteLine($"User {user.Username} deleted.");
                }
                else
                {
                    writer.WriteLine("HTTP/1.1 400 Bad Request");
                    writer.WriteLine();
                    writer.WriteLine("Missing/Invalid ID.");
                }*/
            }
            else
            {
                writer.WriteLine("HTTP/1.1 404 Not Found");
                return;
            }

            // Send a basic response
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine(path == "/users" ? "Content-Type: application/json" : "Content-Type: text/plain");
            writer.WriteLine(); // End of headers
            writer.WriteLine(responseBody);
        }
    }
}