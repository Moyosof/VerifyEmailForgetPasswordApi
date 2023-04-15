using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using VerifyApiForgetApi.Data;

namespace VerifyApiForgetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _ctx;
        public UserController(DataContext context)
        {
            _ctx = context;
        }

        [HttpPost("register")]
       public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            if(_ctx.Users.Any(u => u.Email== request.Email))
            {
                return BadRequest("User already exists.");
            }

            CreatePasswordHash(request.Password, 
                out byte[] passwordHash, 
                out byte[] passwordSalt);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                VerificationToken = CreateRandomToken()
            };

            _ctx.Users.Add(user);
            await _ctx.SaveChangesAsync();
            return Ok("User Succesfully Created ");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if(user == null)
            {
                return BadRequest("User not found");
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Password is incorrect");
            }

            if (user.VerifiedAt == null)
            {
                return BadRequest("Not verified");
            }

            

            return Ok($"Welcome back, {user.Email}! :)");

        }



        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.VerifiedAt = DateTime.Now;
            await _ctx.SaveChangesAsync();





            return Ok("User verified");

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            user.PasswordResetToken = CreateRandomToken();
            user.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _ctx.SaveChangesAsync();


            return Ok("You may reset your password");

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var user = await _ctx.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == request.Token);
            if (user == null)
            {
                return BadRequest("Invalid Token.");
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;


            await _ctx.SaveChangesAsync();


            return Ok("Password successfully reset");

        }


        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac
                     .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }


        private bool VerifyPasswordHash(string password, byte[] passwordHash,byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                
                var computedHash = hmac
                     .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}
