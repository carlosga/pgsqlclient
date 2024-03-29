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
    internal sealed class PgViews 
        : PgSchema
    {
        #region � Constructors �

        public PgViews(PgConnection connection)
            : base(connection)
        {            
        }

        #endregion

        #region � Protected Methods �

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS VIEW_CATALOG, " + 
                    "pg_namespace.nspname AS VIEW_SCHEMA, " + 
                    "pg_class.relname AS VIEW_NAME, " + 
                    "pg_get_ruledef(pg_rewrite.oid) AS DEFINITION, " + 
                    "pg_description.description AS DESCRIPTION " + 
                "FROM " +
                    "pg_class " + 
                        "left join pg_namespace ON pg_class.relnamespace = pg_namespace.oid " +
                        "left join pg_rewrite ON pg_class.oid = pg_rewrite.ev_class " + 
                        "left join pg_description ON pg_class.oid = pg_description.objoid " + 
                "WHERE " +
                    "pg_class.relkind = 'v' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // VIEW_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // VIEW_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // VIEW_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_class.relname";
        }

        #endregion
    }
}
