using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace RemoteLab.Models
{

    public class PoolSummary : IDbAsyncEnumerable 
    {
        [Key]
        [MaxLength(50)]
        [Display(Name = "Pool Name", ShortName="Pool")]
        public String PoolName { get; set; }

        [Display(Name = "Active Directory User Group", ShortName="AD User Grp")]
        public String ActiveDirectoryUserGroup { get; set; }

        [Url]
        [MaxLength(200)]
        [Display(Name = "Logo Url", ShortName = "Logo")]
        public String Logo { get; set; }

        [MaxLength(100)]
        [Display(Name = "Active Directory Pool Admin Group", ShortName = "AD Admin Grp")]
        public String ActiveDirectoryAdminGroup { get; set; }

        [MaxLength(100)]
        [Display(Name="Email Notify List", ShortName="Email")]
        public String EmailNotifyList { get; set; }

        
        [Display(Name = "RDP TCP Port",ShortName="Port")]
        public int RdpTcpPort { get; set; }

        [Display(Name = "Cleanup (in Minutes)", ShortName = "Cleanup")]
        public int CleanupInMinutes { get; set; }

        [Display(Name = "Total Seats", ShortName = "Total")]
        public int PoolCount { get; set; }

        [Display(Name = "In Use", ShortName = "In Use")]
        public int PoolInUse { get; set; }


        [Display(Name = "Avaliable", ShortName = "Available")]
        public int PoolAvailable { get; set; }


        public IDbAsyncEnumerator GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}