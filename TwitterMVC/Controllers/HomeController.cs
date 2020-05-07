﻿using TwitterMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NETCore.MailKit.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TwitterMVC.Controllers
{
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        [HttpGet("")]
        [Route("Index")]
        public IActionResult Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Profile");
            }
        }

        [Authorize]
        [HttpGet("Profile")]
        public IActionResult Profile() => View();

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Profile");
            }
        }

        [HttpGet("Register")]
        public IActionResult Register() => View();

        [HttpGet("VerifiedEmail")]
        public IActionResult VerifiedEmail() => View();

        [HttpGet("VerifyEmailSent")]
        public IActionResult VerifyEmailSent() => View();

        [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword() => View();

        [HttpGet("ForgotPasswordEmailSent")]
        public IActionResult ForgotPasswordEmailSent() => View();

        [HttpGet("ResetPasswordConfirmed")]
        public IActionResult ResetPasswordConfirmed() => View();

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(string emailOrUserName, string password)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                IdentityUser user;
                if (emailOrUserName.Contains('@'))
                {
                    user = await _userManager.FindByEmailAsync(emailOrUserName);

                }
                else
                {
                    user = await _userManager.FindByNameAsync(emailOrUserName);
                }
                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Profile");
                    }
                }

                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Profile");
            }
           
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(string userName, string email, string password)
        {
            if (userName == null || email == null || password == null) { return BadRequest(); } // Bad Input

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null) { return BadRequest(); } // User Already Exists

            user = new IdentityUser
            {
                UserName = userName,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded) { return RedirectToAction("Index"); }

            if (_userManager.Options.SignIn.RequireConfirmedEmail)
            {
                var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                emailToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));

                var emailVerificationUrl = Url
                    .Action(
                        "VerifyEmail",
                        "Home",
                        new { userId = user.Id, emailToken },
                        Request.Scheme,
                        Request.Host.ToString());

                var emailVerificationHtml = $"<a href=\"{HtmlEncoder.Default.Encode(emailVerificationUrl)}\">Verify Email Address!</a>";

                await _emailService.SendAsync("test@email.com", "Email Verification", emailVerificationHtml, true);

                return RedirectToAction("VerifyEmailSent");
            }
            else
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                if (!signInResult.Succeeded) { return BadRequest(); }

                return RedirectToAction("Profile");
            }
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        [HttpGet("VerifyEmail")]
        public async Task<IActionResult> VerifyEmailAsync(string userId, string emailToken)
        {
            if (userId == null || emailToken == null) { return RedirectToAction("Index"); }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) { return BadRequest(); }

            emailToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(emailToken));

            var result = await _userManager.ConfirmEmailAsync(user, emailToken);
            if (!result.Succeeded) { return BadRequest(); }

            return RedirectToAction("VerifiedEmail");
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPasswordAsync(string email)
        {
            if (email == null) { return RedirectToAction("ForgotPassword"); }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) { return BadRequest(); }

            // Generate Password Token
            var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            passwordToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(passwordToken));

            // Generate Password Reset Url
            var passwordResetUrl = Url
                .Action(
                    nameof(ResetPassword),
                    "Home",
                    new { userId = user.Id, passwordToken },
                    Request.Scheme,
                    Request.Host.ToString());

            // Generate an HTML Link from Verification URL
            var passwordResetHtml = $"<a href='{HtmlEncoder.Default.Encode(passwordResetUrl)}'>Click here to reset your password!</a>";

            // Send email through EmailService (MailKit NetCore)
            await _emailService.SendAsync("test@email.com", "Password Reset Request", passwordResetHtml, true);

            return RedirectToAction("ForgotPasswordEmailSent");
        }

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string userId, string passwordToken)
        {
            if (userId == null || passwordToken == null) { return RedirectToAction("Index"); }

            passwordToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(passwordToken));

            return View(new ResetPasswordViewModel { UserId = userId, Token = passwordToken }); // Passing in the ViewModel.
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordViewModel resetPassword) // Receiving the ViewModel from post.
        {
            if (!ModelState.IsValid) { return View(resetPassword); }

            if (resetPassword.UserId == null || resetPassword.Token == null || resetPassword.NewPassword == null)
            { return RedirectToAction("Index"); }

            var user = await _userManager.FindByIdAsync(resetPassword.UserId);
            if (user == null) { return BadRequest(); }

            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.NewPassword);
            if (!result.Succeeded) { return BadRequest(); }

            return RedirectToAction("ResetPasswordConfirmed");
        }
    }
}