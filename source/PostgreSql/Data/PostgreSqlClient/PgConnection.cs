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
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using PostgreSql.Data.Protocol;
using PostgreSql.Data.Schema;
#if (CUSTOM_SSL)
using SecureSocketLayer.Net.Security;
#else
using System.Net.Security;
#endif

namespace PostgreSql.Data.PostgreSqlClient
{	
    [DefaultEvent("InfoMessage")]
    public sealed class PgConnection
        : DbConnection, ICloneable
    {	
        #region · Events ·

        public override event StateChangeEventHandler StateChange;
        public event PgInfoMessageEventHandler		InfoMessage;
        public event PgNotificationEventHandler		Notification;

        #endregion

        #region · SSL Events ·

        public event RemoteCertificateValidationCallback UserCertificateValidation;
        public event LocalCertificateSelectionCallback UserCertificateSelection;

        #endregion

        #region · Fields ·

        private PgConnectionInternal	connectionInternal;
        private PgConnectionOptions		options;
        private ConnectionState			state;
        private bool					disposed;
        private string					connectionString;

        #endregion
        
        #region · Properties ·

        public override string ConnectionString
        {
            get { return this.connectionString; }
            set
            { 
                if (state == ConnectionState.Closed)
                {
                    this.options = new PgConnectionOptions(value);
                    this.connectionString = value;
                }
            }
        }

        public override int ConnectionTimeout
        {
            get 
            { 
                if (this.connectionInternal != null)
                {
                    return this.connectionInternal.Options.ConnectionTimeout;
                }
                else
                {
                    return 15; 
                }
            }
        }

        public override string Database
        {
            get 
            { 
                if (this.connectionInternal != null)
                {
                    return this.connectionInternal.Options.Database;
                }
                else
                {
                    return String.Empty; 
                }
            }
        }

        public override string DataSource
        {
            get 
            { 
                if (this.connectionInternal != null)
                {
                    return this.connectionInternal.Options.DataSource;
                }
                else
                {
                    return String.Empty; 
                }
            }
        }

        public int PacketSize
        {
            get 
            { 
                int packetSize = 8192;
                if (this.connectionInternal != null)
                {
                    packetSize = this.connectionInternal.Options.PacketSize;
                }

                return packetSize; 
            }
        }

        public override string ServerVersion
        {
            get
            {
                if (this.connectionInternal != null)
                {
                    return (string)this.connectionInternal.Database.ParameterStatus["server_version"];
                }
                else
                {
                    return String.Empty; 
                }
            }
        }

        public override ConnectionState State
        {
            get { return this.state; }
        }

        #endregion

        #region · Internal Properties ·

        internal PgConnectionInternal InternalConnection
        {
            get { return this.connectionInternal; }
            set { this.connectionInternal = value; }
        }

        #endregion		

        #region · Constructors ·

        public PgConnection() 
            : this(null)
        {			
        }
            
        public PgConnection(string connectionString) 
            : base()
        {			
            this.state				= ConnectionState.Closed;
            this.connectionString	= String.Empty;

            if (connectionString != null)
            {
                this.ConnectionString = connectionString;
            }
        }		

        #endregion

        #region · IDisposable Methods ·

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                try
                {	
                    if (disposing)
                    {
                        // release any managed resources
                        this.Close();

                        this.connectionInternal	= null;
                        this.connectionString	= null;
                    }

                    // release any unmanaged resources
                }
                finally
                {
                    base.Dispose(disposing);
                }

                this.disposed = true;
            }			
        }

        #endregion

        #region · ICloneable Methods ·

        object ICloneable.Clone()
        {
            return new PgConnection(this.connectionString);
        }

        #endregion

        #region · Protected Methods ·

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return this.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return this.CreateCommand();
        }

        #endregion

        #region · Begin Transaction Methods ·

        public new PgTransaction BeginTransaction()
        {
            return this.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public new PgTransaction BeginTransaction(IsolationLevel level)
        {
            if (this.State == ConnectionState.Closed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }

            return this.connectionInternal.BeginTransaction(level);
        }

        public PgTransaction BeginTransaction(string transactionName)
        {
            return this.BeginTransaction(IsolationLevel.ReadCommitted, transactionName);
        }

        public PgTransaction BeginTransaction(IsolationLevel level, string transactionName)
        {
            if (this.State == ConnectionState.Closed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }

            return this.connectionInternal.BeginTransaction(level, transactionName);
        }

        #endregion

        #region · Methods ·

        public override void ChangeDatabase(string db)
        {
            if (this.state == ConnectionState.Closed)
            {
                throw new InvalidOperationException("ChangeDatabase requires an open and available Connection.");
            }

            if (db == null || db.Trim().Length == 0)
            {
                throw new InvalidOperationException("Database name is not valid.");
            }

            string oldDb = this.connectionInternal.Options.Database;

            try
            {
                /* Close current connection	*/
                this.Close();

                /* Set up the new Database	*/
                this.connectionInternal.Options.Database = db;

                /* Open new connection to new database	*/
                this.Open();
            }
            catch (PgException)
            {
                this.connectionInternal.Options.Database = oldDb;				
                throw;
            }
        }

        public override void Open()
        {
            if (this.connectionString == null || this.connectionString.Length == 0)
            {
                throw new InvalidOperationException("Connection String is not initialized.");
            }
            if (this.state != ConnectionState.Closed)
            {
                throw new InvalidOperationException("Connection already Open.");
            }

            try
            {
                this.state = ConnectionState.Connecting;

                // Open connection
                if (this.options.Pooling)
                {
                    this.connectionInternal = PgPoolManager.Instance.GetPool(this.connectionString).CheckOut();
                }
                else
                {
                    this.connectionInternal = new PgConnectionInternal(this.connectionString);
                    this.connectionInternal.Pooled = false;
                }

                this.SslSetup();
                this.connectionInternal.OwningConnection = this;
                this.connectionInternal.Connect();
                
                // Set connection state to Open
                this.state = ConnectionState.Open;

                if (this.StateChange != null)
                {
                    this.StateChange(this, new StateChangeEventArgs(ConnectionState.Closed, state));
                }

                // Grab Data Types Oid's from the database if requested
                this.connectionInternal.FetchDatabaseOids();

                // Add Info message event handler
                this.connectionInternal.Database.InfoMessage = new InfoMessageCallback(this.OnInfoMessage);

                // Add notification event handler
                this.connectionInternal.Database.Notification = new NotificationCallback(this.OnNotification);
            }
            catch (PgClientException ex)
            {
                this.state = ConnectionState.Closed;
                throw new PgException(ex.Message, ex);
            }
        }

        public override void Close()
        {
            if (this.state == ConnectionState.Open)
            {
                try
                {
                    lock (this.connectionInternal)
                    {
                        PgDatabase database = this.connectionInternal.Database;

                        // Remove info message callback
                        this.connectionInternal.Database.InfoMessage = null;

                        // Remove notification callback
                        this.connectionInternal.Database.Notification = null;

                        // Remove SSL callback handlers
                        this.connectionInternal.Database.UserCertificateValidationCallback = null;
                        this.connectionInternal.Database.UserCertificateSelectionCallback = null;

                        // Dispose Active commands
                        this.connectionInternal.ClosePreparedCommands();

                        // Rollback active transaction
                        this.connectionInternal.DisposeActiveTransaction();

                        // Close connection permanently or send it back to the pool
                        if (this.connectionInternal.Pooled)
                        {
                            PgPoolManager.Instance.GetPool(this.connectionString).CheckIn(this.connectionInternal);
                        }
                        else
                        {
                            this.connectionInternal.Disconnect();
                        }
                    }
                }
                catch
                {
                }
                finally
                {

                    // Update state
                    this.state = ConnectionState.Closed;

                    // Raise StateChange event
                    if (this.StateChange != null)
                    {
                        this.StateChange(this, new StateChangeEventArgs(ConnectionState.Open, this.state));
                    }
                }
            }
        }

        public new PgCommand CreateCommand()
        {		
            PgCommand command = new PgCommand();
            command.Connection = this;
    
            return command;
        }

        #endregion

        #region · Schema Methods ·

        public override DataTable GetSchema()
        {
            return this.GetSchema("MetaDataCollections");
        }

        public override DataTable GetSchema(string collectionName)
        {
            return this.GetSchema(collectionName, null);
        }

        public override DataTable GetSchema(string collectionName, string[] restrictions)
        {
            if (this.state == ConnectionState.Closed)
            {
                throw new InvalidOperationException("Connection should be valid and open.");
            }

            return PgSchemaFactory.GetSchema(this, collectionName, restrictions);
        }

        #endregion

        #region · SSL Setup ·

        internal void SslSetup()
        {
            // Add SSL callback handlers
            this.connectionInternal.Database.UserCertificateValidationCallback = new RemoteCertificateValidationCallback(OnUserCertificateValidation);
            this.connectionInternal.Database.UserCertificateSelectionCallback = new LocalCertificateSelectionCallback(OnUserCertificateSelection);
        }

        #endregion

        #region · Event Handlers Methods ·

        private void OnInfoMessage(PgClientException ex)
        {
            if (this.InfoMessage != null)
            {
                this.InfoMessage(this, new PgInfoMessageEventArgs(ex));
            }
        }

        private void OnNotification(int processID, string condition, string aditional)
        {
            if (this.Notification != null)
            {
                this.Notification(this, new PgNotificationEventArgs(processID, condition, aditional));
            }
        }

        private bool OnUserCertificateValidation(
            object          sender,
            X509Certificate certificate,
            X509Chain       chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (this.UserCertificateValidation != null)
            {
                return this.UserCertificateValidation(this, certificate, chain, sslPolicyErrors);
            }

            return false;
        }

        private X509Certificate OnUserCertificateSelection(
            object                      sender,
            string                      targetHost,
            X509CertificateCollection   localCertificates,
            X509Certificate             remoteCertificate,
            string[]                    acceptableIssuers)
        {
            if (this.UserCertificateSelection != null)
            {
                return this.UserCertificateSelection(this, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
            }

            return null;
        }

        #endregion
    }
}
