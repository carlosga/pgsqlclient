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
using System.Data;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Schema
{
    internal abstract class PgSchema
    {
        #region · Fields ·

        private PgConnection connection;

        #endregion

        #region · Protected Properties ·

        public PgConnection Connection
        {
            get { return this.connection; }
        }

        #endregion

        #region · Constructors ·

        public PgSchema(PgConnection connection)
        {
            this.connection = connection;
        }

        #endregion

        #region · Abstract Methods ·

        protected abstract string BuildSql(string[] restrictions);

        #endregion

        #region · Methods ·

        public DataTable GetSchema(string collectionName, string[] restrictions)
        {
            DataTable		dataTable = null;
            PgDataAdapter	adapter = null;
            PgCommand		command = new PgCommand();
            
            try
            {
                command.Connection	= connection;
                command.CommandText = this.BuildSql(this.ParseRestrictions(restrictions));

                adapter = new PgDataAdapter(command);
                dataTable = new DataTable(collectionName);

                adapter.Fill(dataTable);

                dataTable = this.ProcessResult(connection, dataTable);
            }
            catch (PgException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PgException(ex.Message);
            }
            finally
            {
                command.Dispose();
                adapter.Dispose();
            }

            return dataTable;
        }

        #endregion

        #region · Protected Methods ·

        protected virtual string[] ParseRestrictions(string[] restrictions)
        {
            return restrictions;
        }

        protected virtual DataTable ProcessResult(PgConnection connection, DataTable schema)
        {
            return schema;
        }

        #endregion
    }
}
