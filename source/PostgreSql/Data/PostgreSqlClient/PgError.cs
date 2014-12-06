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

namespace PostgreSql.Data.PostgreSqlClient
{
    [Serializable]
    public sealed class PgError
    {
        #region · Properties ·

        public string Severity
        {
            get;
            set;
        }
        
        public string Message
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public string Detail
        {
            get;
            set;
        }

        public string Hint
        {
            get;
            set;
        }

        public string Where
        {
            get;
            set;
        }

        public string Position
        {
            get;
            set;
        }

        public string File
        {
            get;
            set;
        }

        public int Line
        {
            get;
            set;
        }

        public string Routine
        {
            get;
            set;
        }

        #endregion

        #region · Constructors ·
        
        internal PgError()
        {
        }

        internal PgError(string message)
        {						
            this.Message = message;
        }

        internal PgError(string severity, string code, string message)
        {
            this.Severity = severity;
            this.Code	  = code;
            this.Message  = message;			
        }

        #endregion
    }
}
