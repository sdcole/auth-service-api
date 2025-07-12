using Microsoft.AspNetCore.Mvc;
using AuthServiceAPI.Models;
using AuthServiceAPI.Interfaces;

namespace AuthServiceAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {


        private readonly IHashingService _hashingService;

        public AuthController(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }
        /**
         * This will make API calls to the local OLAMMA instance
         * 
         * 
         **/
        [HttpPost("v1/register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var hashedPass = _hashingService.HashPassword(request.Password);
            return StatusCode(200, new { Response = "Hashed Password = " + Convert.ToBase64String(hashedPass.Hash) + " With a salt of " + Convert.ToBase64String(hashedPass.Salt)});
        }
    }
}
