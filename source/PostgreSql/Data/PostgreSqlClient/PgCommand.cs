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

using PostgreSql.Data.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgCommand
        : DbCommand, ICloneable
    {				
        #region · Fields ·
        
        private PgConnection		  connection;
        private PgTransaction		  transaction;				
        private PgParameterCollection parameters;
        private UpdateRowSource		  updatedRowSource;
        private PgStatement           statement;
        private PgDataReader          activeDataReader;
        private CommandBehavior       commandBehavior;
        private CommandType           commandType;
        private List<string>          namedParameters;
        private bool                  disposed;
        private string				  commandText;
        private int                   commandTimeout;
        private bool				  designTimeVisible;
        
        #endregion

        #region · DbCommand Properties ·

        protected override DbConnection DbConnection
        {
            get { return this.Connection; }
            set { this.Connection = (PgConnection)value; }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return this.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return this.Transaction; }
            set { this.Transaction = (PgTransaction)value; }
        }

        #endregion

        #region · Properties ·

        public override string CommandText
        {
            get { return this.commandText; }
            set 
            {
                if (this.statement != null && this.commandText != value && !String.IsNullOrEmpty(this.CommandText))
                {
                    this.InternalClose();
                }

                this.commandText = value;
            }
        }

        public override CommandType CommandType
        {
            get { return this.commandType; }
            set { this.commandType = value; }
        }
        
        public override int CommandTimeout
        {
            get { return this.commandTimeout; }			
            set
            {
                if (value < 0) 
                {
                    throw new ArgumentException("The property value assigned is less than 0.");
                }

                this.commandTimeout = value;
            }
        }

        public new PgConnection Connection
        {
            get { return this.connection; }
            set
            {
                if (this.connection != null && this.ActiveDataReader != null)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }

                if (this.connection != value)
                {										
                    if (this.transaction != null)
                    {
                        this.transaction = null;
                    }

                    this.InternalClose();
                }

                this.connection = value;
            }
        }

        public override bool DesignTimeVisible
        {
            get { return this.designTimeVisible; }
            set { this.designTimeVisible = value; }
        }

        public new PgParameterCollection Parameters
        {
            get 
            { 
                if (this.parameters == null)
                {
                    this.parameters = new PgParameterCollection();
                }

                return this.parameters; 
            }
        }

        public new PgTransaction Transaction
        {
            get { return this.transaction; }
            set
            {
                if (this.connection != null && this.ActiveDataReader != null)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }

                this.transaction = value; 
            }
        }
        
        public override UpdateRowSource UpdatedRowSource
        {
            get { return this.updatedRowSource; }
            set { this.updatedRowSource = value; }
        }

        #endregion

        #region · Internal Properties ·

        internal PgDataReader ActiveDataReader
        {
            get { return this.activeDataReader; }
            set { this.activeDataReader = value; }
        }

        internal CommandBehavior CommandBehavior
        {
            get { return this.commandBehavior; }
        }

        internal PgStatement Statement
        {
            get { return this.statement; }
        }

        internal int RecordsAffected
        {
            get 
            { 
                if (this.statement != null)
                {
                    return this.statement.RecordsAffected; 
                }
                return -1;
            }
        }

        internal bool IsDisposed
        {
            get { return this.disposed; }
        }
        
        #endregion

        #region · Private Properties ·

        public List<string> NamedParameters
        {
            get
            {
                if (this.namedParameters == null)
                {
                    this.namedParameters = new List<string>();
                }

                return this.namedParameters;
            }
        }

        #endregion

        #region · Constructors ·

        public PgCommand() 
            : base()
        {
            this.commandText	   = String.Empty;
            this.commandType	   = CommandType.Text;
            this.commandTimeout	   = 30;
            this.updatedRowSource  = UpdateRowSource.Both;
            this.commandBehavior   = CommandBehavior.Default;
            this.designTimeVisible = true;
        }

        public PgCommand(string cmdText) 
            : this(cmdText, null, null)
        {
        }
        
        public PgCommand(string cmdText, PgConnection connection) 
            : this(cmdText, connection, null)
        {
        }
        
        public PgCommand(string cmdText, PgConnection connection, PgTransaction transaction) 
            : this()
        {
            this.CommandText = cmdText;
            this.Connection  = connection;
            this.Transaction = transaction;
        }				 

        #endregion

        #region · IDisposable Methods ·
        
        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // release any managed resources

                        if (this.connection != null && this.connection.InternalConnection != null)
                        {
                            this.connection.InternalConnection.RemovePreparedCommand(this);
                        }
                        
                        this.InternalClose();

                        this.commandText = null;

                        if (this.namedParameters != null)
                        {
                            this.namedParameters.Clear();
                            this.namedParameters = null;
                        }
                    }
                    
                    // release any unmanaged resources
                    this.disposed = true;
                }
                finally 
                {
                    base.Dispose(disposing);
                }
            }
        }

        #endregion

        #region · ICloneable Methods ·

        object ICloneable.Clone()
        {
            PgCommand command = new PgCommand();
            
            command.CommandText		 = this.commandText;
            command.Connection		 = this.connection;
            command.Transaction		 = this.transaction;
            command.CommandType		 = this.CommandType;
            command.UpdatedRowSource = this.UpdatedRowSource;
            
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                command.Parameters.Add(((ICloneable)this.Parameters[i]).Clone());
            }

            return command;
        }

        #endregion

        #region · Protected Methods ·

        protected override DbParameter CreateDbParameter()
        {
            return this.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return this.ExecuteReader(behavior);
        }

        #endregion

        #region · Methods ·

        public override void Cancel()
        {			
            throw new NotSupportedException();
        }
        
        public new PgParameter CreateParameter()
        {
            return new PgParameter();
        }

        public override int ExecuteNonQuery()
        {
            this.CheckCommand();

            this.InternalPrepare();
            this.InternalExecute();

            this.InternalSetOutputParameters();
            
            return this.statement.RecordsAffected;
        }
                
        public new PgDataReader ExecuteReader()
        {	
            return this.ExecuteReader(CommandBehavior.Default);			
        }
        
        public new PgDataReader ExecuteReader(CommandBehavior behavior)
        {
            this.CheckCommand();

            this.commandBehavior = behavior;

            this.InternalPrepare();

            if ((commandBehavior & CommandBehavior.SequentialAccess) == CommandBehavior.SequentialAccess 
             || (commandBehavior & CommandBehavior.SingleResult)     == CommandBehavior.SingleResult 
             || (commandBehavior & CommandBehavior.SingleRow)        == CommandBehavior.SingleRow 
             || (commandBehavior & CommandBehavior.CloseConnection)  == CommandBehavior.CloseConnection 
             || commandBehavior == System.Data.CommandBehavior.Default)				
            {
                this.InternalExecute();
            }

            this.activeDataReader = new PgDataReader(this.connection, this);

            return this.activeDataReader;
        }

        public override object ExecuteScalar()
        {
            this.CheckCommand();

            object returnValue = null;

            this.InternalPrepare();
            this.InternalExecute();

            if (this.statement != null && this.statement.HasRows)
            {
                returnValue = ((object[])this.statement.Rows[0])[0];
            }

            return returnValue;
        }

        public override void Prepare()
        {
            this.CheckCommand();

            this.InternalPrepare();
        }

        #endregion

        #region · Internal Methods ·

        internal void InternalPrepare()
        {
            PgConnectionInternal conn = this.connection.InternalConnection;

            conn.AddPreparedCommand(this);			

            try
            {
                string sql = this.commandText;

                if (this.statement        == null 
                 || this.statement.Status == PgStatementStatus.Initial 
                 || this.statement.Status == PgStatementStatus.Error)
                {
                    if (this.commandType == CommandType.StoredProcedure)
                    {
                        sql = this.BuildStoredProcedureSql(sql);
                    }

                    string statementName = this.GetStmtName();
                    string prepareName   = String.Format("PS{0}", statementName);
                    string portalName    = String.Format("PR{0}", statementName);

                    this.statement = conn.Database.CreateStatement(prepareName, portalName, this.ParseNamedParameters(sql));

                    // Parse statement
                    this.statement.Parse();

                    // Describe statement
                    this.statement.Describe();
                }
                else
                {
                    // Close existent portal
                    this.statement.ClosePortal();
                }
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }
        }

        internal void InternalExecute()
        {
            try
            {
                if (this.Parameters.Count != 0)
                {
                    // Set parameter values
                    this.SetParameterValues();
                }

                // Bind Statement
                this.statement.Bind();

                // Execute Statement
                this.statement.Execute();
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }
        }

        internal void InternalClose()
        {
            if (this.statement != null)
            {
                try
                {
                    // Closing the prepared statement closes all his portals too.
                    this.statement.Close();
                    this.statement = null;
                }
                catch (PgClientException ex)
                {
                    throw new PgException(ex.Message, ex);
                }
            }
        }
        
        internal void InternalSetOutputParameters()
        {
            if (this.CommandType == CommandType.StoredProcedure && 
                this.Parameters.Count > 0)
            {
                IEnumerator paramEnumerator = this.Parameters.GetEnumerator();
                int i = 0;

                if (this.statement.Rows != null && this.statement.Rows.Length > 0)
                {
                    object[] values = (object[])this.statement.Rows[0];

                    if (values != null && values.Length > 0)
                    {
                        while (paramEnumerator.MoveNext())
                        {
                            PgParameter parameter = ((PgParameter)paramEnumerator.Current);

                            if (parameter.Direction == ParameterDirection.Output 
                             || parameter.Direction == ParameterDirection.InputOutput 
                             || parameter.Direction == ParameterDirection.ReturnValue)
                            {
                                parameter.Value = values[i];
                                i++;
                            }
                        }
                    }
                }
            }			
        }

        #endregion

        #region · Private Methods ·

        private void CheckCommand()
        {
            if (this.transaction != null && this.transaction.IsUpdated)
            {
                this.transaction = null;
            }

            if (this.connection == null || this.connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection must valid and open");

            }
            if (this.ActiveDataReader != null)
            {
                throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
            }

            if (this.Transaction == null && this.connection.InternalConnection.HasActiveTransaction)
            {
                throw new InvalidOperationException("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction.  The Transaction property of the Command has not been initialized.");
            }

            if (this.Transaction != null && !this.connection.Equals(Transaction.Connection))
            {
                throw new InvalidOperationException("Command Connection is not equal to Transaction Connection");
            }

            if (this.commandText == null || this.commandText.Length == 0)
            {
                throw new InvalidOperationException ("The command text for this Command has not been set.");
            }
        }

        private string BuildStoredProcedureSql(string commandText)
        {
            if (!commandText.Trim().ToLower().StartsWith("select "))
            {
                StringBuilder paramsText = new StringBuilder();

                // Append the stored proc parameter name
                paramsText.Append(commandText);
                paramsText.Append("(");

                for (int i = 0; i < this.Parameters.Count; i++)
                {
                    if (this.Parameters[i].Direction == ParameterDirection.Input 
                     || this.Parameters[i].Direction == ParameterDirection.InputOutput)
                    {
                        // Append parameter name to parameter list
                        paramsText.Append(this.Parameters[i].ParameterName);

                        if (i != this.Parameters.Count - 1)
                        {
                            paramsText = paramsText.Append(",");
                        }
                    }
                }

                paramsText.Append(")");
                paramsText.Replace(",)", ")");
                
                commandText = String.Format("select * from {0}", paramsText.ToString());
            }

            return commandText;
        }

        private string GetStmtName()
        {
            return Guid.NewGuid().GetHashCode().ToString();
        }

        private string ParseNamedParameters(string sql)
        {
            StringBuilder builder      = new StringBuilder();
            StringBuilder paramBuilder = new StringBuilder();
            bool          inCommas     = false;
            bool          inParam      = false;
            int           paramIndex   = 0;

            this.NamedParameters.Clear();

            if (sql.IndexOf('@') == -1)
            {
                return sql;
            }

            for (int i = 0; i < sql.Length; i++)
            {
                char sym = sql[i];

                if (inParam)
                {
                    if (Char.IsLetterOrDigit(sym) || sym == '_' || sym == '$')
                    {
                        paramBuilder.Append(sym);
                    }
                    else
                    {
                        this.NamedParameters.Add(paramBuilder.ToString());
                        paramBuilder.Length = 0;
                        builder.AppendFormat("${0}", ++paramIndex);
                        builder.Append(sym);
                        inParam = false;
                    }
                }
                else
                {
                    if (sym == '\'' || sym == '\"')
                    {
                        inCommas = !inCommas;
                    }
                    else if (!inCommas && sym == '@')
                    {
                        inParam = true;
                        paramBuilder.Append(sym);
                        continue;
                    }

                    builder.Append(sym);
                }
            }

            if (inParam)
            {
                this.NamedParameters.Add(paramBuilder.ToString());
                builder.AppendFormat("${0}", ++paramIndex);
            }

            return builder.ToString();
        }

        private void SetParameterValues()
        {
            if (this.Parameters.Count != 0)
            {
                for (int i = 0; i < this.statement.Parameters.Length; i++)
                {
                    int index = i;

                    if (this.NamedParameters.Count > 0)
                    {
                        index = this.Parameters.IndexOf(this.NamedParameters[i]);
                    }

                    if (this.Parameters[index].Direction == ParameterDirection.Input 
                     || this.Parameters[index].Direction == ParameterDirection.InputOutput)
                    {
                        if (this.Parameters[index].Value == System.DBNull.Value)
                        {
                            this.statement.Parameters[i].Value = null;
                        }
                        else
                        {
                            this.statement.Parameters[i].Value = this.Parameters[index].Value;
                        }
                    }
                }
            }
        }

        #endregion
    }
}