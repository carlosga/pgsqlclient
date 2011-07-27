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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{	
	internal sealed class PgConnectionInternal
        : MarshalByRefObject
	{
		#region · Fields ·

		private PgConnection		                owningConnection;
        private PgDatabase                          database;
		private PgConnectionOptions	                options;
		private PgTransaction		                activeTransaction;
        private SynchronizedCollection<PgCommand>   preparedCommands;
		private long				                created;
        private long                                lifetime;
		private bool				                pooled;
        
		#endregion

		#region · Properties ·

        public PgDatabase Database
		{
			get { return this.database; }
		}

		public PgTransaction ActiveTransaction
		{
			get { return this.activeTransaction; }
			set { this.activeTransaction = value; }
		}

		public long Created
		{
			get { return this.created; }
			set { this.created = value; }
		}
		
		public bool Pooled
		{
			get { return this.pooled; }
			set { this.pooled = value; }
		}

		public PgConnectionOptions Options
		{
			get { return this.options; }
		}

        public SynchronizedCollection<PgCommand> PreparedCommands
		{
			get
			{
				if (this.preparedCommands == null)
				{
                    this.preparedCommands = new SynchronizedCollection<PgCommand>();
				}

				return this.preparedCommands;
			}
		}

		public bool HasActiveTransaction
		{
			get
			{
				return (this.activeTransaction != null && !this.activeTransaction.IsUpdated);
			}
		}

		public PgConnection OwningConnection
		{
			get { return this.owningConnection; }
			set { this.owningConnection = value; }
		}

        public long Lifetime
        {
            get { return this.lifetime; }
            set { this.lifetime = value; }
        }

		#endregion

		#region · Constructors ·

		public PgConnectionInternal(string connectionString)
		{
            this.options    = new PgConnectionOptions(connectionString);
            this.database   = new PgDatabase(this.options);
			this.created	= 0;
            this.lifetime   = 0;
			this.pooled		= true;
		}

		#endregion

		#region · Methods ·

		public void Connect()
		{
			try
			{
				this.database.Connect();
			}
			catch (PgClientException ex)
			{
				throw new PgException(ex.Message, ex);
			}
		}
		
		public void Disconnect()
		{
            try
            {
                this.database.Disconnect();
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }
            finally
            {
		        this.owningConnection   = null;
                this.database           = null;
		        this.options            = null;
		        this.activeTransaction  = null;
		        this.preparedCommands   = null;
		        this.created            = 0;
                this.lifetime           = 0;
                this.pooled             = false;
            }
        }

		public PgTransaction BeginTransaction(IsolationLevel level)
		{
			if (this.activeTransaction != null && !this.activeTransaction.IsUpdated)
			{
				throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
			}

			try
			{
				this.activeTransaction = new PgTransaction(this.owningConnection, level);
				this.activeTransaction.InternalBeginTransaction();
			}
			catch (PgClientException ex)
			{
				throw new PgException(ex.Message, ex);
			}

			return this.activeTransaction;			
		}

        public PgTransaction BeginTransaction(IsolationLevel level, string transactionName)
        {
            if (this.activeTransaction != null && !this.activeTransaction.IsUpdated)
            {
                throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
            }

            try
            {
                this.activeTransaction = new PgTransaction(this.owningConnection, level);
                this.activeTransaction.InternalBeginTransaction();
                this.activeTransaction.Save(transactionName);
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }

            return this.activeTransaction;
        }

		public void DisposeActiveTransaction()
		{
			// Rollback active transation
			if (this.HasActiveTransaction)
			{
				this.activeTransaction.Dispose();
				this.activeTransaction = null;
			}
		}

		public void ClosePreparedCommands()
		{
			if (this.PreparedCommands.Count > 0)
			{
                this.PreparedCommands
                    .ToList()
                    .ForEach(c => c.InternalClose());
				
				this.preparedCommands.Clear();
				this.preparedCommands = null;
			}
		}

		public void AddPreparedCommand(PgCommand command)
		{
			if (!this.PreparedCommands.Contains(command))
			{
				this.PreparedCommands.Add(command);
			}
		}

		public void RemovePreparedCommand(PgCommand command)
		{
			if (this.PreparedCommands.Contains(command))
			{
				this.PreparedCommands.Remove(command);
			}
		}

        #endregion

        #region · Internal Methods ·

        internal bool Verify()
        {
            bool isValid = true;

            try
            {
                // Try to send a Sync message to the PostgreSQL Server
                this.database.Sync();
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }

        internal void FetchDatabaseOids()
        {
            if (this.database.Options.UseDatabaseOids)
            {
                string sql = "SELECT oid FROM pg_type WHERE typname=@typeName";

                if (this.owningConnection != null)
                {
                    PgCommand command = new PgCommand(sql, this.owningConnection);
                    command.Parameters.Add("@typeName", PgDbType.VarChar);

                    // After the connection gets established we should update the Data Types collection oids
                    foreach (PgType type in this.Database.DataTypes)
                    {
                        command.Parameters["@typeName"].Value = type.Name;

                        object realOid = command.ExecuteScalar();

                        if (realOid != null)
                        {
                            if (Convert.ToInt32(realOid) != type.Oid)
                            {
                                type.UpdateOid(Convert.ToInt32(realOid));
                            }
                        }
                    }

                    command.Dispose();
                }
            }
        }

        #endregion
    }
}