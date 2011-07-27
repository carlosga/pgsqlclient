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


namespace PostgreSql.Data.Protocol
{
    internal sealed class PgRowDescriptor
    {
        #region · Fields ·

        private PgFieldDescriptor[] fields;

        #endregion

        #region · Properties ·

        public PgFieldDescriptor[] Fields
        {
            get { return this.fields; }
            set { this.fields = value; }
        }

        #endregion

        #region · Constructors ·

        public PgRowDescriptor()
        {
        }

        public PgRowDescriptor(int count)
        {
            this.fields = new PgFieldDescriptor[count];
        }

        #endregion
    }
}
