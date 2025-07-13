using Microsoft.AspNetCore.Mvc;
using AuthServiceAPI.Models;
using AuthServiceAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using AuthServiceAPI.Data;
using System.Text.RegularExpressions;
using System.Text;

namespace AuthServiceAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {


        private readonly IHashingService _hashingService;
        private readonly AppDbContext _dbContext;

        public AuthController(IHashingService hashingService, AppDbContext dbContext)
        {
            _hashingService = hashingService;
            _dbContext = dbContext;
        }


        /**
         * Register new user to the auth service.
         * 
         * 
         **/
        [HttpPost("v1/register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {

            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();

            // Basic null/empty check
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return BadRequest(new { Response = "Email is required." });
            }

            if (normalizedEmail.Length > 254)
            {
                return BadRequest(new { Response = "Email is too long. Maximum is 254 characters." });
            }

            if (!Regex.IsMatch(normalizedEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return BadRequest(new { Response = "Email format is invalid." });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Response = "Password is required." });
            }

            if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$"))
            {
                return BadRequest(new
                {
                    Response = "Password must be at least 12 characters and contain at least one lowercase, one uppercase, one digit, and one special character."
                });
            }

            if (await _dbContext.Users.AnyAsync(u => u.Email == normalizedEmail))
            {
                return Conflict(new { Response = "User already exists." });
            }

            var hashedPass = _hashingService.HashPassword(request.Password);

            User newUser = new User
            {
                Email = request.Email.ToLower(),
                PasswordHash = Convert.ToBase64String(hashedPass.Hash),
                PasswordSalt = Convert.ToBase64String(hashedPass.Salt),
                FirstName = request.FirstName,
                LastName = request.LastName,

            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
            return StatusCode(200, new { Response = "New User Created: " + newUser.Id });
        }



        /**
         * Login that will return a session cookie
         * 
         * 
         **/
        [HttpPost("v1/login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {

            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();

            // Basic null/empty check
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return BadRequest(new { Response = "Email is required." });
            }

            if (normalizedEmail.Length > 254)
            {
                return BadRequest(new { Response = "Email is too long. Maximum is 254 characters." });
            }

            if (!Regex.IsMatch(normalizedEmail, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                return BadRequest(new { Response = "Email format is invalid." });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { Response = "Password is required." });
            }

            if (!Regex.IsMatch(request.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{12,}$"))
            {
                return BadRequest(new
                {
                    Response = "Password must be at least 12 characters and contain at least one lowercase, one uppercase, one digit, and one special character."
                });
            }



            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user == null)
                return NotFound(new { Response = "User not found." });

            var passwordBytes = Encoding.UTF8.GetBytes(request.Password);
            var saltBytes = Convert.FromBase64String(user.PasswordSalt); // assuming base64 encoding
            var storedHash = Convert.FromBase64String(user.PasswordHash);

            var hashedPass = _hashingService.HashValue(passwordBytes, saltBytes);

            if (hashedPass.SequenceEqual(storedHash))
            {
                // Create new session object
                var session = new Session
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    DeviceHash = ComputeDeviceHash(Request), // Implement this method to hash user-agent or device info
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7), // Session expires in 7 days, adjust as needed
                    LastAccessed = DateTime.UtcNow,
                    IsActive = true
                };

                // Add and save to DB
                _dbContext.Sessions.Add(session);
                await _dbContext.SaveChangesAsync();

                // Optionally, set the session ID in a secure cookie
                Response.Cookies.Append("session_id", session.Id.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = session.ExpiresAt
                });
                // Create session cookie or token here
                return Ok(new { Response = "Login successful" });
            }

            return Unauthorized(new { Response = "Invalid credentials, please try again." });

        }

        private string ComputeDeviceHash(HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].ToString();
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(userAgent);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
