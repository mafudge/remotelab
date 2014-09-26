using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RemoteLab.CustomValidators
{
    public class NotAnEmailAttribute : ValidationAttribute
    {
        public NotAnEmailAttribute() {}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            String stringValue = (String)value;
            String userNameLabel = RemoteLab.Properties.Settings.Default.LoginFormUsernameLabel;
            if (stringValue.Contains("@"))
            {
                return new ValidationResult(String.Format("Please enter your {0} not your email address.",userNameLabel));
            }

            return null;
        }


    }

}