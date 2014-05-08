using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;

namespace RemoteLab.Authentication  
{
    /// <summary>
    /// Active Directory Authentication
    /// </summary>
    public class ActiveDirectoryAuthentication : IAuthentication 
    {
        public string Domain { get; set; }
        private string Path;
        public ActiveDirectoryAuthentication(string domain) 
        {
            this.Domain = domain;
            this.Path = "LDAP://" + domain;
        }

        public bool Authenticate(string UserName, string Password) 
        {
            bool authenticated = false;
            try
            {
                DirectoryEntry entry = new DirectoryEntry(this.Path,UserName,Password);
                object result = entry.NativeObject;
                authenticated = true;
            }
            catch (DirectoryServicesCOMException e) 
            { 
                // authentication fail 
            }
            return authenticated;
        }

    }
}
