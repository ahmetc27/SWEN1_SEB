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
        private readonly UserRepository _userRepo;
        private readonly RequestHandler _requestHandler;
        public Server(int port, string connectionString)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _userRepo = new UserRepository(connectionString);
            _requestHandler = new RequestHandler();
        }

        public void Start()
        {
            _listener.Start();
            Console.WriteLine("Server started. Waiting for connections...");

            while(true)
            {
                var client = _listener.AcceptTcpClient();
                Console.WriteLine("New connection received!");

                _requestHandler.HandleRequest(client);
                RouteRequest(client);
            }
        }

        public void RouteRequest(TcpClient client)
        {
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream) { AutoFlush = true };

            // Handle different paths and methods
            string responseBody = string.Empty;
            if(_requestHandler.Path == "/users" && _requestHandler.Method == "GET")
            {
                var users = _userRepo.GetAllUsers(); // get users from database
                responseBody = JsonSerializer.Serialize(users);
            }
            else if(_requestHandler.Path == "/users" && _requestHandler.Method == "POST")
            {
                User? user = null;

                try
                {
                    if(!string.IsNullOrEmpty(_requestHandler.Body))
                    {
                        user = JsonSerializer.Deserialize<User>(_requestHandler.Body);
                    }

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
            else if(_requestHandler.Path.StartsWith("/users/") && _requestHandler.Method == "DELETE")
            {
                string[] _parts = _requestHandler.Path.Split('/');
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
            }
            else
            {
                writer.WriteLine("HTTP/1.1 404 Not Found");
                return;
            }

            // Send a basic response
            writer.WriteLine("HTTP/1.1 200 OK");
            writer.WriteLine(_requestHandler.Path == "/users" ? "Content-Type: application/json" : "Content-Type: text/plain");
            writer.WriteLine(); // End of headers
            writer.WriteLine(responseBody);
        }
    }
}