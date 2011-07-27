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
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgStatement
    {
        #region · Fields ·

        private PgDatabase      db;
        private string			stmtText;
        private bool			hasRows;
        private string			tag;
        private string			parseName;
        private string			portalName;
        private int				fetchSize;
        private bool			allRowsFetched;
        private PgRowDescriptor rowDescriptor;
        private object[]		rows;
        private int				rowIndex;
        private PgParameter[]	parameters;
        private PgParameter		outParameter;
        private int				recordsAffected;
        private char			transactionStatus;

        private PgStatementStatus status;

        #endregion

        #region · Properties ·

        public PgDatabase DbHandle
        {
            get { return this.db; }
            set { this.db = value; }
        }

        public string StmtText
        {
            get { return this.stmtText; }
            set { this.stmtText = value; }
        }

        public bool HasRows
        {
            get { return this.hasRows; }
        }

        public string Tag		
        {
            get { return this.tag; }
        }

        public string ParseName
        {
            get { return this.parseName; }
            set { this.parseName = value; }
        }

        public string PortalName
        {
            get { return this.portalName; }
            set { this.portalName = value; }
        }

        public PgRowDescriptor RowDescriptor
        {
            get { return this.rowDescriptor; }
        }

        public object[] Rows
        {
            get { return this.rows; }
        }

        public PgParameter[] Parameters
        {
            get { return this.parameters; }
        }

        public PgParameter OutParameter
        {
            get { return this.outParameter; }
            set { this.outParameter = value; }
        }

        public int RecordsAffected
        {
            get { return this.recordsAffected; }
        }

        public PgStatementStatus Status
        {
            get { return this.status; }
        }

        public char TransactionStatus
        {
            get { return this.transactionStatus; }
        }

        #endregion

        #region · Constructors ·

        public PgStatement() : this(null)
        {
        }

        public PgStatement(PgDatabase db)
            : this(db, null, null)
        {
        }

        public PgStatement(PgDatabase db, string parseName, string portalName)
            : this(db, parseName, portalName, null)
        {
        }

        public PgStatement(PgDatabase db, string stmtText)
            : this(db, null, null, stmtText)
        {
        }

        public PgStatement(string parseName, string portalName) : this(null, parseName, portalName, null)
        {
        }

        public PgStatement(PgDatabase db, string parseName, string portalName, string stmtText)
        {
            this.db					= db;
            this.outParameter		= new PgParameter();
            this.rows				= null;
            this.rowIndex			= 0;
            this.hasRows            = false;
            this.allRowsFetched     = false;
            this.parseName			= parseName;
            this.portalName			= portalName;
            this.recordsAffected	= -1;
            this.status				= PgStatementStatus.Initial;
            this.fetchSize			= 200;
            this.stmtText			= stmtText;

            GC.SuppressFinalize(this);
        }

        #endregion

        #region · Methods ·

        public void Parse()
        {
            lock (this.db)
            {
                try
                {
                    // Update status
                    this.status = PgStatementStatus.Parsing;
                    
                    // Clear actual row list
                    this.rows		    = null;
                    this.rowIndex	    = 0;
                    this.hasRows        = false;
                    this.allRowsFetched = false;

                    // Initialize RowDescriptor and Parameters
                    this.rowDescriptor	= new PgRowDescriptor(0);
                    this.parameters		= new PgParameter[0];

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    packet.WriteNullString(this.ParseName);
                    packet.WriteNullString(this.stmtText);
                    packet.Write((short)0);

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.PARSE);
                    
                    // Update status
                    this.status	= PgStatementStatus.Parsed;
                }
                catch (PgClientException)
                {
                    // Update status
                    this.status	= PgStatementStatus.Error;
                    // Throw exception
                    throw;
                }
            }
        }

        public void Describe()
        {
            this.Describe('S');
        }

        public void DescribePortal()
        {
            this.Describe('P');
        }
        
        private void Describe(char stmtType)
        {
            lock (this.db)
            {
                try
                {
                    // Update status
                    this.status = PgStatementStatus.Describing;

                    string name = ((stmtType == 'S') ? this.ParseName : this.PortalName);

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    packet.Write((byte)stmtType);
                    packet.WriteNullString(name);

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.DESCRIBE);

                    // Flush pending messages
                    this.db.Flush();

                    // Receive Describe response
                    PgResponsePacket response = null;
                    do
                    {
                        response = this.db.ReceiveResponsePacket();
                        this.ProcessSqlPacket(response);
                    } 
                    while (!response.IsRowDescription && !response.IsNoData);

                    // Review if there are some parameter with a domain as a Data Type
                    foreach (PgParameter parameter in this.parameters)
                    {
                        if (parameter.DataType == null)
                        {
                            // It's a non supported data type or a domain data type
                            PgStatement stmt = new PgStatement(this.db, String.Format("select typbasetype from pg_type where oid = {0} and typtype = 'd'", parameter.DataTypeOid));

                            try
                            {
                                stmt.Query();

                                if (!stmt.HasRows)
                                {
                                    throw new PgClientException("Unsupported data type");
                                }

                                int baseTypeOid = Convert.ToInt32(stmt.FetchRow()[0]);

                                if (baseTypeOid == 0 || !this.db.DataTypes.Contains(baseTypeOid))
                                {
                                    throw new PgClientException("Unsupported data type");
                                }

                                // Try to add the data type to the list of supported data types
                                parameter.DataType = this.db.DataTypes[baseTypeOid];
                            }
                            catch
                            {
                                throw new PgClientException("Unsupported data type");
                            }
                        }
                    }

                    // Update status
                    this.status	= PgStatementStatus.Described;
                }
                catch
                {
                    // Update status
                    this.status	= PgStatementStatus.Error;
                    // Throw exception
                    throw;
                }
            }
        }

        public void Bind()
        {
            lock (this.db)
            {
                try
                {
                    // Update status
                    this.status = PgStatementStatus.Binding;

                    // Clear actual row list
                    this.rows           = null;
                    this.rowIndex       = 0;
                    this.hasRows        = false;
                    this.allRowsFetched = false;

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    // Destination portal name
                    packet.WriteNullString(this.PortalName);
                
                    // Prepared statement name
                    packet.WriteNullString(this.ParseName);
                    
                    // Send parameters format code.
                    packet.Write((short)parameters.Length);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        packet.Write((short)this.parameters[i].DataType.FormatCode);
                    }

                    // Send parameter values
                    packet.Write((short)parameters.Length);
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        packet.Write(this.parameters[i]);
                    }
                    
                    // Send column information
                    packet.Write((short)this.rowDescriptor.Fields.Length);
                    for (int i = 0; i < this.rowDescriptor.Fields.Length; i++)
                    {
                        packet.Write((short)this.rowDescriptor.Fields[i].DataType.FormatCode);
                    }

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.BIND);

                    // Update status
                    this.status	= PgStatementStatus.Parsed;
                }
                catch
                {
                    // Update status
                    this.status	= PgStatementStatus.Error;
                    // Throw exception
                    throw;
                }
            }
        }

        public void Execute()
        {
            lock (this.db)
            {
                try
                {
                    // Update status
                    this.status = PgStatementStatus.Executing;

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    packet.WriteNullString(this.PortalName);
                    packet.Write(this.fetchSize);	// Rows to retrieve ( 0 = nolimit )

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.EXECUTE);

                    // Flush pending messages
                    this.db.Flush();
                    
                    // Receive response
                    PgResponsePacket response = null;

                    do
                    {
                        response = this.db.ReceiveResponsePacket();
                        this.ProcessSqlPacket(response);
                    } 
                    while (!response.IsReadyForQuery && !response.IsCommandComplete && !response.IsPortalSuspended);

                    // reset rowIndex
                    this.rowIndex = 0;

                    // If the command is finished and has returned rows
                    // set all rows are received
                    if ((response.IsReadyForQuery || response.IsCommandComplete) && this.HasRows)
                    {
                        this.allRowsFetched = true;
                    }

                    // If all rows are received or the command doesn't return
                    // rows perform a Sync.
                    if (!this.HasRows || this.allRowsFetched)
                    {
                        this.db.Sync();
                    }

                    // Update status
                    this.status	= PgStatementStatus.Executed;
                }
                catch
                {
                    this.status	= PgStatementStatus.Error;
                    throw;
                }
            }
        }

        public void ExecuteFunction(int id)
        {
            lock (this.db)
            {
                try
                {
                    // Update status
                    this.status = PgStatementStatus.Executing;

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    // Function id
                    packet.Write(id);

                    // Send parameters format code.
                    packet.Write((short)this.parameters.Length);
                    for (int i = 0; i < this.parameters.Length; i++)
                    {
                        packet.Write((short)this.parameters[i].DataType.FormatCode);
                    }

                    // Send parameter values
                    packet.Write((short)this.parameters.Length);
                    for (int i = 0; i < this.parameters.Length; i++)
                    {
                        packet.Write(this.parameters[i]);
                    }

                    // Send the format code for the function result
                    packet.Write(PgCodes.BINARY_FORMAT);

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.FUNCTION_CALL);
                    
                    // Receive response
                    PgResponsePacket response = null;
                    do
                    {
                        response = this.db.ReceiveResponsePacket();
                        this.ProcessSqlPacket(response);
                    }
                    while (!response.IsReadyForQuery);

                    // Update status
                    this.status	= PgStatementStatus.Executed;
                }
                catch
                {
                    // Update status
                    this.status	= PgStatementStatus.Error;
                    // Throw exception
                    throw;
                }
            }
        }

        public void Query()
        {
            ArrayList innerRows = new ArrayList();

            lock (this.db)
            {
                try
                {
                    // Update Status
                    this.status = PgStatementStatus.OnQuery;

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    packet.WriteNullString(this.stmtText);

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.QUERY);

                    // Set fetch size
                    this.fetchSize = 1;

                    // Receive response
                    PgResponsePacket response = null;

                    do
                    {
                        response = this.db.ReceiveResponsePacket();
                        this.ProcessSqlPacket(response);

                        if (this.hasRows && response.Message == PgBackendCodes.DATAROW)
                        {
                            innerRows.Add(this.rows[0]);
                            this.rowIndex = 0;
                        }
                    }
                    while (!response.IsReadyForQuery);

                    if (this.hasRows)
                    {
                        // Obtain all the rows
                        this.rows = (object[])innerRows.ToArray(typeof(object));

                        // reset rowIndex
                        this.rowIndex = 0;

                        // Set allRowsFetched flag
                        this.allRowsFetched = true;
                    }

                    // Update status
                    this.status = PgStatementStatus.Executed;
                }
                catch
                {
                    // Update status
                    this.status = PgStatementStatus.Error;
                    // Throw exception
                    throw;
                }
                finally
                {
                    // reset fetch size
                    this.fetchSize = 200;
                }
            }
        }

        public object[] FetchRow()
        {
            object[] row = null;

            if ((!this.allRowsFetched && this.rows == null) ||
                (!this.allRowsFetched && this.rows.Length == 0) ||
                (!this.allRowsFetched && this.rowIndex >= this.fetchSize))
            {
                lock (this)
                {
                    // Retrieve next group of rows
                    this.Execute();
                }
            }

            if (this.rows != null &&
                this.rows.Length > 0 &&
                this.rows[this.rowIndex] != null)
            {
                // Return always first row
                row = (object[])this.rows[this.rowIndex++];
            }

            if (this.rows != null && (this.rowIndex >= this.rows.Length || this.rows[this.rowIndex] == null))
            {
                this.rows = null;
            }

            return row;
        }

        public void Close()
        {
            this.Close('S');
        }

        public void ClosePortal()
        {
            this.Close('P');
        }

        private void Close(char stmtType)
        {
            lock (this.db)
            {
                try
                {
                    string name = ((stmtType == 'S') ? this.ParseName : this.PortalName);

                    PgOutputPacket packet = new PgOutputPacket(this.db.DataTypes, this.db.Encoding);

                    packet.Write((byte)stmtType);
                    packet.WriteNullString(String.IsNullOrEmpty(name) ? "" : name);

                    // Send packet to the server
                    this.db.SendPacket(packet, PgFrontEndCodes.CLOSE);

                    // Sync server and client
                    this.db.Flush();

                    // Read until CLOSE COMPLETE message is received
                    PgResponsePacket response = null;

                    do
                    {
                        response = this.db.ReceiveResponsePacket();
                        this.ProcessSqlPacket(response);
                    }
                    while (!response.IsCloseComplete);

                    // Clear rows
                    this.rows = null;
                    this.rowIndex = 0;

                    // Update Status
                    this.status = PgStatementStatus.Initial;
                }
                catch
                {
                    // Update Status
                    this.status = PgStatementStatus.Error;

                    // Throw exception
                    throw;
                }
            }
        }

        #endregion

        #region · Misc Methods ·

        public string GetPlan(bool verbose)
        {
            lock (db)
            {
                try
                {
                    PgStatement getPlan = new PgStatement();

                    getPlan.DbHandle	= this.db;
                    getPlan.StmtText	= "EXPLAIN ANALYZE ";
                    if (verbose)
                    {
                        getPlan.StmtText += "VERBOSE ";
                    }
                    getPlan.StmtText += stmtText;

                    getPlan.Query();

                    StringBuilder stmtPlan = new StringBuilder();

                    foreach (object[] row in getPlan.Rows)
                    {
                        stmtPlan.AppendFormat("{0} \r\n", row[0]);
                    }

                    getPlan.Close();

                    return stmtPlan.ToString();
                }
                catch (PgClientException)
                {
                    throw;
                }
            }
        }

        #endregion

        #region · Response Methods ·

        private void ProcessSqlPacket(PgResponsePacket packet)
        {
            switch (packet.Message)
            {
                case PgBackendCodes.READY_FOR_QUERY:
                    this.transactionStatus = packet.ReadChar();
                    break;

                case PgBackendCodes.FUNCTION_CALL_RESPONSE:
                    this.ProcessFunctionResult(packet);
                    break;

                case PgBackendCodes.ROW_DESCRIPTION:
                    this.ProcessRowDescription(packet);
                    break;

                case PgBackendCodes.DATAROW:
                    this.hasRows = true;
                    this.ProcessDataRow(packet);
                    break;

                case PgBackendCodes.EMPTY_QUERY_RESPONSE:
                case PgBackendCodes.NODATA:
                    this.hasRows	= false;
                    this.rows		= null;
                    this.rowIndex	= 0;
                    break;

                case PgBackendCodes.COMMAND_COMPLETE:
                    this.ProcessTag(packet);
                    break;

                case PgBackendCodes.PARAMETER_DESCRIPTION:
                    this.ProcessParameterDescription(packet);
                    break;

                case PgBackendCodes.BIND_COMPLETE:
                case PgBackendCodes.PARSE_COMPLETE:
                case PgBackendCodes.CLOSE_COMPLETE:
                    break;
            }
        }

        private void ProcessTag(PgResponsePacket packet)
        {
            string[] elements = null;

            tag = packet.ReadNullString();
            
            elements = tag.Split(' ');

            switch (elements[0])
            {
                case "FETCH":
                case "SELECT":
                    this.recordsAffected = -1;
                    break;

                case "INSERT":
                    this.recordsAffected = Int32.Parse(elements[2]);
                    break;

                case "UPDATE":
                case "DELETE":
                case "MOVE":
                    this.recordsAffected = Int32.Parse(elements[1]);
                    break;
            }
        }

        private void ProcessFunctionResult(PgResponsePacket packet)
        {
            int length = packet.ReadInt32();

            outParameter.Value = packet.ReadValue(outParameter.DataType, length);
        }

        private void ProcessRowDescription(PgResponsePacket packet)
        {
            this.rowDescriptor = new PgRowDescriptor(packet.ReadInt16());

            for (int i = 0; i < rowDescriptor.Fields.Length; i++)
            {
                this.rowDescriptor.Fields[i] = new PgFieldDescriptor();

                this.rowDescriptor.Fields[i].FieldName		= packet.ReadNullString();
                this.rowDescriptor.Fields[i].OidTable		= packet.ReadInt32();
                this.rowDescriptor.Fields[i].OidNumber		= packet.ReadInt16();
                this.rowDescriptor.Fields[i].DataType       = this.db.DataTypes[packet.ReadInt32()];
                this.rowDescriptor.Fields[i].DataTypeSize	= packet.ReadInt16();
                this.rowDescriptor.Fields[i].TypeModifier	= packet.ReadInt32();
                this.rowDescriptor.Fields[i].FormatCode     = (PgTypeFormat)packet.ReadInt16();
            }
        }

        private void ProcessParameterDescription(PgResponsePacket packet)
        {
            this.parameters = new PgParameter[packet.ReadInt16()];

            for (int i = 0; i < parameters.Length; i++)
            {
                this.parameters[i]          = new PgParameter(packet.ReadInt32());
                this.parameters[i].DataType = this.db.DataTypes[this.parameters[i].DataTypeOid];
            }
        }
        
        private void ProcessDataRow(PgResponsePacket packet)
        {
            int			fieldCount	= packet.ReadInt16();
            object[]	values		= new object[fieldCount];

            if (this.rows == null)
            {
                this.rows		= new object[fetchSize];
                this.rowIndex	= 0;
            }
            
            for (int i = 0; i < values.Length; i++)
            {
                int length = packet.ReadInt32();

                switch (length)
                {
                    case -1:
                        values[i] = System.DBNull.Value;
                        break;

                    default:
                        PgTypeFormat formatCode = this.rowDescriptor.Fields[i].DataType.FormatCode;

                        if (this.status == PgStatementStatus.OnQuery)
                        {
                            formatCode = this.rowDescriptor.Fields[i].FormatCode;
                        }

                        if (formatCode == PgTypeFormat.Text)
                        {
                            values[i] = packet.ReadValueFromString(this.rowDescriptor.Fields[i].DataType, length);
                        }
                        else
                        {
                            values[i] = packet.ReadValue(this.rowDescriptor.Fields[i].DataType, length);
                        }
                        break;
                }
            }

            this.rows[this.rowIndex++] = values;
        }

        #endregion
    }
}