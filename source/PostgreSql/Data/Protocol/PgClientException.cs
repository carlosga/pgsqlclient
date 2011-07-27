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
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientException 
        : Exception
    {	
        #region · Fields ·
        
        private string					message;
        private PgClientErrorCollection	errors;

        #endregion

        #region · Properties ·
        
        public new string Message
        {
            get { return this.message; }
        }

        public PgClientErrorCollection Errors
        {
            get { return this.errors; }
        }
        
        #endregion

        #region · Constructors ·
        
        public PgClientException(string message) : base(message)
        {
            this.errors = new PgClientErrorCollection();
            this.message = message;
        }

        #endregion
    }
}
