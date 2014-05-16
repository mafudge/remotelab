using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemoteLab.Models
{
    public class RemoteLabViewModel
    {
        public ReservationStatus ReservationStatus { get; set; }
        public Computer RemoteLabComputer { get; set; }
        public Pool Pool { get; set; }
        public String CurrentUser { get; set; }

    }
}