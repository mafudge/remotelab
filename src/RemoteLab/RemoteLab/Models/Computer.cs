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
        [Display( Name="Computer Name", ShortName = "Computer")]
        public string ComputerName { get; set;}

        [MaxLength(50)]
        [Display( Name = "User Name", ShortName = "User")]
        public string UserName { get; set; }

        
        [Display( Name = "Reserved On", ShortName = "Reserved")]
        public DateTime? Reserved { get; set; }

        [Display(Name = "Logged On", ShortName = "Logon")]
        public DateTime? Logon { get; set; }

        public Pool Pool { get; set; }
    }
}