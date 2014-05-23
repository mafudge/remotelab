using RemoteLab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;


namespace RemoteLab.Utilities
{
    /// <summary>
    /// Extension Method Class on Top of the ClaimsPrincipal Class
    /// </summary>
    public static  class ClaimsPrincipalExtensions
    {

        public static bool IsAdministrator(this ClaimsPrincipal User)
        {
            return User.Claims.Any(c => c.Type == ClaimTypes.UserData && 
                                        c.Value.Equals(ClaimsUtility.APPLICATION_ADMINISTRATOR, StringComparison.InvariantCultureIgnoreCase)
                                  );
        }

        public static bool IsAdministratorOfPool( this ClaimsPrincipal User, Pool Pool)
        {
            return User.Claims.Any( c=> c.Type==ClaimTypes.Role && 
                                         c.Value.Equals(Pool.ActiveDirectoryAdminGroup, StringComparison.InvariantCultureIgnoreCase) 
                                  );
        }

        public static bool IsUserOfPool(this ClaimsPrincipal User, Pool Pool)
        {
            return User.Claims.Any(c => c.Type == ClaimTypes.Role &&
                                         c.Value.Equals(Pool.ActiveDirectoryUserGroup, StringComparison.InvariantCultureIgnoreCase)
                                  );
        }

        public static bool IsAdministratorOfAnyPool( this ClaimsPrincipal User) 
        {
            return User.Claims.Any( c=> c.Type == ClaimTypes.UserData &&
                                        c.Value.Equals(ClaimsUtility.APPLICATION_POOL_ADMINISTRATOR, StringComparison.InvariantCultureIgnoreCase)
                                  );
        }

        public static bool IsUserOfAnyPool(this ClaimsPrincipal User)
        {
            return User.Claims.Any(c => c.Type == ClaimTypes.UserData &&
                                        c.Value.Equals(ClaimsUtility.APPLICATION_POOL_USER, StringComparison.InvariantCultureIgnoreCase)
                                  );
        }


    }
}