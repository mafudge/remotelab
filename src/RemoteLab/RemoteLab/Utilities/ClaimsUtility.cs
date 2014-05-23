using Microsoft.AspNet.Identity;
using RemoteLab.DirectoryServices;
using RemoteLab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace RemoteLab.Utilities
{
    /// <summary>
    /// Utility class to build a ClaimsIdentity for an authenticated user
    /// </summary>
    public class ClaimsUtility
    {

        private IEnumerable<Pool> Pools;
        private IDirectoryServices Auth;
        private string AdminGroup;

        public const string APPLICATION_ADMINISTRATOR = "APPLICATION_ADMINISTRATOR";            // Admin of this app
        public const string APPLICATION_POOL_ADMINISTRATOR = "APPLICATION_POOL_ADMINISTRATOR";  // Admin of at least 1 pool
        public const string APPLICATION_POOL_USER = "APPLICATION_POOL_USER";                    // User of at least 1 pool

        public ClaimsUtility( IDirectoryServices Auth, IEnumerable<Pool> Pools, string AdministratorGroup) 
        {
            this.Pools = Pools;
            this.Auth = Auth;
            this.AdminGroup = AdministratorGroup;
        }


        public  ClaimsIdentity BuildClaimsIdentityForUser(string UserName )
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, UserName) },
                    DefaultAuthenticationTypes.ApplicationCookie,
                    ClaimTypes.Name,
                    ClaimTypes.Role);

            var claims = new List<Claim>();

            // Build claims for Pool Users and Admins
            var adminCount = 0;
            var userCount = 0;
            var group = String.Empty;
            foreach (Pool p in this.Pools)
            {
                group = p.ActiveDirectoryAdminGroup;
                if (group != null && 
                    !claims.Exists(c => c.Value.Equals(group, StringComparison.InvariantCultureIgnoreCase)) &&
                    this.Auth.UserIsInGroup(UserName, group))
                {

                    claims.Add(new Claim(ClaimTypes.Role, group));
                    adminCount++;
                }

                group = p.ActiveDirectoryUserGroup;
                if (group != null &&
                    !claims.Exists(c => c.Value.Equals(group, StringComparison.InvariantCultureIgnoreCase)) &&
                    this.Auth.UserIsInGroup(UserName, group))
                {
                    claims.Add(new Claim(ClaimTypes.Role, group));
                    userCount++;
                }

            }
            if (adminCount> 0) { claims.Add(new Claim(ClaimTypes.UserData, APPLICATION_POOL_ADMINISTRATOR)); }
            if (userCount> 0) { claims.Add(new Claim(ClaimTypes.UserData, APPLICATION_POOL_USER)); }

            // Last Check: Is the User An Administrator of the Application?
            if (Auth.UserIsInGroup(UserName, this.AdminGroup))
            {
                claims.Add(new Claim(ClaimTypes.Role, this.AdminGroup));
                claims.Add(new Claim(ClaimTypes.UserData, APPLICATION_ADMINISTRATOR));
            }

            identity.AddClaims(claims);

            return identity;
        }

    }
}