using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RemoteLab.Models
{
    public class DownloadEventsViewModel
    {
        [Key]
        [Required]
        [Display(Name="Pool Name")]
        public string PoolName {get; set;}


        [Required]
        [Display(Name = "Start Date", Prompt="mm/dd/yyyy")]

        public DateTime StartDate {get; set;}

        [Required]
        [Display(Name = "End Date", Prompt = "mm/dd/yyyy")]
        public DateTime EndDate {get ;set;}

        [Required]
        [EnumDataType(typeof(DownloadEventsFormat))]
        [Display(Name = "Format")]
        public DownloadEventsFormat Format {get; set; }

    }

    public enum DownloadEventsFormat {
        CSV, XML, JSON
    }
}