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
    internal sealed class PgFunctions 
        : PgSchema
    {
        #region · Constructors ·

        public PgFunctions(PgConnection connection)
            : base(connection)
        {            
        }

        #endregion

        #region · Protected Methods ·

        protected override string BuildSql(string[] restrictions)
        {
            string where = "";
            string sql =
                "SELECT " +
                    "current_database() AS FUNCTION_CATALOG, " +
                    "pg_namespace.nspname AS FUNCTION_SCHEMA, " +
                    "pg_proc.proname AS FUNCTION_NAME, " +
                    "pg_language.lanname AS PROCEDURE_LANGUAGE, " +
                    "pg_proc.proisagg AS IS_AGGREGATE, " +
                    "pg_proc.prosecdef AS IS_SECURITY_DEFINER, " + 
                    "pg_proc.proisstrict AS IS_STRICT, " + 
                    "case pg_proc.provolatile  " + 
                        "when 'i' THEN 'INMUTABLE' " + 
                        "when 's' THEN 'STABLE' " + 
                        "when 'v' THEN 'VOLATILE' " + 
                    "END  AS VOLATILE, " +
                    "pg_proc.proretset AS RETURNS_SET, " + 
                    "pg_proc.pronargs AS ARGUMENT_NUMBER, " + 
                    "pg_proc.prosrc AS SOURCE, " +
                    "pg_description.description AS DESCRIPTION " + 
                "FROM " +
                    "pg_proc " +
                    "left join pg_namespace ON pg_proc.pronamespace = pg_namespace.oid " + 
                    "left join pg_language ON pg_proc.prolang = pg_language.oid " + 
                    "left join pg_description ON pg_proc.oid = pg_description.objoid ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // FUNCTION_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // FUNCTION_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    if (where.Length > 0)
                    {
                        where += " and ";
                    }
                    where += String.Format("pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // FUNCTION_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    if (where.Length > 0)
                    {
                        where += " and ";
                    }
                    where += String.Format(" pg_proc.proname = '{0}'", restrictions[2]);
                }
            }

            if (where.Length > 0)
            {
                sql += " WHERE " + where;
            }

            sql += " ORDER BY pg_namespace.nspname, pg_proc.proname";

            return sql;
        }

        #endregion
    }
}
