﻿using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class Sha256Adapter
    {
        public Sha256Adapter()
        {
        }

        /// <summary>
        /// Gets the hashed password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public string GetHashedPassword(string password)
        {
            //hash
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }
}