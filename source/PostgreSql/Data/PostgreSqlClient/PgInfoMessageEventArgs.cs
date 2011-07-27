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
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgInfoMessageEventArgs 
        : EventArgs
    {
        #region · Fields ·

        private PgErrorCollection errors	= new PgErrorCollection();
        private string			  message	= String.Empty;

        #endregion

        #region · Properties ·

        public PgErrorCollection Errors
        {
            get { return this.errors; }
        }

        public string Message
        {
            get { return this.message; }
        }

        #endregion

        #region · Constructors ·

        internal PgInfoMessageEventArgs(PgClientException ex)
        {
            this.message = ex.Message;
            
            foreach (PgClientError error in ex.Errors)
            {
                PgError newError = new PgError();

                newError.Severity	= error.Severity;
                newError.Code		= error.Code;
                newError.Message	= error.Message;
                newError.Detail		= error.Detail;
                newError.Hint		= error.Hint;
                newError.Line		= error.Line;
                newError.Where		= error.Where;
                newError.Position	= error.Position;
                newError.Routine	= error.Routine;

                this.errors.Add(newError);
            }
        }

        #endregion
    }
}