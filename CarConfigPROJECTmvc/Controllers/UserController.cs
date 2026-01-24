using CarConfigDATA.Models;
using CarConfigDATA.Security;
using CarConfigDATA.Security;
using CarConfigPROJECTmvc.ViewModels.User;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;


namespace CarConfigPROJECTmvc.Controllers
{
    public class UserController : Controller
    {

        private readonly AutoConfigDbContext _context;

        public UserController(AutoConfigDbContext context)
        {
            _context = context;
        }

        // User/Login?ReturnUrl=%2FGenre GET
        public IActionResult Login(string returnUrl)
        {
            var loginVm = new UserLoginVM
            {
                ReturnUrl = returnUrl
            };

            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLoginVM loginVm)
        {
            // Try to get a user from database
            var existingUser =
                _context
                    .Users
                    .Include(x => x.Role)
                    .FirstOrDefault(x => x.Username == loginVm.Username);

            if (existingUser == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            // Check is password hash matches
            var b64hash = PasswordHashProvider.GetHash(loginVm.Password, existingUser.Salt);
            if (b64hash != existingUser.PasswordHash)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View();
            }

            var claims = new List<Claim>() {
                new Claim(ClaimTypes.Name, loginVm.Username),
                new Claim(ClaimTypes.Role, existingUser.Role.Name)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties();

            // We need to wrap async code here into synchronous since we don't use async methods
            Task.Run(async () =>
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties)
            ).GetAwaiter().GetResult();

            if (loginVm.ReturnUrl != null)
                return LocalRedirect(loginVm.ReturnUrl);
            else if (existingUser.Role.Name == "Admin")
                return RedirectToAction("Index", "Home");
            else if (existingUser.Role.Name == "User")
                return RedirectToAction("Index", "Home");
            else
                return View();
        }
        //GET /User/Logout
        public IActionResult Logout()
        {
            Task.Run(async () =>
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme)
            ).GetAwaiter().GetResult();

            return View(); 
        }

        //GET /User/Register -- prikaz forme
        public IActionResult Register()
        {
            return View(); // /User/Register.cshtml 

        }

        //POST /User/Register
        [HttpPost]
        public IActionResult Register(UserRegisterVM userVm)
        {
            try
            {
                // Check if there is such a username in the database already
                var trimmedUsername = userVm.Username.Trim();
                if (_context.Users.Any(x => x.Username.Equals(trimmedUsername)))
                {
                    ModelState.AddModelError("", "Username already exist");
                }

                // Hash the password
                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(userVm.Password, b64salt);

                // Create user from DTO and hashed password
                var user = new User
                {                  
                    Username = userVm.Username,
                    Salt = b64salt,
                    PasswordHash = b64hash,                  
                    Email = userVm.Email,
                   
                    RoleId = 2, // regular user


                };

                // Add user and save changes to database
                _context.Add(user);
                _context.SaveChanges();

                //Vrati doma
                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",ex.Message);
                return View();
            }
        }

    }
}
