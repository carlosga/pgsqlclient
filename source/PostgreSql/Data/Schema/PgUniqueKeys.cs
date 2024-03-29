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
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgUniqueKeys 
        : PgSchema
    {
        #region � Constructors �

        public PgUniqueKeys(PgConnection connection)
            : base(connection)
        {            
        }

        #endregion

        #region � Protected Methods �

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS TABLE_CATALOG, " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "null AS COLUMN_NAME, " +
                    "pg_constraint.conname AS UK_NAME, " +
                    "pg_constraint.conkey AS UK_COLUMNS, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_constraint " +
                    "left join pg_class ON pg_constraint.conrelid = pg_class.oid " +
                    "left join pg_namespace ON pg_constraint.connamespace = pg_namespace.oid " +
                    "left join pg_description ON pg_constraint.oid = pg_description.objoid " +
                "WHERE " +
                    "pg_constraint.contype = 'u' ";

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
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_class.relname, pg_constraint.conname";
        }

        protected override System.Data.DataTable ProcessResult(PostgreSql.Data.PostgreSqlClient.PgConnection connection, System.Data.DataTable schema)
        {
            DataTable uniqueKeyColumns = schema.Clone();
            string sql =
                "SELECT " +
                    "column_name " +
                "FROM information_schema.columns " +
                "WHERE " +
                    "table_catalog=current_database() AND " +
                    "table_schema=@tableSchema AND " +
                    "table_name=@tableName AND " +
                    "ordinal_position=@ordinalPosition";

            PgCommand selectColumn = new PgCommand(sql, connection);
            selectColumn.Parameters.Add("@tableSchema", PgDbType.Text);
            selectColumn.Parameters.Add("@tableName", PgDbType.Text);
            selectColumn.Parameters.Add("@ordinalPosition", PgDbType.Text);

            try
            {
                uniqueKeyColumns.BeginLoadData();

                selectColumn.Prepare();

                foreach (DataRow row in schema.Rows)
                {
                    Array pkColumns = (Array)row["UK_COLUMNS"];

                    for (int i = 0; i < pkColumns.Length; i++)
                    {
                        DataRow primaryKeyColumn = uniqueKeyColumns.NewRow();

                        // Grab the table column name
                        selectColumn.Parameters["@tableSchema"].Value     = row["TABLE_SCHEMA"];
                        selectColumn.Parameters["@tableName"].Value       = row["TABLE_NAME"];
                        selectColumn.Parameters["@ordinalPosition"].Value = Convert.ToInt16(pkColumns.GetValue(i + 1));

                        string pkColumnName = (string)selectColumn.ExecuteScalar();

                        // Create the new primary key column info
                        primaryKeyColumn["TABLE_CATALOG"] = row["TABLE_CATALOG"];
                        primaryKeyColumn["TABLE_SCHEMA"]  = row["TABLE_SCHEMA"];
                        primaryKeyColumn["TABLE_NAME"]    = row["TABLE_NAME"];
                        primaryKeyColumn["COLUMN_NAME"]   = pkColumnName;
                        primaryKeyColumn["DESCRIPTION"]   = row["DESCRIPTION"];

                        uniqueKeyColumns.Rows.Add(primaryKeyColumn);
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // CleanUp
                selectColumn.Dispose();

                uniqueKeyColumns.EndLoadData();
                uniqueKeyColumns.AcceptChanges();

                uniqueKeyColumns.Columns.Remove("UK_COLUMNS");
            }

            return uniqueKeyColumns;
        }

        #endregion
    }
}
