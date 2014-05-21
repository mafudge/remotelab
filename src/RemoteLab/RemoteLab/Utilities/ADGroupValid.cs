using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RemoteLab.DirectoryServices;

namespace RemoteLab.Utilities
{
    public class ADGroupValid : ValidationAttribute
    {
        IDirectoryServices Auth {get; set;}

        public ADGroupValid()
        {
            this.Auth = new ActiveDirectory(Properties.Settings.Default.ActiveDirectoryFqdn);
        }
        public  override bool IsValid(object value)
        {                
            return Auth.GroupExists((string)value);
        }
    }
}