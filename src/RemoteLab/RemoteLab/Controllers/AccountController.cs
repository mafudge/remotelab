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

        private IDirectoryServices Auth;

        public AccountController(IDirectoryServices Auth) 
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
                    /* 
                     * Logon should be re-written to use unique url's for each pool.
                     * for example: http://remotelab/Auth/ischool ==> ischool pool.
                     * When user authenticates look up 3 AD groups for roles:
                     * 1) Remote Lab AdministratorGroup (Admin of the entire system)
                     * 2) Pool Administrator Group  (You can get this from Database Row for pool)
                     * 3) Pool Users Group (You can get this also from Database row for pool) 
                     * 
                     * Set each of these 3 roles as claims like this:
                     * identity.AddClaim(new Claim(ClaimTypes.Role, "IST-RemoteLab-Users")); 
                     * 
                     * Then in code you can verify with User.IsInRole("IST-RemoteLab-Users")
                     * 
                     * 
                     * Implement a view for when user is not in pool /notallowed or something.
                    */

                    // Admin group check 
                    if (Auth.UserIsInGroup(model.UserName, Properties.Settings.Default.AdministratorGroup)) {
                        identity.AddClaim( new Claim(ClaimTypes.Role, Properties.Resources.Administrator));
                    }


                    await SignInAsync(identity, false);
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
            return Properties.Settings.Default.AdministratorGroup.ToLowerInvariant().Contains(UserName.ToLowerInvariant());
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