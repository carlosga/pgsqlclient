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
using System.Collections;
using System.Globalization;

namespace PostgreSql.Data.PostgreSqlClient
{
    [Serializable]
    public sealed class PgErrorCollection 
        : ICollection, IEnumerable
    {
        #region · Fields ·

        private ArrayList errors;

        #endregion

        #region · Properties ·

        public PgError this[string errorMessage] 
        {
            get { return (PgError)this.errors[errors.IndexOf(errorMessage)]; }
            set { this.errors[errors.IndexOf(errorMessage)] = (PgError)value; }
        }

        public PgError this[int errorIndex] 
        {
            get { return (PgError)this.errors[errorIndex]; }
            set { this.errors[errorIndex] = (PgError)value; }
        }

        public int Count
        {
            get { return this.errors.Count; }
        }

        public bool IsSynchronized
        {
            get { return this.errors.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return this.errors.SyncRoot; }
        }

        #endregion

        #region · Constructors ·

        internal PgErrorCollection()
        {
            this.errors = new ArrayList();
        }

        #endregion

        #region · Methods ·

        public IEnumerator GetEnumerator()
        {
            return errors.GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            this.errors.CopyTo(array, index);
        }
            
        internal PgError Add(PgError error)
        {
            this.errors.Add(error);

            return error;
        }

        internal PgError Add(string severity, string message, string code)
        {
            PgError error = new PgError(severity, code, message);

            return this.Add(error);
        }

        #endregion
    }
}
