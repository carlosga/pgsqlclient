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
    internal sealed class PgFunctionParameters 
        : PgSchema
    {
        #region · Constructors ·

        public PgFunctionParameters(PgConnection connection)
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
                    "null AS PARAMETER_NAME, " +
                    "null AS DATA_TYPE, " +
                    "0 AS PARAMETER_DIRECTION, " +
                    "pg_proc.pronargs AS ARGUMENT_NUMBER, " +
                    "pg_proc.proallargtypes AS ARGUMENT_TYPES, " +
                    "pg_proc.proargmodes AS ARGUMENT_MODES, " +
                    "pg_proc.proargnames AS ARGUMENT_NAMES " +
                "FROM " +
                    "pg_proc " +
                    "left join pg_namespace ON pg_proc.pronamespace = pg_namespace.oid ";

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

        protected override DataTable ProcessResult(PgConnection connection, DataTable schema)
        {
            DataTable functionParameters = schema.Clone();

            functionParameters.BeginLoadData();

            foreach (DataRow row in schema.Rows)
            {
                int     argNumber   = Convert.ToInt32(row["ARGUMENT_NUMBER"]);
                Array   argTypes    = (Array)row["ARGUMENT_TYPES"];
                Array   argNames    = (Array)row["ARGUMENT_NAMES"];

                if (!Convert.ToBoolean(row["RETURNS_SET"]))
                {
                    DataRow functionParameter = functionParameters.NewRow();

                    // Create the new foreign key column info
                    functionParameter["FUNCTION_CATALOG"]       = row["FUNCTION_CATALOG"];
                    functionParameter["FUNCTION_SCHEMA"]        = row["FUNCTION_SCHEMA"];
                    functionParameter["FUNCTION_NAME"]          = row["FUNCTION_NAME"];
                    functionParameter["PARAMETER_NAME"]         = "result";
                    functionParameter["DATA_TYPE"]              = "";
                    functionParameter["PARAMETER_DIRECTION"]    = (Int32)ParameterDirection.Output;

                    functionParameters.Rows.Add(functionParameter);
                }

                for (int i = 0; i < argNumber; i++)
                {
                    DataRow functionParameter = functionParameters.NewRow();

                    // Create the new foreign key column info
                    functionParameter["FUNCTION_CATALOG"]   = row["FUNCTION_CATALOG"];
                    functionParameter["FUNCTION_SCHEMA"]    = row["FUNCTION_SCHEMA"];
                    functionParameter["FUNCTION_NAME"]      = row["FUNCTION_NAME"];
                    functionParameter["PARAMETER_NAME"]     = (string)argNames.GetValue(i + 1);
                    functionParameter["DATA_TYPE"]          = "";
                    functionParameter["PARAMETER_DIRECTION"]= (Int32)ParameterDirection.Input;

                    functionParameters.Rows.Add(functionParameter);
                }
            }

            functionParameters.EndLoadData();
            functionParameters.AcceptChanges();

            functionParameters.Columns.Remove("RETURNS_SET");
            functionParameters.Columns.Remove("ARGUMENT_NUMBER");
            functionParameters.Columns.Remove("ARGUMENT_TYPES");
            functionParameters.Columns.Remove("ARGUMENT_MODES");
            functionParameters.Columns.Remove("ARGUMENT_NAMES");

            return functionParameters;
        }

        #endregion
    }
}
