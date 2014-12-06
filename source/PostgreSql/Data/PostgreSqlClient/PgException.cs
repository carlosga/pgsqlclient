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

using PostgreSql.Data.Protocol;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Runtime.Serialization;

namespace PostgreSql.Data.PostgreSqlClient
{
    [Serializable]
    public class PgException
        : DbException
    {	
        #region · Fields ·
        
        private PgErrorCollection	errors;

        #endregion

        #region · Properties ·
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public PgErrorCollection Errors
        {
            get { return this.errors; }
        }
        
        #endregion

        #region · Constructors ·
        
        internal PgException() : base()
        {
            this.errors = new PgErrorCollection();
        }

        internal PgException(string message) : base(message)
        {
            this.errors = new PgErrorCollection();
        }

        internal PgException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.errors = new PgErrorCollection();
        }
        
        internal PgException(string message, PgClientException ex) : base(message)
        {
            this.errors	= new PgErrorCollection();

            this.GetPgExceptionErrors(ex);
        }

        #endregion

        #region · Private Methods ·

        private void GetPgExceptionErrors(PgClientException ex)
        {
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

                errors.Add(newError);
            }
        }

        #endregion
    }
}