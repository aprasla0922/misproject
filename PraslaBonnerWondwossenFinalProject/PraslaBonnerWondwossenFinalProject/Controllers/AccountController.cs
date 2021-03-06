﻿using System;

using System.Globalization;

using System.Linq;

using System.Security.Claims;

using System.Threading.Tasks;

using System.Web;

using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

using Microsoft.Owin.Security;

using PraslaBonnerWondwossenFinalProject.Models;



namespace PraslaBonnerWondwossenFinalProject.Controllers

{
    [Authorize]

    public class AccountController : Controller

    {
        private AppDbContext db = new AppDbContext();

        private ApplicationSignInManager _signInManager;

        private AppUserManager _userManager;



        public AccountController()

        {

        }



        public AccountController(AppUserManager userManager, ApplicationSignInManager signInManager)

        {

            UserManager = userManager;

            SignInManager = signInManager;

        }



        public ApplicationSignInManager SignInManager

        {

            get

            {

                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

            }

            private set

            {

                _signInManager = value;

            }

        }



        public AppUserManager UserManager

        {

            get

            {

                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>();

            }

            private set

            {

                _userManager = value;

            }

        }



        //

        // GET: /Account/Login

        [AllowAnonymous]

        public ActionResult Login(string returnUrl)

        {

            if (HttpContext.User.Identity.IsAuthenticated) //user has been redirected here from a page they're not authorized to see

            {

                return View("Error", new string[] { "Access Denied" });

            }
            AuthenticationManager.SignOut(); //this removes any old cookies hanging around

            ViewBag.ReturnUrl = returnUrl;

            return View();

        }



        //

        // POST: /Account/Login

        [HttpPost]

        [AllowAnonymous]

        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)

        {

            if (!ModelState.IsValid)

            {

                return View(model);

            }



            // This doesn't count login failures towards account lockout

            // To enable password failures to trigger account lockout, change to shouldLockout: true

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)

            {

                case SignInStatus.Success:
                    AppUserManager manager = new AppUserManager(new UserStore<AppUser>(db));
                    var user = await manager.FindAsync(model.Email, model.Password);
                    if (manager.IsInRole(user.Id,"Customer"))
                    {
                        //redirects user to Create A bank account page if User nodernot have a bank account
                        if (user.BankAccounts.Count() == 0) {
                            return RedirectToAction("Create", "BankAccounts");
                        }
                        return RedirectToAction("Index", "Customers");
                    } else if (manager.IsInRole(user.Id,"Employee"))
                    {
                        return RedirectToAction("Index", "Employees");
                    }
                    return RedirectToAction("Index","RoleAdmin");

                case SignInStatus.Failure:

                default:
                    ModelState.AddModelError("", "Incorrect Email or Password");
                    return View(model);

            }

        }





        //

        // GET: /Account/Register

        [AllowAnonymous]

        public ActionResult Register()

        {

            return View();

        }



        //

        // POST: /Account/Register

        [HttpPost]

        [AllowAnonymous]

        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Register(RegisterViewModel model)

        {

            if (ModelState.IsValid)

            {

                //Add fields to user here so they will be saved to do the database

                var user = new AppUser { UserName = model.Email, Email = model.Email, FName = model.FName, LName=model.LName, Address=model.Address, PhoneNumber =model.PhoneNumber, Birthday=model.Birthday, isActive=model.isActive, Middle=model.Middle, State=model.State, City=model.City, Zip=model.Zip };

                var result = await UserManager.CreateAsync(user, model.Password);



                // Once you get roles working, you may want to add users to roles upon creation

                // await UserManager.AddToRoleAsync(user.Id, "User");

                // --OR--

                await UserManager.AddToRoleAsync(user.Id, "Customer");





                if (result.Succeeded)

                {

                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);



                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771

                    // Send an email with this link

                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Create", "BankAccounts");

                }

                AddErrors(result);

            }



            // If we got this far, something failed, redisplay form

            return View(model);

        }



        //

        // GET: /Account/ForgotPassword

        [AllowAnonymous]

        public ActionResult ForgotPassword()

        {

            return View();

        }



        //

        // POST: /Account/ForgotPassword

        [HttpPost]

        [AllowAnonymous]

        [ValidateAntiForgeryToken]

        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)

        {

            if (ModelState.IsValid)

            {

                var user = await UserManager.FindByNameAsync(model.Email);

                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)) || model.YearOfBirth != user.Birthday.Year)
                {

                    // Don't reveal that the user does not exist or is not confirmed

                    return View("ForgotPasswordConfirmation");

                }

                // Send an email with this link

                //string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                //                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                //
                //                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");



                //TODO: Figure out how to send an email after
                //EmailMessaging.SendEmail(user.Email, "Reset Password Confirmation", "This email worked");
                string url = Url.Action("ResetPassword", "Account",
                new System.Web.Routing.RouteValueDictionary(new { id = user.Id }),
                "http", Request.Url.Host);

                EmailMessaging.SendEmail("alijessnate@gmail.com", "Team 22:reset password", "Use the following link to reset your password: " + url );


                return RedirectToAction("ForgotPasswordConfirmation", "Account");

            }



            // If we got this far, something failed, redisplay form

            return View(model);

        }



        //

        // GET: /Account/ForgotPasswordConfirmation

        [AllowAnonymous]

        public ActionResult ForgotPasswordConfirmation()

        {
            return View();
        }



        //

        // GET: /Account/ResetPassword

        [AllowAnonymous]

        public ActionResult ResetPassword(string code)

        {

            return code == null ? View("Error") : View();

        }



        //

        // POST: /Account/ResetPassword

        [HttpPost]

        [AllowAnonymous]

        [ValidateAntiForgeryToken]

        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)

        {

            if (!ModelState.IsValid)

            {

                return View(model);

            }

            var user = await UserManager.FindByNameAsync(model.Email);

            if (user == null)

            {

                // Don't reveal that the user does not exist

                return RedirectToAction("ResetPasswordConfirmation", "Account");

            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);

            if (result.Succeeded)

            {

                return RedirectToAction("ResetPasswordConfirmation", "Account");

            }

            AddErrors(result);

            return View();

        }



        //

        // GET: /Account/ResetPasswordConfirmation

        [AllowAnonymous]

        public ActionResult ResetPasswordConfirmation()

        {

            return View();

        }



        //

        // POST: /Account/LogOff

        [HttpPost]

        [ValidateAntiForgeryToken]

        public ActionResult LogOff()

        {

            AuthenticationManager.SignOut();

            return RedirectToAction("Index", "Home");

        }



        //

        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]

        public ActionResult ExternalLoginFailure()

        {

            return View();

        }

        public ActionResult InactiveAccountError()
        {
            return View();
        }



        protected override void Dispose(bool disposing)

        {

            if (disposing)

            {

                if (_userManager != null)

                {

                    _userManager.Dispose();

                    _userManager = null;

                }



                if (_signInManager != null)

                {

                    _signInManager.Dispose();

                    _signInManager = null;

                }

            }



            base.Dispose(disposing);

        }



        #region Helpers

        // Used for XSRF protection when adding external logins

        private const string XsrfKey = "XsrfId";



        private IAuthenticationManager AuthenticationManager

        {

            get

            {

                return HttpContext.GetOwinContext().Authentication;

            }

        }



        private void AddErrors(IdentityResult result)

        {

            foreach (var error in result.Errors)

            {

                ModelState.AddModelError("", error);

            }

        }



        private ActionResult RedirectToLocal(string returnUrl)

        {

            if (Url.IsLocalUrl(returnUrl))

            {

                return Redirect(returnUrl);

            }

            return RedirectToAction("Index", "Home");

        }



        internal class ChallengeResult : HttpUnauthorizedResult

        {

            public ChallengeResult(string provider, string redirectUri)

                : this(provider, redirectUri, null)

            {

            }



            public ChallengeResult(string provider, string redirectUri, string userId)

            {

                LoginProvider = provider;

                RedirectUri = redirectUri;

                UserId = userId;

            }



            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }



            public override void ExecuteResult(ControllerContext context)

            {

                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };

                if (UserId != null)

                {

                    properties.Dictionary[XsrfKey] = UserId;

                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);

            }

        }

        #endregion

    }

}