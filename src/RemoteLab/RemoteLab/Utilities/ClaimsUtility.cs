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
    public class ClaimsUtility
    {

        private IEnumerable<Pool> Pools;
        private IDirectoryServices Auth;
        private string AdminGroup;

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

            // Admin claim check
            if (Auth.UserIsInGroup(UserName, this.AdminGroup))
            {
                claims.Add(new Claim(ClaimTypes.Role, this.AdminGroup));
            }

            // Build claims for Pool Users and Admins
            foreach (Pool p in this.Pools)
            {
                var group = p.ActiveDirectoryAdminGroup;
                if (group != null &&
                    !claims.Exists(c => c.Value.Equals(group, StringComparison.InvariantCultureIgnoreCase)) &&
                    this.Auth.UserIsInGroup(UserName, group))
                {

                    claims.Add(new Claim(ClaimTypes.Role, group));
                }

                group = p.ActiveDirectoryUserGroup;
                if (group != null &&
                    !claims.Exists(c => c.Value.Equals(group, StringComparison.InvariantCultureIgnoreCase)) &&
                    this.Auth.UserIsInGroup(UserName, group))
                {
                    claims.Add(new Claim(ClaimTypes.Role, group));
                }
            }

            identity.AddClaims(claims);

            return identity;
        }

    }
}