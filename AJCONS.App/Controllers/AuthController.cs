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
                ViewData["SweetAlert_Error"] = result.ErrorMessage;
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
                if (result.Data.IsFirstLogin)
                {
                    return RedirectToAction("FirstLoginSetup", "Student");
                }
                return RedirectToAction("Index", "Student");
            }
            else if (result.Data.Role == "Mentor")
            {
                // If mentor account was just registered and is pending approval, don't redirect to dashboard
                if (TempData["MentorRegistered"] != null && (bool)TempData["MentorRegistered"])
                {
                    ViewData["SweetAlert_Error"] = "Your mentor account is pending approval. You will be able to log in once approved.";
                    return View(dto);
                }
                return RedirectToAction("Index", "Mentor");
            }
            else if (result.Data.Role == "ExternalPartner")
            {
                return RedirectToAction("Index", "ExternalPartner");
            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ViewData["SweetAlert_Error"] = "Invalid role.";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var vm = new RegisterViewModel();
            await PopulateRegisterOptions(vm);
            return View(vm);
        }

        [HttpPost("RegisterMentor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterMentor([Bind(Prefix = "Mentor")] MentorRegistrationDto dto)
        {
            var vm = new RegisterViewModel { Mentor = dto };
            if (!ModelState.IsValid)
            {
                await PopulateRegisterOptions(vm);
                return View("Register", vm);
            }

            var result = await _authService.RegisterMentorAsync(dto);
            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Registration Submitted!";
                TempData["SweetAlert_Message"] = "Your mentor registration has been submitted for admin approval. You will be able to log in once approved.";
                TempData["MentorRegistered"] = true;
                return RedirectToAction(nameof(Login));
            }

            ViewData["SweetAlert_Error"] = result.ErrorMessage;
            await PopulateRegisterOptions(vm);
            return View("Register", vm);
        }

        [HttpPost("RegisterExternalPartner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterExternalPartner([Bind(Prefix = "ExternalPartner")] ExternalPartnerRegistrationDto dto)
        {
            var vm = new RegisterViewModel { ExternalPartner = dto };
            if (!ModelState.IsValid)
            {
                await PopulateRegisterOptions(vm);
                return View("Register", vm);
            }

            var result = await _authService.RegisterExternalPartnerAsync(dto);
            if (result.IsSuccess)
            {
                TempData["SweetAlert_Type"] = "success";
                TempData["SweetAlert_Title"] = "Registration Submitted!";
                TempData["SweetAlert_Message"] = "Your registration has been submitted for admin approval. You will be able to log in once approved.";
                return RedirectToAction(nameof(Login));
            }

            ViewData["SweetAlert_Error"] = result.ErrorMessage;
            await PopulateRegisterOptions(vm);
            return View("Register", vm);
        }

        private async Task PopulateRegisterOptions(RegisterViewModel vm)
        {
            var options = await _authService.GetRegisterOptionsAsync();
            if (options.IsSuccess)
            {
                vm.Options = options.Data;
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
