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
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    public class PostgreSqlClientFactory 
        : DbProviderFactory
    {
        #region · Static Fields ·

        public static readonly PostgreSqlClientFactory Instance = new PostgreSqlClientFactory();

        #endregion

        #region · Constructors ·

        private PostgreSqlClientFactory() 
            : base()
        {
        }

        #endregion

        #region · Properties ·

        public override bool CanCreateDataSourceEnumerator
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region · Methods ·

        public override DbCommand CreateCommand()
        {
            return new PgCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new PgCommandBuilder();
        }

        public override DbConnection CreateConnection()
        {
            return new PgConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new PgConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new PgDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new PgParameter();
        }

        #endregion
    }
}
