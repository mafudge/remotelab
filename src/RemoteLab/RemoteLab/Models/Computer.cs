using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RemoteLab.Models
{
    public class Computer
    {
        [Key]
        [MaxLength(50)]
        public string ComputerName { get; set;}

        [MaxLength(50)]
        public string UserName { get; set; }

        public DateTime? Reserved { get; set; }

        public DateTime? Logon { get; set; }

        public Pool Pool { get; set; }
    }
}