using ChatGPT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ChatGPT.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly ChatAppContext _context;

        public AuthorizationController(ChatAppContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(User user)
        {
            if (ModelState.IsValid)
            {
                user.Role = "user";
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await Authorization(user.Login, user.Password);
                return RedirectToAction("Index", "Chat");
            }
            else
            {
                return View(user);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Authorization(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                return BadRequest("пароль не установлен");
            }

            var user = await _context.Users.FirstOrDefaultAsync(w => w.Login == login && w.Password == password);
            if (user == null)
            {
                return Unauthorized();
            }

            // Получение должности пользователя из базы данных
            var role = await _context.Users.FirstOrDefaultAsync(r => r.Role == user.Role);
            if (role == null)
            {
                return NotFound("aboba");
            }

            // Создание claims для аутентификации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, role.Role),
                new Claim("userId", user.UserId.ToString() , ClaimValueTypes.Integer32)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            
            if(HttpContext != null)
                await HttpContext.SignInAsync(claimsPrincipal);
            
            if (HttpContext != null)
                return RedirectToAction("Index", "Home");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Chat");
        }
    }

}
