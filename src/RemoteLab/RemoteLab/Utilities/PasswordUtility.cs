using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Effortless.Net.Encryption;

namespace RemoteLab.Utilities
{
     /// <summary>
     /// Deals with the reversable encryption requirement for secure password storage.
     /// </summary>
    public class PasswordUtility
    {

        public byte[] Key {get; private set; }

        public PasswordUtility( string key )
        {
            try
            {
                this.Key = this.StringToByteArray(key);
            }
            catch (Exception e) 
            {
                throw new Exception("Invalid Encryption Key. Please make sure to run setup.");
            }
        }


        public string Encrypt(string PlainTextPassword, string InitializationVector)
        {
            byte[] vector = this.StringToByteArray(InitializationVector);
            return Strings.Encrypt(PlainTextPassword, this.Key, vector);
        }

        public string Decrypt(string EncryptedPassword, string InitializationVector)
        {
            byte[] vector = this.StringToByteArray(InitializationVector);
            return Strings.Decrypt(EncryptedPassword, this.Key, vector);
        }

        public string NewInitializationVector()
        {
            return ByteArrayToString(Bytes.GenerateIV());
        }

        /// <summary>
        /// Converts a comma-separated string of byte integers eg. "123, 255, 0, 40" into a byte array
        /// </summary>
        /// <param name="CommaSeparatedListOfIntBytes"></param>
        /// <returns></returns>
        private byte[] StringToByteArray(String CommaSeparatedListOfIntBytes) 
        {
            return CommaSeparatedListOfIntBytes.Split(',').Select( s=> Byte.Parse(s.Trim()) ).ToArray<Byte>();
        }


        /// <summary>
        /// Converts an array of bytes into a comma-separated string of byte integers eg. "123, 255, 0, 40" 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private string ByteArrayToString( byte[] array )
        {
            var result = String.Empty;
            foreach (byte b in array) 
            {
                result += String.Format("{0:d}, ",b);
            }
            return result.Substring(0, result.Length-2);
        }

    }
}