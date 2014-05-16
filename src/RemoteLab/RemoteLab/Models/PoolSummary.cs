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
        public String PoolName { get; set; }

        [Display(Name = "Active Directory User Group", ShortName="AD User Grp")]
        public String ActiveDirectoryUserGroup { get; set; }

        [Url]
        [MaxLength(200)]
        [Display(Name = "Logo", ShortName = "Logo")]
        public String Logo { get; set; }

        [MaxLength(100)]
        [Display(Name = "Active Directory Pool Admin Group", ShortName = "AD Admin Grp")]
        public String ActiveDirectoryAdminGroup { get; set; }

        [MaxLength(100)]
        public String EmailNotifyList { get; set; }

        
        public int RdpTcpPort { get; set; }

        public int CleanupInMinutes { get; set; }

        public int PoolCount { get; set; }

        public int PoolInUse { get; set; }

      
        public int PoolAvailable { get; set; }


        public IDbAsyncEnumerator GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}