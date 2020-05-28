using TwitterMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NETCore.MailKit.Core;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TwitterMVC.ViewModels;
using System.Security.Claims;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TwitterMVC.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace TwitterMVC.Controllers
{
    [Route("Home")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ITweetRepository _tweetRepository;
        private readonly AppDbContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService, ITweetRepository tweetRepository,
            AppDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _tweetRepository = tweetRepository;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [Route("/")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {              
                return View("Explore");
            }
            else
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
          

                
                ApplicationUser user = await _userManager.Users.Include(u => u.TweetLikes).Include(u => u.Followers).ThenInclude(f => f.Follower).Include(u => u.Following).ThenInclude(f => f.User).SingleAsync(u => u.Id == userId);


                //var usersToFollow = _context.UserFollows.Where(f => f.FollowerId != userId).ToList();
                IQueryable<ApplicationUser> usersToFollow = _userManager.Users.Include(u => u.Followers).Where(u => u.Id != user.Id);

                IndexViewModel viewModel = new IndexViewModel();
                viewModel.CurrentUser = user;

                ICollection<Tweet> tweetsOrdered = new Collection<Tweet>();

                IEnumerable<Tweet> tweets = _tweetRepository.GetUserTweets(user.Id);

                List<string> followedUsers = new List<string>();

                foreach(var item in user.Following)
                {
                    followedUsers.Add(item.UserId);
                }
                var followedUsersTweets = _tweetRepository.GetFollowedUsersTweets(followedUsers).ToList();
                foreach (Tweet item in tweets)
                {
                    tweetsOrdered.Add(item);
                }
                foreach (Tweet item in followedUsersTweets)
                {
                    tweetsOrdered.Add(item);
                }
                foreach (var userToFollow in usersToFollow)
                {
                    if (userToFollow.Followers.Count != 0)
                    {
                        var followedAlready = userToFollow.Followers.Where(f => f.FollowerId == userId).ToList().Count;
                        if (followedAlready == 0)
                        {
                            viewModel.UsersToFollow.Add(userToFollow);

                        }
                    }
                    if(userToFollow.Followers.Count == 0)
                    {
                        viewModel.UsersToFollow.Add(userToFollow);

                    }
                }
                //_tweetRepository.AddLikeToTweet(tweets.First().Id);
                viewModel.Tweets = tweetsOrdered.OrderByDescending(t => t.CreatedDate).ToList();
                return View("Index", viewModel);
            }
        }

        public async Task<IActionResult> AddTweet(string tweetContent)
        {
            if(tweetContent == null)
            {
                return RedirectToAction("Index");

            }
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            ApplicationUser user = await _userManager.Users.Include(u => u.TweetLikes).Include(u => u.Followers).Include(u => u.Following).SingleAsync(u => u.Id == userId);
            Tweet newTweet = new Tweet()
            {
                User = user,
                Content = tweetContent
            };
            _tweetRepository.PostTweet(newTweet);

            return RedirectToAction("Index");

        }
        [Route("FollowUser")]
        public async Task<IActionResult> FollowUser( string followedId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            ApplicationUser user = await _userManager.Users.Include(u => u.TweetLikes).Include(u => u.Followers).Include(u => u.Following).SingleAsync(u => u.Id == userId);

            ApplicationUser userToFollow = await _userManager.Users.Include(u => u.Following).SingleAsync(u => u.Id == followedId);
            UserFollow newFollow = new UserFollow()
            {
                UserId = userToFollow.Id,
                User = userToFollow,
                Follower = user,
                FollowerId = userId
            };
            user.Following.Add(newFollow);
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
        [Route("Profile")]
        [Authorize]
        public async Task<IActionResult> Profile() {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId



            ApplicationUser user = await _userManager.Users.Include(u => u.TweetLikes).Include(u => u.Followers).ThenInclude(f => f.Follower).Include(u => u.Following).ThenInclude(f => f.User).SingleAsync(u => u.Id == userId);


            //var usersToFollow = _context.UserFollows.Where(f => f.FollowerId != userId).ToList();
            IQueryable<ApplicationUser> usersToFollow = _userManager.Users.Include(u => u.Followers).Where(u => u.Id != user.Id);

            ProfileViewModel viewModel = new ProfileViewModel();
            viewModel.CurrentUser = user;

            ICollection<Tweet> tweetsOrdered = new Collection<Tweet>();

            IEnumerable<Tweet> tweets = _tweetRepository.GetUserTweets(user.Id);

            List<string> followedUsers = new List<string>();

            foreach (var item in user.Following)
            {
                followedUsers.Add(item.UserId);
            }
            foreach (Tweet item in tweets)
            {
                tweetsOrdered.Add(item);
            }

            foreach (var userToFollow in usersToFollow)
            {
                if (userToFollow.Followers.Count != 0)
                {
                    var followedAlready = userToFollow.Followers.Where(f => f.FollowerId == userId).ToList().Count;
                    if (followedAlready == 0)
                    {
                        viewModel.UsersToFollow.Add(userToFollow);

                    }
                }
                if (userToFollow.Followers.Count == 0)
                {
                    viewModel.UsersToFollow.Add(userToFollow);

                }
            }
            //_tweetRepository.AddLikeToTweet(tweets.First().Id);
            viewModel.Tweets = tweetsOrdered.OrderByDescending(t => t.CreatedDate).ToList();
            return View("Profile", viewModel);
        } 

        [Route("Login")]
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
        [Route("Explore")]
        public IActionResult Explore()
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
        [Route("Register")]
        public IActionResult Register() => View();
        [Route("VerifiedEmail")]
        public IActionResult VerifiedEmail() => View();
        [Route("VerifyEmailSent")]
        public IActionResult VerifyEmailSent() => View();
        [Route("ForgotPassword")]
        public IActionResult ForgotPassword() => View();
        [Route("ForgotPasswordEmailSent")]
        public IActionResult ForgotPasswordEmailSent() => View();
        [Route("ResetPasswordConfirmed")]
        public IActionResult ResetPasswordConfirmed() => View();

        [HttpPost("Explore")]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(string emailOrUserName, string password)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                if (emailOrUserName == null) return RedirectToAction("Index");
                ApplicationUser user;
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
                    var tweets = _tweetRepository.GetUserTweets(user.Id);
                    var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", tweets);
                    }
                }

                return RedirectToAction("Explore");
            }
            else
            {
                return RedirectToAction("Index");
            }
           
        }
        [HttpPost("profile")]
        public async Task<IActionResult> EditForm(IFormFile banner, IFormFile avatar)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId



            ApplicationUser user = await _userManager.Users.SingleAsync(u => u.Id == userId);
            if (banner != null)

            {
                if (banner.Length > 0)

                //Convert Image to byte and save to database

                {

                    byte[] p1 = null;
                    using (var fs1 = banner.OpenReadStream())
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                    user.Banner = p1;

                }
            }
            if (avatar != null)

            {
                if (avatar.Length > 0)

                //Convert Image to byte and save to database

                {

                    byte[] p1 = null;
                    using (var fs1 = avatar.OpenReadStream())
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                    user.Avatar = p1;

                }
            }
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Profile");
        }
        
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(string name,string userName, string email, string password)
        {
            if (name == null ||userName == null || email == null || password == null) { return BadRequest(); } // Bad Input

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null) { return BadRequest(); } // User Already Exists

            user = new ApplicationUser
            {
                FullName = name,
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
        [Route("Logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        [Route("VerifyEmail")]
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

        [Route("ResetPassword")]
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