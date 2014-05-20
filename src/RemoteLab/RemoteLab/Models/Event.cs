using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RemoteLab.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        [Display(Name = "Event Name", ShortName = "Event")]
        public string EventName { get; set; }

        [MaxLength(50)]
        [Required]
        [Display(Name = "User Name", ShortName = "UserName")]
        public string UserName { get; set; }

        [MaxLength(50)]
        [Required]
        [Display( Name = "Computer Name", ShortName = "ComputerName")]
        public string ComputerName { get; set; }

        [MaxLength(50)]
        [Required]
        [Display( Name = "Pool Name", ShortName = "Pool")]
        public String PoolName { get; set; }        

        [Required]
        [Display(Name = "Date/Time Stamp", ShortName= "DT Stamp")]
        public DateTime DtStamp { get; set; }

    }
}