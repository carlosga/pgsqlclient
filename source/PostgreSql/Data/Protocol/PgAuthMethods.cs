/*
 *  PgSqlClient - ADO.NET Data Provider for PostgreSQL 7.4+
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. 
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2003, 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System.Security.Cryptography;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal class MD5Authentication
    {
        #region · Static Methods ·

        public static string GetMD5Hash(byte[] salt, string password)
        {
            using (HashAlgorithm csp = MD5.Create())
            {
                StringBuilder md5    = new StringBuilder();
                int           length = Encoding.Default.GetByteCount(password);
                byte[]        data   = new byte[salt.Length + length];

                Encoding.Default.GetBytes(password, 0, length, data, 0);

                salt.CopyTo(data, length);

                // Calculate hash value
                byte[] hash = csp.ComputeHash(data);

                // Calculate MD5 string
                for (int i = 0; i < hash.Length; i++)
                {
                    md5.Append(hash[i].ToString("x2"));
                }

                return md5.ToString();
            }
        }

        #endregion
    }
}