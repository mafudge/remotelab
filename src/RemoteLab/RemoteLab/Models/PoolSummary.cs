using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace RemoteLab.Models
{

    public class PoolSummary
    {
        [Key]
        [MaxLength(50)]
        public String PoolName { get; set; }

        public int PoolCount { get; set; }

        public int PoolInUse { get; set; }

        public int PoolAvailable { get; set; }

    }
}