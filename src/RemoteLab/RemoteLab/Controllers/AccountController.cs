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
using RemoteLab.DirectoryServices;
using RemoteLab.Services;
using RemoteLab.Utilities;


namespace RemoteLab.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {

        private readonly IDirectoryServices Auth;
        private readonly RemoteLabService Svc;

        public AccountController(IDirectoryServices Auth, RemoteLabService Svc) 
        {
            this.Auth = Auth;
            this.Svc = Svc;
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
                    // Build the set of claims for the authenticated user for the various Pools & application administrator
                    var ClaimsUtil = new ClaimsUtility(Auth, await this.Svc.GetPoolsAsync(), Properties.Settings.Default.AdministratorADGroup);                   
                    var identity = ClaimsUtil.BuildClaimsIdentityForUser(model.UserName);
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
            HttpContext.Session.Clear();
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }


        // GET: /Account/Roles
        [Authorize]
        public async Task<ActionResult> Roles()
        {
            var pools = await this.Svc.GetPoolsAsync();
            ViewBag.User = (ClaimsPrincipal)HttpContext.User;
            return View(pools);
        }

        // Post: /Account/RolesCheck
        [AdministratorAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RolesCheck(FormCollection form)
        {
            var UserName = (String)form["UserName"];
            var pools = await this.Svc.GetPoolsAsync();
            var ClaimsUtil = new ClaimsUtility(Auth, await this.Svc.GetPoolsAsync(), Properties.Settings.Default.AdministratorADGroup);
            var identity = ClaimsUtil.BuildClaimsIdentityForUser(UserName);
            ViewBag.User = new ClaimsPrincipal(identity);
            return View("Roles",pools);
        }


        // Post: /Account/RolesCheck
        [AdministratorAuthorize]
        [HttpGet]
        public async Task<ActionResult> RolesCheck()
        {
            return View();
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
            return Properties.Settings.Default.AdministratorADGroup.ToLowerInvariant().Contains(UserName.ToLowerInvariant());
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