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
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgTransaction
        : DbTransaction
    {
        #region · Fields ·

        private PgConnection		connection;
        private IsolationLevel		isolationLevel;
        private bool				disposed;
        private bool				isUpdated;

        #endregion

        #region · Protected Properties ·

        protected override DbConnection DbConnection
        {
            get { return this.connection; }
        }

        #endregion

        #region · Internal Properties ·

        internal bool IsUpdated
        {
            get { return this.isUpdated; }
            set 
            { 
                if (this.connection != null && value)
                {
                    this.connection.InternalConnection.ActiveTransaction = null;
                    this.connection	= null;
                }
                this.isUpdated = value; 
            }
        }

        #endregion

        #region · Properties ·

        public new PgConnection Connection
        {
            get { return this.connection; }
        }

        public override IsolationLevel IsolationLevel 
        {
            get { return this.isolationLevel; }
        }

        #endregion

        #region · Constructors ·

        private PgTransaction() 
            : this(null)
        {
        }
        
        internal PgTransaction(PgConnection connection) 
            : this(connection, IsolationLevel.ReadCommitted)
        {
        }

        internal PgTransaction(PgConnection connection, IsolationLevel isolation)
        {
            this.connection		= connection;
            this.isolationLevel = isolation;
        }				

        #endregion

        #region · Finalizer ·

        ~PgTransaction()
        {
            this.Dispose(false);
        }

        #endregion

        #region · IDisposable Methods ·

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (this.connection != null && !this.isUpdated)
                        {
                            // Implicitly roll back if the transaction still valid.
                            this.Rollback();
                        }
                    }
                    finally
                    {
                        if (this.connection != null)
                        {
                            this.connection.InternalConnection.ActiveTransaction = null;
                            this.connection	= null;
                        }
                        this.disposed	= true;
                        this.isUpdated	= true;
                    }
                }
            }			
        }
        
        #endregion

        #region · DbTransaction Overriden Methods ·

        public override void Commit()
        {
            this.CheckTransaction();

            try
            {
                this.connection.InternalConnection.Database.CommitTransaction();
                
                this.IsUpdated = true;
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }
        }

        public override void Rollback()
        {
            this.CheckTransaction();

            try
            {
                this.connection.InternalConnection.Database.RollbackTransction();
                
                this.IsUpdated = true;
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }
        }

        #endregion

        #region · SavePoint Methods ·

        /// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Save(System.String)"]/*'/>
        public void Save(string savePointName)
        {
            lock (this)
            {
                if (savePointName == null)
                {
                    throw new ArgumentException("No transaction name was be specified.");
                }
                else
                {
                    if (savePointName.Length == 0)
                    {
                        throw new ArgumentException("No transaction name was be specified.");
                    }
                }
                if (this.isUpdated)
                {
                    throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
                }

                try
                {
                    PgCommand command = new PgCommand(
                        "SAVEPOINT " + savePointName,
                        this.connection,
                        this);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (PgClientException ex)
                {
                    throw new PgException(ex.Message, ex);
                }
            }
        }

        /// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Commit(System.String)"]/*'/>
        public void Commit(string savePointName)
        {
            lock (this)
            {
                if (savePointName == null)
                {
                    throw new ArgumentException("No transaction name was be specified.");
                }
                else
                {
                    if (savePointName.Length == 0)
                    {
                        throw new ArgumentException("No transaction name was be specified.");
                    }
                }
                if (this.isUpdated)
                {
                    throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
                }

                try
                {
                    PgCommand command = new PgCommand(
                        "RELEASE SAVEPOINT " + savePointName,
                        this.connection,
                        this);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (PgClientException ex)
                {
                    throw new PgException(ex.Message, ex);
                }
            }
        }

        /// <include file='Doc/en_EN/FbTransaction.xml' path='doc/class[@name="FbTransaction"]/method[@name="Rollback(System.String)"]/*'/>
        public void Rollback(string savePointName)
        {
            lock (this)
            {
                if (savePointName == null)
                {
                    throw new ArgumentException("No transaction name was be specified.");
                }
                else
                {
                    if (savePointName.Length == 0)
                    {
                        throw new ArgumentException("No transaction name was be specified.");
                    }
                }
                if (this.isUpdated)
                {
                    throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
                }

                try
                {
                    PgCommand command = new PgCommand(
                        "ROLLBACK WORK TO SAVEPOINT " + savePointName,
                        this.connection,
                        this);
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                catch (PgClientException ex)
                {
                    throw new PgException(ex.Message, ex);
                }
            }
        }

        #endregion

        #region · Internal Methods ·

        internal void InternalBeginTransaction()
        {
            try
            {
                this.connection.InternalConnection.Database.BeginTransaction(isolationLevel);

                this.IsUpdated = false;
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex.Message, ex);
            }
        }

        #endregion

        #region · Private Methods ·

        private void CheckTransaction()
        {
            if (this.isUpdated)
            {
                throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
            }
        }

        #endregion
    }
}
