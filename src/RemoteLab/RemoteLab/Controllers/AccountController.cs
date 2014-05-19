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
using RemoteLab.Services;


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
                    var identity = BuildClaimsIdentity(Auth, model.UserName, Properties.Settings.Default.AdministratorADGroup, await this.Svc.GetPoolsAsync());
                   
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

        [Authorize]
        public ActionResult Roles()
        {
            return View();
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #region Helpers

        //TODO:Refactor into its own class for unit testing
        private ClaimsIdentity BuildClaimsIdentity(IDirectoryServices Auth, string UserName, string AdminGroup, IEnumerable<Pool> pools)
        {
            var identity = new ClaimsIdentity( new [] { new Claim(ClaimTypes.Name, UserName) }, 
                    DefaultAuthenticationTypes.ApplicationCookie,
                    ClaimTypes.Name, 
                    ClaimTypes.Role);

            var claims = new List<Claim>();

            // Admin claim check
            if (Auth.UserIsInGroup(UserName, AdminGroup)) 
            {
                claims.Add(new Claim(ClaimTypes.Role, AdminGroup));
            }

            // Build claims for Pool Users and Admins
            foreach(Pool p in pools)
            {
                var group = p.ActiveDirectoryAdminGroup;
                if (group != null && 
                    !claims.Exists( c=>c.Value.Equals(group,StringComparison.InvariantCultureIgnoreCase)) && 
                    Auth.UserIsInGroup(UserName, group))
                {
                            
                    claims.Add(new Claim( ClaimTypes.Role, group));
                }

                group = p.ActiveDirectoryUserGroup;
                if (group != null &&
                    !claims.Exists(c => c.Value.Equals(group, StringComparison.InvariantCultureIgnoreCase)) && 
                    Auth.UserIsInGroup(UserName, group))
                {
                    claims.Add(new Claim(ClaimTypes.Role, group));
                }
            }

            identity.AddClaims( claims );

                return identity;
        }
   
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