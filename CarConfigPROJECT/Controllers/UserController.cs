using CarConfigPROJECT.DTOs;
using CarConfigDATA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarConfigDATA.Security;

namespace CarConfigPROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AutoConfigDbContext _context;

        public UserController(IConfiguration configuration, AutoConfigDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Username već postoji.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Email već postoji.");

            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(request.Password, salt);

            // Dohvati default User rolu
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (role == null)
                return BadRequest("Default role 'User' ne postoji u bazi.");

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                Salt = salt,
                PasswordHash = hash,
                RoleId = role.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = JwtTokenProvider.CreateToken(
                secureKey: _configuration["JWT:SecureKey"],
                expiration: 120,
                subject: user.Username,
                role: role.Name
            );

            return Ok(new { Username = user.Username, Role = role.Name, Token = token });
        }
        // ------------------ LOGIN ------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            var user = await _context.Users.Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return BadRequest("Korisnik ne postoji.");

            var hash = PasswordHashProvider.GetHash(request.Password, user.Salt);
            if (hash != user.PasswordHash)
                return BadRequest("Pogrešna lozinka.");

            var token = JwtTokenProvider.CreateToken(
                secureKey: _configuration["JWT:SecureKey"],
                expiration: 120,
                subject: user.Username,
                role: user.Role.Name
            );

            //return Ok(new { Username = user.Username, Role = user.Role.Name, Token = token });
            return Ok(new {Token = $"{token}"});
        }

        // ------------------ TEST ADMIN ------------------
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Samo Admin korisnici mogu ovo vidjeti!");
        }

        // ------------------ TEST SVI USERI ------------------
        [HttpGet("all-users")]
        [Authorize]
        public IActionResult AllUsers()
        {
            return Ok("Svi prijavljeni korisnici ovo mogu vidjeti.");
        }
    }

}






