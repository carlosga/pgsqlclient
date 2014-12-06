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
    internal sealed class PgClientErrorCollection 
        : List<PgClientError>
    {
        #region · Properties ·

        public PgClientError this[string errorMessage] 
        {
            get { return (PgClientError)this[IndexOf(errorMessage)]; }
            set { this[IndexOf(errorMessage)] = (PgClientError)value; }
        }

        #endregion

        #region · Methods ·
    
        public bool Contains(string errorMessage)
        {
            return (this.IndexOf(errorMessage) != -1);
        }
        
        public int IndexOf(string errorMessage)
        {
            int index = 0;

            foreach(PgClientError item in this)
            {
                if (item.Message.CaseInsensitiveCompare(errorMessage))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void RemoveAt(string errorMessage)
        {
            this.RemoveAt(IndexOf(errorMessage));
        }

        public new PgClientError Add(PgClientError error)
        {
            base.Add(error);

            return error;
        }

        public PgClientError Add(string severity, string message, string code)
        {
            return Add(new PgClientError(severity, code, message));
        }

        #endregion
    }
}