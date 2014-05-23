using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RemoteLab.Utilities 
{
    /// <summary>
    /// Enegeral Convenience Class for Enums, to convert the string back to an enum
    /// </summary>
    public class EnumUtils
    {
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }
}