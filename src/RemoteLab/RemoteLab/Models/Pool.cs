using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RemoteLab.Models
{
    public class Pool
    {
        [Key]
        [MaxLength(50)]
        [Display(Name = "Pool Name", ShortName = "Pool")]
        [Required]
        public String PoolName {get; set;}

        [MaxLength(100)]
        [Display(Name = "Active Directory User Group", ShortName = "AD User Grp")]
        public String ActiveDirectoryUserGroup { get; set; }

        [Url]
        [MaxLength(200)]
        [Display(Name = "Logo Url", ShortName = "Logo")]
        public String Logo { get; set; }

        [MaxLength(100)]
        [Display(Name = "Active Directory Pool Admin Group", ShortName = "AD Admin Grp")]
        public String ActiveDirectoryAdminGroup { get; set; }

        [MaxLength(100)]
        [Display(Name = "Email Notify List", ShortName = "Email")]
        public String EmailNotifyList {get; set; }

        [Display(Name = "RDP TCP Port", ShortName = "Port")]
        [Required]
        public int RdpTcpPort { get; set; }

        [Display(Name = "Cleanup (in Minutes)", ShortName = "Cleanup")]
        [Required]
        public int CleanupInMinutes { get; set; }

    }
}