using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;


namespace RemoteLab.DirectoryServices  
{
    /// <summary>
    /// Active Directory Authentication
    /// </summary>
    public class ActiveDirectory : IDirectoryServices 
    {
        public string Domain { get; set; }
        private string Path;
        public ActiveDirectory(string domain) 
        {
            this.Domain = domain;
            this.Path = "LDAP://" + domain;
        }

        public bool Authenticate(string UserName, string Password) 
        {
            bool authenticated = false;
            try
            {
                using(DirectoryEntry entry = new DirectoryEntry(this.Path,UserName,Password))
                {
                    object result = entry.NativeObject;
                    authenticated = true;
                }
            }
            catch (DirectoryServicesCOMException e) 
            { 
                // authentication fail 
            }
            return authenticated;
        }

        public bool UserIsInGroup(string UserName, string GroupName) 
        {
                using(var context = new PrincipalContext(ContextType.Domain,this.Domain))
                using (var user = UserPrincipal.FindByIdentity(context, UserName))
                using (var group = GroupPrincipal.FindByIdentity(context, GroupName))
                {
                    return (group != null && user.IsMemberOf(group));
                }
        }

        [Obsolete]
        public IEnumerable<String> GetGroups(string UserName) 
        {
            var groups = new List<String>();
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, this.Domain))
            {
                try 
	            {	        
		            UserPrincipal user = UserPrincipal.FindByIdentity(context,UserName);
                    foreach (GroupPrincipal g in user.GetAuthorizationGroups())
                    {
                        groups.Add(g.SamAccountName);
                    }
	            }
	            catch (Exception)
	            {		
		            throw; // for now.
	            }
            }
            return groups;
        }


        public bool GroupExists(string GroupName)
        {
            using (var context = new PrincipalContext(ContextType.Domain, this.Domain))
            using (var group = GroupPrincipal.FindByIdentity(context, GroupName))
            {
                return (group != null); 
            }

        }
    }
}
