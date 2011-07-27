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
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgIndexes
        : PgSchema
    {
        #region · Constructors ·

        public PgIndexes(PgConnection connection)
            : base(connection)
        {
        }

        #endregion

        #region · Protected Methods ·

        protected override string BuildSql(string[] restrictions)
        {
            string sql = 
                "SELECT " +
                    "current_database() AS TABLE_CATALOG, " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "current_database() AS INDEX_CATALOG, " + 
                    "pg_namespidx.nspname AS INDEX_SCHEMA, " +
                    "pg_classidx.relname AS INDEX_NAME, " +
                    "pg_am.amname AS TYPE, " +
                    "pg_index.indisclustered AS IS_CLUSTERED, " +
                    "pg_index.indisunique AS IS_UNIQUE, " +
                    "pg_index.indisprimary AS IS_PRIMARY, " +
                    "pg_am.amindexnulls AS ALLOW_NULLS, " +
                    "pg_am.amcanmulticol AS IS_MULTICOLUMN, " +
                    "pg_am.amconcurrent AS IS_CONCURRENT, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_index " +
                    "left join pg_class ON pg_index.indrelid = pg_class.oid " + 
                    "left join pg_class as pg_classidx ON pg_index.indexrelid = pg_classidx.oid " +
                    "left join pg_namespace ON pg_classidx.relnamespace = pg_namespace.oid "  +
                    "left join pg_namespace as pg_namespidx ON pg_classidx.relnamespace = pg_namespidx.oid " +
                    "left join pg_am ON pg_classidx.relam = pg_am.oid left join pg_description ON pg_index.indexrelid = pg_description.objoid ";
            
            if (restrictions != null && restrictions.Length > 0)
            {
                // TABLE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // TABLE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // TABLE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }

                // INDEX_NAME
                if (restrictions.Length > 3 && restrictions[3] != null)
                {
                    sql += String.Format(" and pg_classidx.relname = '{0}'", restrictions[3]);
                }
            }

            sql += "ORDER BY pg_namespace.nspname, pg_class.relname, pg_classidx.relname";

            return sql;
        }

        #endregion
    }
}
