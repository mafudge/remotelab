using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RemoteLab.DirectoryServices;

namespace RemoteLab.Utilities
{
    /// <summary>
    /// For model validation, determines if value is an Active Directory Group
    /// </summary>
    public class ADGroupValidationAttribute : ValidationAttribute
    {
        IDirectoryServices Auth {get; set;}

        public ADGroupValidationAttribute()
        {
            this.Auth = new ActiveDirectory(Properties.Settings.Default.ActiveDirectoryDNSDomain);
        }
        public  override bool IsValid(object value)
        {                
            return Auth.GroupExists((string)value);
        }
    }
}