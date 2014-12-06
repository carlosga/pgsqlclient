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
    internal sealed class PgTypeCollection 
        : List<PgType>
    {
        #region · Properties ·

        public PgType this[string name] 
        {
            get { return (PgType)this[this.IndexOf(name)]; }
            set { this[this.IndexOf(name)] = (PgType)value; }
        }

        #endregion

        #region · Methods ·
    
        public bool Contains(int oid)
        {
            return (this.IndexOf(oid) != -1);
        }
        
        public int IndexOf(int oid)
        {
            int index = 0;

            foreach (PgType item in this)
            {
                if (item.Oid == oid)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public int IndexOf(string name)
        {
            int index = 0;

            foreach (PgType item in this)
            {
                if (item.Name.CaseInsensitiveCompare(name))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void RemoveAt(string typeName)
        {
            this.RemoveAt(this.IndexOf(typeName));
        }

        public PgType Add(int          oid
                        , string       name
                        , PgDataType   dataType
                        , int          elementType
                        , PgTypeFormat formatCode
                        , int          size)
        {
            return this.Add(new PgType(oid, name, dataType, elementType, formatCode, size));
        }

        public PgType Add(int          oid
                        , string       name
                        , PgDataType   dataType
                        , int          elementType
                        , PgTypeFormat formatCode
                        , int          size
                        , string       delimiter)
        {
            return this.Add(new PgType(oid, name, dataType, elementType, formatCode, size, delimiter));
        }

        public PgType Add(int          oid
                        , string       name
                        , PgDataType   dataType
                        , int          elementType
                        , PgTypeFormat formatCode
                        , int          size
                        , string       delimiter
                        , string       prefix)
        {
            return this.Add(new PgType(oid, name, dataType, elementType, formatCode, size, delimiter, prefix));
        }

        public new PgType Add(PgType type)
        {
            return this.Add(type);
        }

        #endregion
    }
}
