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
    internal sealed class PgSequences 
        : PgSchema
    {
        #region · Constructors ·

        public PgSequences(PgConnection connection)
            : base(connection)
        {
        }

        #endregion

        #region · Protected Methods ·

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS SEQUENCE_CATALOG, " +
                    "pg_namespace.nspname AS SEQUENCE_SCHEMA, " +
                    "pg_class.relname AS SEQUENCE_NAME, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_class " +
                    "left join pg_namespace ON pg_class.relnamespace = pg_namespace.oid " +
                    "left join pg_description ON pg_class.oid = pg_description.objoid " +
                "WHERE pg_class.relkind = 'S' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // SEQUENCE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // SEQUENCE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // SEQUENCE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }
            }

            sql += " ORDER BY pg_namespace.nspname, pg_class.relname";

            return sql;
        }

        #endregion
    }
}
