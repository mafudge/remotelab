using System;
namespace RemoteLab.Authentication
{
    /// <summary>
    /// Implement this interface if you desire a different method of forms authentication such as LDAP
    /// </summary>
    public interface IAuthentication
    {
        bool Authenticate(string UserName, string Password);
    }
}
