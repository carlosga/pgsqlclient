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

using System;
using System.Collections.Generic;
using System.Globalization;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgCharactersetCollection : 
        List<PgCharacterSet>
    {
        #region · Properties ·

        public PgCharacterSet this[string name] 
        {
            get { return (PgCharacterSet)this[this.IndexOf(name)]; }
            set { this[this.IndexOf(name)] = (PgCharacterSet)value; }
        }

        #endregion

        #region · Methods ·
    
        public bool Contains(string characterset)
        {
            return (this.IndexOf(characterset) != -1);
        }
        
        public int IndexOf(string characterset)
        {
            int index = 0;
            
            foreach (PgCharacterSet item in this)
            {
                if (item.Name.CaseInsensitiveCompare(characterset))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void RemoveAt(string charset)
        {
            this.RemoveAt(this.IndexOf(charset));
        }

        public PgCharacterSet Add(string charset, string systemCharset)
        {
            PgCharacterSet cs = new PgCharacterSet(charset, systemCharset);

            this.Add(cs);

            return cs;
        }

        public PgCharacterSet Add(string charset, int cp)
        {
            PgCharacterSet cs = new PgCharacterSet(charset, cp);

            this.Add(cs);

            return cs;
        }

        #endregion
    }
}
