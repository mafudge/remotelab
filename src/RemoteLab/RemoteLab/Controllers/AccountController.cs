using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using RemoteLab.Models;
using RemoteLab.Authentication;


namespace RemoteLab.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private IAuthentication Auth;

        public AccountController(IAuthentication Auth) 
        {
            this.Auth = Auth;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
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
            if (ModelState.IsValid)
            {   
                if (Auth.Authenticate(model.UserName, model.Password) )
                {
                    // Create the identity 
                    var identity = new ClaimsIdentity( new [] { new Claim(ClaimTypes.Name, model.UserName) }, 
                            DefaultAuthenticationTypes.ApplicationCookie,
                            ClaimTypes.Name, 
                            ClaimTypes.Role);
                    // Check if user is an administrator
                    if (IsAdministrator(model.UserName)) { identity.AddClaim(new Claim(ClaimTypes.Role,Properties.Resources.Administrator)); }
                    await SignInAsync(identity, model.RememberMe);
                    return RedirectToLocal(returnUrl);

                }
                else
                {
                    ModelState.AddModelError("", Properties.Resources.InvalidUserNameOrPassword);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Helpers
   
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private bool IsAdministrator(string UserName) {
            return Properties.Settings.Default.Administrators.ToLowerInvariant().Contains(UserName.ToLowerInvariant());
        }

        private async Task SignInAsync(ClaimsIdentity identity, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }



        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        #endregion
    }
}