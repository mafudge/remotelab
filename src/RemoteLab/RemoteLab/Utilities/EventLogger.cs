using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace RemoteLab.Utilities
{
    public class EventLogger
    {
        public  EventLog Log {get; private set; }
        public EventLogger() 
        {
            this.Log = new EventLog("Application");
            this.Log.Source = ".NET Runtime";               
        }

        public EventLogger( EventLog e)
        {
            this.Log = e;
        }

        public void WriteToLog(string message)
        {
            this.Log.WriteEntry(message);
        }
    }
}