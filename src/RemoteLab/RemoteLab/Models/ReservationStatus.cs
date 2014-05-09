using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemoteLab.Models
{
    public enum ReservationStatus
    {
        Unknown,
        ExistingReservation,
        NewReservation,
        PoolFull
    }
}