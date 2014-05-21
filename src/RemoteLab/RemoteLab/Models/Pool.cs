using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using RemoteLab.Utilities;

namespace RemoteLab.Models
{
    public class Pool
    {
        [Key]
        [MaxLength(50)]
        [Display(Name = "Pool Name", ShortName = "Pool", Prompt = "Must be unique")]
        [Required]
        public String PoolName {get; set;}

        [MaxLength(100)]
        [Display(Name = "Active Directory User Group", ShortName = "AD User Grp", Description="Members of this group can connect to computers in this pool.", Prompt="AD Group name for users allowed to connect to this pool")]
        [ADGroupValid(ErrorMessage="The Active Directory group does not exist.")]
        public String ActiveDirectoryUserGroup { get; set; }

        [MaxLength(100)]
        [Display(Name = "Active Directory Pool Admin Group", ShortName = "AD Admin Grp", Description = "Members of this group can administer this pool.", Prompt = "AD Group name for users allowed to admin this pool")]
        [ADGroupValid(ErrorMessage = "The Active Directory group does not exist.")]
        public String ActiveDirectoryAdminGroup { get; set; }

        [MaxLength(100)]
        [Display(Name = "Email Notify List", ShortName = "Email", Prompt="Comma-separated list of emails to be notified of pool events")]
        public String EmailNotifyList {get; set; }

        [Display(Name = "RDP TCP Port", ShortName = "Port", Prompt = "Computers in the pool listen on this port for RDP")]
        [Required]
        public int RdpTcpPort { get; set; }

        [Display(Name = "Cleanup (in Minutes)", ShortName = "Cleanup", Prompt = "Release unused reservations after ? minutes")]
        [Required]
        public int CleanupInMinutes { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Remote Administrator User", ShortName = "Remote User", Prompt = "Domain user account with admin access to comptuters in the pool")]
        public string RemoteAdminUser { get; set;}

        [Required]
        [DataType(DataType.Password)]
        [UIHint("Password")]
        [MaxLength(255)]
        [Display( Name = "Remote Administrator Password", ShortName = "Admin Password", Prompt = "Password for the domain account")]
        public string RemoteAdminPassword { get; set; }

        [Required]
        [DataType(DataType.Html)]
        [AllowHtml]
        [UIHint("tinymce_full_compressed")]
        [Display( Name = "Welcome Message (New Reservations)", ShortName="Welcome Message", Prompt = "This message will display on the new resevration page")]
        public string WelcomeMessage { get; set; }

        [MaxLength(255)]
        public string InitializationVector {get; set;}
    }
}