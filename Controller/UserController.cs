// Controllers should remain simple and only focus on request handling and routing
using System.Text.Json;
using SEB.Models;
using SEB.Repository;

namespace SEB.Controller
{
    public class UserController
    {
        private readonly UserRepository _userRepo;

        public UserController(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public string GetAllUsers()
        {
            var users = _userRepo.GetAllUsers();
            return JsonSerializer.Serialize(users);
        }

        public string AddUser(string requestBody)
        {
            try
            {
                var user = JsonSerializer.Deserialize<User>(requestBody);
                if(user != null)
                {
                    _userRepo.Add(user);
                    return JsonSerializer.Serialize(user);
                }
            }
            catch(JsonException ex)
            {
                Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                return "Failed to register user";
            }
            return string.Empty;
        }
        public void DeleteUser(int userId)
        {
            _userRepo.Delete(userId);
        }
    }
}