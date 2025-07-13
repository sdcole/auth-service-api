using Microsoft.AspNetCore.Mvc;
using AuthServiceAPI.Models;
using AuthServiceAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using AuthServiceAPI.Data;
using System.Text.RegularExpressions;

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
            return StatusCode(200, new { Response = "New User Created: "  + newUser.Id});
        }


    }
}
