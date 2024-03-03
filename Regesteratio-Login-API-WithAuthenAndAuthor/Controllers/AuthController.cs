using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Regesteratio_Login_API_WithAuthenAndAuthor.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;



    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase

    {


        private readonly IMongoCollection<User> _usersCollection;


        public AuthController(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDB"));
            var database = client.GetDatabase("RegistrationWithAuth");
            _usersCollection = database.GetCollection<User>("Users");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegistrationRequest request)
        {
            // Check if email already exists
            var existingUser = await _usersCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return Conflict("Email already exists");
            }

            // Hash the password
            string passwordHash = HashPassword(request.Password);

            // Create new user
            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash
            };

            // Insert user into MongoDB
            await _usersCollection.InsertOneAsync(newUser);

            return Ok("Registration successful");
        }

        // Helper method to hash the password 
        private string HashPassword(string password)
        {

            return password;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Find user by email
            var user = await _usersCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid password");
            }

            // Generate JWT token 
            var token = GenerateJwtToken(user);

            return Ok(new { token });
        }

        // Helper method to verify the password 
        private bool VerifyPassword(string password, string passwordHash)
        {
            // Implement your password verification logic here

            return password == passwordHash;
        }

        // Helper method to generate JWT token 
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789")); // Replace with your actual secret key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        // Add other claims as needed
    };

            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1), // Token expiration time
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }




    }

    //authorization 

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("profile")]
        public IActionResult UserProfile()
        {
            // Retrieve user information from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            // Add other user properties as needed

            return Ok(new { UserId = userId, Email = userEmail });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminEndpoint()
        {
            // Only accessible to users with the "Admin" role
            return Ok("Admin endpoint");
        }
    }
