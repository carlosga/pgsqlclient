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

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgTriggers 
        : PgSchema
    {
        #region · Constructors ·

        public PgTriggers(PgConnection connection)
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
                    "pg_class.relnamespace AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "current_database() AS TRIGGER_CATALOG, " +
                    "pg_namespace.nspname AS TRIGGER_SCHEMA, " +
                    "pg_proc.proname AS TRIGGER_NAME, " +
                    "pg_language.lanname AS PROCEDURE_LANGUAGE, " +
                    "pg_proc.proisagg AS IS_AGGREGATE, " +
                    "pg_proc.prosecdef AS IS_SECURITY_DEFINER, " +
                    "pg_proc.proisstrict AS IS_STRICT, " +
                    "pg_proc.proretset AS RETURNS_SET " +
                "FROM " +
                    "pg_trigger " +
                    "left join pg_class ON pg_trigger.tgconstrrelid = pg_class.oid " +
                    "left join pg_proc ON pg_trigger.tgfoid = pg_proc.oid " +
                    "left join pg_namespace ON pg_proc.pronamespace = pg_namespace.oid " +
                    "left join pg_language ON pg_proc.prolang = pg_language.oid ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // TABLE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // TABLE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_class.relnamespace = '{0}'", restrictions[1]);
                }

                // TABLE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }

                // TRIGGER_NAME
                if (restrictions.Length > 3 && restrictions[3] != null)
                {
                    sql += String.Format(" and pg_proc.proname = '{0}'", restrictions[3]);
                }
            }

            sql += " ORDER BY pg_namespace.nspname, pg_proc.proname";

            return sql;
        }

        #endregion
    }
}
