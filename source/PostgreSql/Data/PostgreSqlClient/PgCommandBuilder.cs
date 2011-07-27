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
using System.Data.Common;
using System.Globalization;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgCommandBuilder
        : DbCommandBuilder
    {
        #region · Static Methods ·

        /// <include file='Doc/en_EN/FbCommandBuilder.xml' path='doc/class[@name="FbCommandBuilder"]/method[@name="DeriveParameters(PgCommand)"]/*'/>
        public static void DeriveParameters(PgCommand command)
        {
            if (command.CommandType != CommandType.StoredProcedure)
            {
                throw new InvalidOperationException("The command text is not a valid stored procedure name.");
            }

            string originalSpName   = command.CommandText.Trim();
            string schemaName       = "";
            string spName           = "";
            string quotePrefix      = "\"";
            string quoteSuffix      = "\"";

            if (originalSpName.Contains("."))
            {
                string[] parts = originalSpName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 2)
                {
                    throw new InvalidOperationException("The command stored procedure name is not valid.");
                }

                schemaName  = parts[0];
                spName      = parts[1];
            }
            else
            {
                spName = originalSpName;
            }

            if (schemaName.StartsWith(quotePrefix) && schemaName.EndsWith(quoteSuffix))
            {
                schemaName = schemaName.Substring(1, spName.Length - 2);
            }
            else
            {
                schemaName = schemaName.ToUpper(CultureInfo.CurrentUICulture);
            }

            if (spName.StartsWith(quotePrefix) && spName.EndsWith(quoteSuffix))
            {
                spName = spName.Substring(1, spName.Length - 2);
            }
            else
            {
                spName = spName.ToUpper(CultureInfo.CurrentUICulture);
            }

            string paramsText = String.Empty;

            command.Parameters.Clear();

            DataView dataTypes = command.Connection.GetSchema("DataTypes").DefaultView;

            DataTable spSchema = command.Connection.GetSchema(
                "FunctionParameters", new string[] { null, schemaName, spName });

            int count = 1;
            foreach (DataRow row in spSchema.Rows)
            {
                dataTypes.RowFilter = String.Format(
                    CultureInfo.CurrentUICulture,
                    "TypeName = '{0}'",
                    row["PARAMETER_DATA_TYPE"]);

                PgParameter parameter = command.Parameters.Add(
                    "@" + row["PARAMETER_NAME"].ToString().Trim(),
                    PgDbType.VarChar);

                parameter.PgDbType  = (PgDbType)dataTypes[0]["ProviderDbType"];
                parameter.Direction = (ParameterDirection)row["PARAMETER_DIRECTION"];
                parameter.Size      = Convert.ToInt32(row["PARAMETER_SIZE"], CultureInfo.InvariantCulture);

                if (parameter.PgDbType == PgDbType.Decimal ||
                    parameter.PgDbType == PgDbType.Numeric)
                {
                    if (row["NUMERIC_PRECISION"] != DBNull.Value)
                    {
                        parameter.Precision = Convert.ToByte(row["NUMERIC_PRECISION"], CultureInfo.InvariantCulture);
                    }
                    if (row["NUMERIC_SCALE"] != DBNull.Value)
                    {
                        parameter.Scale = Convert.ToByte(row["NUMERIC_SCALE"], CultureInfo.InvariantCulture);
                    }
                }

                count++;
            }
        }

        #endregion

        #region · Fields ·

        private PgRowUpdatingEventHandler rowUpdatingHandler;

        #endregion

        #region · Properties ·

        public new PgDataAdapter DataAdapter
        {
            get { return (PgDataAdapter)base.DataAdapter; }
            set { base.DataAdapter = value; }
        }

        #endregion

        #region · Constructors ·

        public PgCommandBuilder() 
            : this(null)
        {
        }

        public PgCommandBuilder(PgDataAdapter adapter) 
            : base()
        {
            this.DataAdapter    = adapter;
            this.ConflictOption = ConflictOption.OverwriteChanges;
        }

        #endregion

        #region · DbCommandBuilder methods ·

        public new PgCommand GetInsertCommand()
        {
            return base.GetInsertCommand(true) as PgCommand;
        }

        public new PgCommand GetUpdateCommand()
        {
            return base.GetUpdateCommand(true) as PgCommand;
        }

        public new PgCommand GetUpdateCommand(bool useColumnsForParameterNames)
        {
            return base.GetUpdateCommand(useColumnsForParameterNames) as PgCommand;
        }

        public new PgCommand GetDeleteCommand()
        {
            return base.GetDeleteCommand(true) as PgCommand;
        }

        public new PgCommand GetDeleteCommand(bool useColumnsForParameterNames)
        {
            return base.GetDeleteCommand(useColumnsForParameterNames) as PgCommand;
        }

        public override string QuoteIdentifier(string unquotedIdentifier)
        {
            if (unquotedIdentifier == null)
            {
                throw new ArgumentNullException("Unquoted identifier parameter cannot be null");
            }

            return String.Format(base.QuotePrefix, unquotedIdentifier, base.QuoteSuffix);
        }

        public override string UnquoteIdentifier(string quotedIdentifier)
        {
            if (quotedIdentifier == null)
            {
                throw new ArgumentNullException("Quoted identifier parameter cannot be null");
            }

            string unquotedIdentifier = quotedIdentifier.Trim();

            if (unquotedIdentifier.StartsWith(base.QuotePrefix))
            {
                unquotedIdentifier = unquotedIdentifier.Remove(0, 1);
            }
            if (unquotedIdentifier.EndsWith(base.QuoteSuffix))
            {
                unquotedIdentifier = unquotedIdentifier.Remove(unquotedIdentifier.Length - 1, 1);
            }

            return unquotedIdentifier;
        }

        #endregion

        #region · Protected DbCommandBuilder methods ·

        protected override void ApplyParameterInfo(DbParameter p, DataRow row, StatementType statementType, bool whereClause)
        {
        }

        protected override string GetParameterName(int parameterOrdinal)
        {
            return String.Format("@p{0}", parameterOrdinal);
        }

        protected override string GetParameterName(string parameterName)
        {
            return String.Format("@{0}", parameterName);
        }

        protected override string GetParameterPlaceholder(int parameterOrdinal)
        {
            return this.GetParameterName(parameterOrdinal);
        }

        protected override void SetRowUpdatingHandler(DbDataAdapter adapter)
        {
            if (!(adapter is PgDataAdapter))
            {
                throw new InvalidOperationException("adapter needs to be a PgDataAdapter");
            }

            this.rowUpdatingHandler = new PgRowUpdatingEventHandler(this.RowUpdatingHandler);
            ((PgDataAdapter)adapter).RowUpdating += this.rowUpdatingHandler;
        }

        #endregion

        #region · Event Handlers ·

        private void RowUpdatingHandler(object sender, PgRowUpdatingEventArgs e)
        {
            base.RowUpdatingHandler(e);
        }

        #endregion
    }
}
