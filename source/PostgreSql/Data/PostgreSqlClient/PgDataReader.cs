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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{	
    public sealed class PgDataReader 
        : DbDataReader
    {		
        #region · Fields ·
        
        private const int		STARTPOS = -1;
        private bool			disposed;
        private bool			open;
        private int				position;
        private int				recordsAffected;
        private object[]		row;
        private DataTable		schemaTable;
        private CommandBehavior	behavior;
        private PgCommand		command;
        private PgConnection	connection;
        private PgStatement     statement;
        private Queue           refCursors;

        #endregion

        #region · Indexers ·

        public override object this[int i]
        {
            get { return this.GetValue(i); }
        }

        public override object this[string name]
        {			
            get { return this.GetValue(this.GetOrdinal(name)); }
        }

        #endregion

        #region · Constructors ·

        internal PgDataReader(PgConnection connection, PgCommand command)
        {
            this.open               = true;
            this.recordsAffected    = -1;
            this.position           = STARTPOS;
            this.refCursors         = new Queue();
            this.connection         = connection;
            this.command            = command;
            this.behavior	        = this.command.CommandBehavior;
            this.statement          = this.command.Statement;

            this.Initialize();
        }

        #endregion

        #region · Finalizer ·

        /// <include file='Doc/en_EN/FbDataReader.xml' path='doc/class[@name="FbDataReader"]/destructor[@name="Finalize"]/*'/>
        ~PgDataReader()
        {
            this.Dispose(false);
        }

        #endregion

        #region · IDisposable methods ·

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // release any managed resources
                        this.Close();
                        
                        this.command            = null;
                        this.statement          = null;
                        this.connection         = null;
                        this.refCursors         = null;
                        this.row                = null;
                        this.schemaTable        = null;
                        this.recordsAffected    = -1;
                        this.position           = -1;
                    }

                    // release any unmanaged resources
                }
                finally
                {
                }

                this.disposed = true;
            }
        }

        #endregion

        #region · IDataReader Properties & Methods ·

        public override int Depth 
        {
            get { return 0; }
        }

        public override bool IsClosed
        {
            get { return !this.open; }
        }

        public override int RecordsAffected 
        {
            get { return this.IsClosed ? this.recordsAffected : -1; }
        }

        public override bool HasRows
        {
            get { return this.command.Statement.HasRows; }
        }

        public override void Close()
        {
            if (!this.open)
            {
                return;
            }

            // This will update RecordsAffected property
            this.UpdateRecordsAffected();

            if (this.command != null && !this.command.IsDisposed)
            {
                if (this.command.Statement != null)
                {
                    // Set values of output parameters
                    this.command.InternalSetOutputParameters();		
                }

                this.command.ActiveDataReader = null;
            }			

            if (this.connection != null)
            {
                if ((this.behavior & CommandBehavior.CloseConnection) == CommandBehavior.CloseConnection)
                {
                    this.connection.Close();
                }
            }

            this.refCursors.Clear();

            this.open		= false;			
            this.position	= STARTPOS;
        }

        public override bool NextResult()
        {
            bool hasMoreResults = false;

            if (this.refCursors.Count != 0 && this.connection.InternalConnection.HasActiveTransaction)
            {
                string sql = String.Format("fetch all in \"{0}\"", (string)this.refCursors.Dequeue());

                // Reset position
                this.position   = STARTPOS;

                // Close the active statement
                this.statement.Close();

                // Create a new statement to fetch the current refcursor
                string statementName    = Guid.NewGuid().ToString();
                string prepareName      = String.Format("PS{0}", statementName);
                string portalName       = String.Format("PR{0}", statementName);

                this.statement = this.connection.InternalConnection.Database.CreateStatement(prepareName, portalName, sql);
                // this.statement.Query();
                this.statement.Parse();
                this.statement.Describe();
                this.statement.Bind();
                this.statement.Execute();

                // Allow the DataReader to process more refcursors
                hasMoreResults = true;
            }

            return hasMoreResults;
        }

        public override bool Read()
        {
            bool read = false;

            if ((this.behavior == CommandBehavior.SingleRow && this.position != STARTPOS) || 
                !this.command.Statement.HasRows)
            {
            }
            else
            {
                try
                {
                    this.position++;

                    row = this.statement.FetchRow();

                    read = (this.row == null) ? false : true;
                }
                catch (PgClientException ex)
                {
                    throw new PgException(ex.Message, ex);
                }
            }
                        
            return read;
        }

        #endregion

        #region · GetSchemaTable Method ·

        public override DataTable GetSchemaTable()
        {
            if (this.schemaTable == null)
            {
                int     tableCount      = 0;
                string  currentTable    = "";

                this.schemaTable = this.GetSchemaTableStructure();

                this.schemaTable.BeginLoadData();

                PgCommand columnsCmd = new PgCommand(this.GetColumnsSql(), this.connection);
                columnsCmd.Parameters.Add("@OidNumber", PgDbType.Int4);
                columnsCmd.Parameters.Add("@OidTable", PgDbType.Int4);
                                
                PgCommand primaryKeyCmd	= new PgCommand(this.GetPrimaryKeysSql(), this.connection);
                primaryKeyCmd.Parameters.Add("@OidTable", PgDbType.Int4);
                
                for (int i = 0; i < this.statement.RowDescriptor.Fields.Length; i++)
                {
                    object[]	columnInfo	= null;
                    Array		pKeyInfo	= null;

                    // Execute commands
                    columnsCmd.Parameters[0].Value = this.statement.RowDescriptor.Fields[i].OidNumber;
                    columnsCmd.Parameters[1].Value = this.statement.RowDescriptor.Fields[i].OidTable;

                    primaryKeyCmd.Parameters[0].Value = this.statement.RowDescriptor.Fields[i].OidTable;

                    columnsCmd.InternalPrepare(); // First time it will prepare the command, next times it will close the open portal
                    columnsCmd.InternalExecute();

                    primaryKeyCmd.InternalPrepare(); // First time it will prepare the command, next times it will close the open portal
                    primaryKeyCmd.InternalExecute();

                    // Get Column Information
                    if (columnsCmd.Statement.Rows != null && columnsCmd.Statement.Rows.Length > 0)
                    {
                        columnInfo = (object[])columnsCmd.Statement.Rows[0];
                    }

                    // Get Primary Key Info
                    if (primaryKeyCmd.Statement.Rows != null && primaryKeyCmd.Statement.Rows.Length > 0)
                    {
                        object[] temp = (object[])primaryKeyCmd.Statement.Rows[0];
                        pKeyInfo = (Array)temp[0];
                    }

                    // Add row information
                    DataRow schemaRow = this.schemaTable.NewRow();

                    schemaRow["ColumnName"]			= this.GetName(i);
                    schemaRow["ColumnOrdinal"]		= (i + 1);
                    schemaRow["ColumnSize"]         = this.GetSize(i);
                    if (this.IsNumeric(i))
                    {
                        schemaRow["NumericPrecision"]	= this.GetNumericPrecision(i);
                        schemaRow["NumericScale"]		= this.GetNumericScale(i);
                    }
                    else
                    {
                        schemaRow["NumericPrecision"]	= DBNull.Value;
                        schemaRow["NumericScale"]		= DBNull.Value;
                    }
                    schemaRow["DataType"]			= this.GetFieldType(i);
                    schemaRow["ProviderType"]       = this.GetProviderDbType(i);
                    schemaRow["IsLong"]				= this.IsLong(i);
                    schemaRow["IsRowVersion"]		= this.CultureAwareCompare(this.GetName(i), "oid");
                    schemaRow["IsUnique"]			= false;
                    schemaRow["IsAliased"]			= this.IsAliased(i);
                    schemaRow["IsExpression"]       = this.IsExpression(i);
                    schemaRow["BaseCatalogName"]	= System.DBNull.Value;

                    if (columnInfo != null)
                    {
                        schemaRow["BaseSchemaName"]		= columnInfo[0].ToString();
                        schemaRow["BaseTableName"]		= columnInfo[1].ToString();
                        schemaRow["BaseColumnName"]		= columnInfo[2].ToString();
                        schemaRow["IsReadOnly"]			= (bool)columnInfo[7];
                        schemaRow["IsAutoIncrement"]	= (bool)columnInfo[7];
                        schemaRow["IsKey"]              = this.IsPrimaryKey(pKeyInfo, Convert.ToInt32(columnInfo[5]));
                        schemaRow["AllowDBNull"]		= ((bool)columnInfo[6]) ? false : true;
                    }
                    else
                    {
                        schemaRow["IsReadOnly"]			= false;
                        schemaRow["IsAutoIncrement"]	= false;
                        schemaRow["IsKey"]				= false;
                        schemaRow["AllowDBNull"]		= System.DBNull.Value;						
                        schemaRow["BaseSchemaName"]		= System.DBNull.Value;
                        schemaRow["BaseTableName"]		= System.DBNull.Value;
                        schemaRow["BaseColumnName"]		= System.DBNull.Value;
                    }

                    if (!String.IsNullOrEmpty(schemaRow["BaseSchemaName"].ToString()) &&
                        schemaRow["BaseSchemaName"].ToString() != currentTable)
                    {
                        tableCount++;
                        currentTable = schemaRow["BaseSchemaName"].ToString();
                    }

                    this.schemaTable.Rows.Add(schemaRow);
                }

                if (tableCount > 1)
                {
                    foreach (DataRow row in this.schemaTable.Rows)
                    {
                        row["IsKey"]    = false;
                        row["IsUnique"] = false;
                    }
                }

                this.schemaTable.EndLoadData();

                columnsCmd.Dispose();
                primaryKeyCmd.Dispose();
            }

            return this.schemaTable;
        }

        private string GetColumnsSql()
        {
            return	
                "SELECT " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "pg_attribute.attname AS COLUMN_NAME, " +
                    "pg_attribute.atttypid AS DATA_TYPE, " +
                    "pg_attribute.attlen AS COLUMN_SIZE, " +
                    "pg_attribute.attnum AS ORDINAL_POSITION, " +
                    "pg_attribute.attnotnull AS IS_NOT_NULL, " +
                    "(pg_depend.objid is not null) AS IS_AUTOINCREMENT " +
                "FROM pg_attribute " +
                    "left join pg_class ON pg_attribute.attrelid = pg_class.oid " + 
                    "left join pg_namespace ON pg_class.relnamespace = pg_namespace.oid " + 
                    "left join pg_attrdef ON (pg_class.oid = pg_attrdef.adrelid AND pg_attribute.attnum = pg_attrdef.adnum) " +
                    "left join pg_depend ON (pg_attribute.attrelid = pg_depend.refobjid AND pg_attribute.attnum = pg_depend.refobjsubid  AND pg_depend.deptype = 'i') " +
                "WHERE " +
                    "pg_attribute.attisdropped = false AND " +
                    "pg_attribute.attnum > 0 AND " +
                    "pg_attribute.attnum = @OidNumber AND " +
                    "pg_attribute.attrelid = @OidTable";
        }

        private string GetPrimaryKeysSql()
        {
            return	"SELECT " + 
                        "pg_constraint.conkey AS PK_COLUMNS " + 
                    "FROM pg_constraint " + 
                        "left join pg_class ON pg_constraint.conrelid = pg_class.oid " + 
                        "left join pg_namespace ON pg_constraint.connamespace = pg_namespace.oid " + 
                        "left join pg_description ON pg_constraint.oid = pg_description.objoid " + 
                    "WHERE " + 
                        "pg_constraint.contype = 'p' and " +
                        "pg_class.oid = @OidTable";
        }

        private bool IsPrimaryKey(System.Array pKeyInfo, int ordinal)
        {
            if (pKeyInfo != null)
            {
                for (int i = pKeyInfo.GetLowerBound(0); i <= pKeyInfo.GetUpperBound(0); i++)
                {
                    if ((short)pKeyInfo.GetValue(i) == ordinal)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region · IDataRecord Properties & Methods ·
        
        public override int FieldCount
        {			
            get 
            {
                if (this.statement != null)
                {
                    return this.statement.RowDescriptor.Fields.Length;
                }

                return -1;
            }
        }

        public override String GetName(int i)
        {
            this.CheckIndex(i);

            return this.statement.RowDescriptor.Fields[i].FieldName;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetDataTypeName(int i)
        {
            this.CheckIndex(i);

            return this.statement.RowDescriptor.Fields[i].DataType.Name;
        }

        public override Type GetFieldType(int i)
        {			
            this.CheckIndex(i);

            return this.statement.RowDescriptor.Fields[i].DataType.SystemType;
        }

        public override object GetValue(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return this.row[i];
        }

        public override int GetValues(object[] values)
        {
            this.CheckPosition();
            
            for (int i = 0; i < this.FieldCount; i++)
            {
                values[i] = this.GetValue(i);
            }

            return values.Length;
        }

        public override int GetOrdinal(string name)
        {
            if (this.IsClosed)
            {
                throw new InvalidOperationException("Reader closed");
            }

            for (int i = 0; i < this.FieldCount; i++)
            {
                if (this.CultureAwareCompare(this.statement.RowDescriptor.Fields[i].FieldName, name))
                {
                    return i;
                }
            }

            return -1;
        }

        public override bool GetBoolean(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToBoolean(this.GetValue(i));
        }

        public override byte GetByte(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToByte(this.GetValue(i));
        }

        public override long GetBytes(int i, long dataIndex, byte[]	buffer, int	bufferIndex, int length)
        {
            int bytesRead	= 0;
            int realLength	= length;

            if (buffer == null)
            {
                if (this.IsDBNull(i))
                {
                    return 0;
                }
                else
                {
                    byte[] data = (byte[])this.GetValue(i);

                    return data.Length;
                }
            }
            
            byte[] byteArray = (byte[])this.GetValue(i);

            if (length > (byteArray.Length - dataIndex))
            {
                realLength = byteArray.Length - (int)dataIndex;
            }
                                
            Array.Copy(byteArray, (int)dataIndex, buffer, bufferIndex, realLength);

            if ((byteArray.Length - dataIndex) < length)
            {
                bytesRead = byteArray.Length - (int)dataIndex;
            }
            else
            {
                bytesRead = length;
            }

            return bytesRead;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override char GetChar(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);
            
            return Convert.ToChar(this.GetValue(i));
        }

        public override long GetChars(int	i, long	dataIndex, char[] buffer, int bufferIndex, int length)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            if (buffer == null)
            {
                if (this.IsDBNull(i))
                {
                    return 0;
                }
                else
                {
                    char[] data = ((string)this.GetValue(i)).ToCharArray();

                    return data.Length;
                }
            }
            
            int charsRead	= 0;
            int realLength	= length;
            
            char[] charArray = ((string)this.GetValue(i)).ToCharArray();

            if (length > (charArray.Length - dataIndex))
            {
                realLength = charArray.Length - (int)dataIndex;
            }
                                
            System.Array.Copy(charArray, (int)dataIndex, buffer, 
                bufferIndex, realLength);

            if ( (charArray.Length - dataIndex) < length)
            {
                charsRead = charArray.Length - (int)dataIndex;
            }
            else
            {
                charsRead = length;
            }

            return charsRead;
        }
        
        public override Guid GetGuid(int i)
        {
            throw new NotSupportedException("Guid datatype is not supported");
        }

        public override Int16 GetInt16(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToInt16(this.GetValue(i));
        }

        public override Int32 GetInt32(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToInt32(this.GetValue(i));
        }
        
        public override Int64 GetInt64(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToInt64(this.GetValue(i));
        }

        public override float GetFloat(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToSingle(this.GetValue(i));
        }

        public override double GetDouble(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);
            
            return Convert.ToDouble(this.GetValue(i));
        }

        public override string GetString(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToString(this.GetValue(i));
        }

        public override Decimal GetDecimal(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToDecimal(this.GetValue(i));
        }

        public override DateTime GetDateTime(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return Convert.ToDateTime(this.GetValue(i));
        }

        public TimeSpan GetTimeSpan(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (TimeSpan)GetValue(i);
        }

        public PgTimeSpan GetPgTimeSpan(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return new PgTimeSpan(this.GetTimeSpan(i));
        }

        #region · Geometric Types ·

        public PgPoint GetPgPoint(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgPoint)this.GetProviderSpecificValue(i);
        }

        public PgBox GetPgBox(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgBox)this.GetProviderSpecificValue(i);
        }

        public PgLSeg GetPgLSeg(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgLSeg)this.GetProviderSpecificValue(i);
        }

        public PgCircle GetPgCircle(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgCircle)this.GetProviderSpecificValue(i);
        }

        public PgPath GetPgPath(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgPath)this.GetProviderSpecificValue(i);
        }

        public PgPolygon GetPgPolygon(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgPolygon)this.GetProviderSpecificValue(i);
        }

        #endregion

        #region · PostGIS Types ·

        public PgBox2D GetPgBox2D(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            return (PgBox2D)this.GetProviderSpecificValue(i);
        }

        #endregion

        public override Type GetProviderSpecificFieldType(int i)
        {
            return this.GetFieldType(i);
        }

        public override object GetProviderSpecificValue(int i)
        {
            this.CheckPosition();
            this.CheckIndex(i);

            switch (this.GetProviderDbType(i))
            {
                case PgDbType.Interval:
                    return this.GetPgTimeSpan(i);

                default:
                    return this.GetValue(i);
            }
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return this.GetValues(values);
        }

        public override bool IsDBNull(int i)
        {	
            this.CheckPosition();
            this.CheckIndex(i);

            bool returnValue = false;

            if (this.row[i] == System.DBNull.Value)
            {
                returnValue = true;
            }

            return returnValue;
        }

        #endregion

        #region · IEnumerable Methods ·

        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this);
        }

        #endregion

        #region · Private Methods ·

        private void Initialize()
        {
            if (this.connection.InternalConnection.HasActiveTransaction)
            {
                // Ref cursors can be fetched only if there are an active transaction
                if (this.command.CommandType == CommandType.StoredProcedure &&
                    this.command.Statement.RowDescriptor.Fields.Length == 1 &&
                    this.command.Statement.RowDescriptor.Fields[0].DataType.IsRefCursor)
                {
                    // Clear refcursor's queue
                    this.refCursors.Clear();

                    // Add refcrusr's names to the queue
                    object[] row = new object[0];

                    while (row != null)
                    {
                        row = this.statement.FetchRow();

                        if (row != null)
                        {
                            this.refCursors.Enqueue(row[0]);
                        }
                    }

                    // Grab information of the first refcursor
                    this.NextResult();
                }
            }
        }

        private void CheckIndex(int i)
        {
            if (i < 0 || i >= this.FieldCount)
            {
                throw new IndexOutOfRangeException("Could not find specified column in results.");
            }
        }

        private void CheckPosition()
        {
            if (this.position == STARTPOS)
            {
                throw new InvalidOperationException("There are no data to read.");
            }
        }

        private int GetSize(int i)
        {
            return this.command.Statement.RowDescriptor.Fields[i].DataType.Size;			
        }

        private PgDbType GetProviderDbType(int i)
        {
            return (PgDbType)this.command.Statement.RowDescriptor.Fields[i].DataType.DataType;
        }

        private int GetNumericPrecision(int i)
        {
            #warning "Add implementation"
            return 0;
        }

        private int GetNumericScale(int i)
        {
            #warning "Add implementation"
            return 0;
        }

        private bool IsNumeric(int i)
        {				
            if (i < 0 || i >= this.FieldCount)
            {
                throw new IndexOutOfRangeException("Could not find specified column in results.");
            }
            
            return this.command.Statement.RowDescriptor.Fields[i].DataType.IsNumeric;
        }

        private bool IsLong(int i)
        {			
            if (i < 0 || i >= this.FieldCount)
            {
                throw new IndexOutOfRangeException("Could not find specified column in results.");
            }

            return this.command.Statement.RowDescriptor.Fields[i].DataType.IsLong;
        }

        private bool IsAliased(int i)
        {
            /* TODO: Add implementation	*/
            return false;
        }

        private bool IsExpression(int i)
        {	
            bool returnValue = false;

            if (this.command.Statement.RowDescriptor.Fields[i].OidNumber == 0 &&
                this.command.Statement.RowDescriptor.Fields[i].OidTable	== 0)
            {
                returnValue = true;
            }

            return returnValue;
        }

        private DataTable GetSchemaTableStructure()
        {
            DataTable schema = new DataTable("Schema");			

            schema.Columns.Add("ColumnName"		, System.Type.GetType("System.String"));
            schema.Columns.Add("ColumnOrdinal"	, System.Type.GetType("System.Int32"));
            schema.Columns.Add("ColumnSize"		, System.Type.GetType("System.Int32"));
            schema.Columns.Add("NumericPrecision", System.Type.GetType("System.Int32"));
            schema.Columns.Add("NumericScale"	, System.Type.GetType("System.Int32"));
            schema.Columns.Add("DataType"		, System.Type.GetType("System.Type"));
            schema.Columns.Add("ProviderType"	, System.Type.GetType("System.Int32"));
            schema.Columns.Add("IsLong"			, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("AllowDBNull"	, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsReadOnly"		, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsRowVersion"	, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsUnique"		, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsKey"			, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsAutoIncrement", System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsAliased"		, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("IsExpression"	, System.Type.GetType("System.Boolean"));
            schema.Columns.Add("BaseSchemaName"	, System.Type.GetType("System.String"));
            schema.Columns.Add("BaseCatalogName", System.Type.GetType("System.String"));
            schema.Columns.Add("BaseTableName"	, System.Type.GetType("System.String"));
            schema.Columns.Add("BaseColumnName"	, System.Type.GetType("System.String"));
            
            return schema;
        }

        private void UpdateRecordsAffected()
        {
            if (this.command != null && !this.command.IsDisposed)
            {
                if (this.command.RecordsAffected != -1)
                {
                    this.recordsAffected = this.recordsAffected == -1 ? 0 : this.recordsAffected;
                    this.recordsAffected += this.command.RecordsAffected;
                }
            }
        }

        private bool IsCommandBehavior(CommandBehavior behavior)
        {
            return ((behavior & this.behavior) == behavior);
        }

        private bool CultureAwareCompare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare
            (
                strA, 
                strB, 
                CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | 
                CompareOptions.IgnoreCase
            ) == 0 ? true : false;
        }

        #endregion
    }
}