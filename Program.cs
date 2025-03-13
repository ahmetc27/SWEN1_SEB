using System;
using SEB.Models;
using SEB.Server;

//User user = new User { Username = "Ahmet", Elo = 1500 };
//Console.WriteLine(user.ToString());  
// Output: User: Ahmet, ELO: 1500

// Create and start the server
var server = new Server(port: 10001);
server.Start();