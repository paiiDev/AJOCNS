using AJOCNS.Domain.Interfaces;
using AJOCNS.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AJOCNS.App.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.Identity?.IsAuthenticated == true && User.IsInRole("Student"))
            {
                return RedirectToAction("Index", "Student");
            }
            else if (User.Identity?.IsAuthenticated == true && User.IsInRole("Mentor"))
            {
                return RedirectToAction("Index", "Mentor");
            }
            else if (User.Identity?.IsAuthenticated == true && User.IsInRole("ExternalPartner"))
            {
                return RedirectToAction("Index", "ExternalPartner");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            var result = await _authService.LoginAsync(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Data.UserId.ToString()),
                new Claim(ClaimTypes.Email, result.Data.Email),
                new Claim(ClaimTypes.Name, result.Data.Name),
                new Claim(ClaimTypes.Role, result.Data.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
            {
                IsPersistent = dto.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                            new ClaimsPrincipal(claimsIdentity),
                                             authProperties);

            if (result.Data.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (result.Data.Role == "Student")
            {
                return RedirectToAction("Index", "Student");
            }
            else if (result.Data.Role == "Mentor")
            {
                return RedirectToAction("Index", "Mentor");
            }
            else if (result.Data.Role == "ExternalPartner")
            {
                return RedirectToAction("Index", "ExternalPartner");
            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ModelState.AddModelError(string.Empty, "Invalid role.");
                return View("Index", dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
