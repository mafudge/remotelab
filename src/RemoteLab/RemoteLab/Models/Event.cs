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
        public string EventName { get; set; }

        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        [MaxLength(50)]
        [Required]
        public string ComputerName { get; set; }

        [MaxLength(50)]
        [Required]
        public String PoolName { get; set; }        

        [Required]
        public DateTime DtStamp { get; set; }

    }
}