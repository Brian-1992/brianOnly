using System;
using System.Security.Cryptography;

namespace JCLib
{
    public class Encrypt
    {
        public static string GetSalt()
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            return Convert.ToBase64String(salt);
        }

        public static string GetHash(string pwd, string salt_str)
        {
            byte[] salt = Convert.FromBase64String(salt_str);
            var pbkdf2 = new Rfc2898DeriveBytes(pwd, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashResult = new byte[36];
            Array.Copy(salt, 0, hashResult, 0, 16);
            Array.Copy(hash, 0, hashResult, 16, 20);

            return Convert.ToBase64String(hashResult);
        }

        public static bool CheckHash(string input_pwd, string db_pwd, string db_salt)
        {
            string hashValue = GetHash(input_pwd, db_salt);
            return (input_pwd == hashValue);
        }
    }
}
