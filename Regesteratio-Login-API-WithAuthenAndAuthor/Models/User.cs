using MongoDB.Bson;

namespace Regesteratio_Login_API_WithAuthenAndAuthor.Models
{
    // User model
    public class User
    {
        public ObjectId Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        // Add other user properties as needed
    }

    // Registration request model
    public class RegistrationRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        // Add other registration fields as needed
    }

    // Login request model
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}
