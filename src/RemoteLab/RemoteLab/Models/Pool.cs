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
        public String PoolName {get; set;}
        
        
        public String ActiveDirectoryUserGroup { get; set; }

        [Url]
        [MaxLength(200)]
        public String Logo { get; set; }

        [MaxLength(100)]
        public String ActiveDirectoryAdminGroup { get; set; }

        [MaxLength(100)]
        public String EmailNotifyList {get; set; }

        public int RdpTcpPort { get; set; }

        public int CleanupInMinutes { get; set; }

    }
}