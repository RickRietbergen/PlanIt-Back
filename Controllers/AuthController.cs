using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanIt.Database;
using PlanIt.Database.Entities;
using PlanIt.Database.Enums;
using PlanIt.Database.JWT.Services;
using PlanIt.Database.Models;
using System.Security.Cryptography;

namespace PlanIt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly PlanItContext dataContext;
        private readonly IConfiguration configuration;
        private readonly JWTService jwtService;
        private readonly AesService aesService;

        public AuthController(PlanItContext dataContext, IConfiguration configuration)
        {
            this.dataContext = dataContext;
            this.configuration = configuration;
            jwtService = new JWTService(configuration.GetSection("AppSettings:Token").Value);
            aesService = new AesService();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserCreateModel model)
        {
            var user = await dataContext.Users.Where(g => g.Username == model.Username).FirstOrDefaultAsync();

            if (user != null)
            {
                return BadRequest("Username is already in use.");
            }

            var newUser = new User();
            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);

            newUser.Username = model.Username;
            newUser.PasswordHash = passwordHash;
            newUser.PasswordSalt = passwordSalt;

            using var rsa = RSA.Create();
            byte[] privateKeyPkcs8 = rsa.ExportPkcs8PrivateKey();
            byte[] publicKey = rsa.ExportSubjectPublicKeyInfo();

            string publicKeyBase64Encoded = Convert.ToBase64String(publicKey);
            string privateKeyBase64Encoded = Convert.ToBase64String(privateKeyPkcs8);

            newUser.PublicKey = publicKeyBase64Encoded;
            newUser.EncryptedPrivateKey = aesService.EncryptString(model.Password, privateKeyBase64Encoded);

            dataContext.Users.Add(newUser);
            dataContext.SaveChanges();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginModel model)
        {
            var user = await dataContext.Users.Where(g => g.Username == model.Username).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("User was not found");
            }

            if (!VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Username or Password does not match");
            }

            return Ok(new
            {
                user = user.Id,
                userName = user.Username,
                token = jwtService.CreateJWT(user),
                privateKey = aesService.DecryptString(model.Password, user.EncryptedPrivateKey),
                publicKey = user.PublicKey,
            });
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
