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
using System.Text.RegularExpressions;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgConnectionOptions
    {
        #region · Fields ·

        private string	dataSource;		
        private string	database;
        private string	userID;
        private string	password;
        private int		portNumber;
        private int		packetSize;		
        private int		connectionTimeout;
        private long	connectionLifetime;
        private int		minPoolSize;
        private int		maxPoolSize;
        private bool	pooling;
        private	bool	ssl;
        private bool    useDatabaseOids;

        #endregion

        #region · Properties ·

        public string DataSource
        {
            get { return this.dataSource; }
        }

        public string Database
        {
            get { return this.database; }
            set { this.database = value; }
        }

        public string UserID
        {
            get { return this.userID; }
        }

        public string Password
        {
            get { return this.password; }
        }

        public int PacketSize
        {
            get { return this.packetSize; }
        }

        public int PortNumber
        {
            get { return this.portNumber; }
        }

        public int ConnectionTimeout
        {
            get { return this.connectionTimeout; }
        }

        public long ConnectionLifeTime
        {
            get { return this.connectionLifetime; }
        }
        
        public int MinPoolSize
        {
            get { return this.minPoolSize; }
        }

        public int MaxPoolSize
        {
            get { return this.maxPoolSize; }
        }

        public bool Pooling
        {
            get { return this.pooling; }
        }

        public bool Ssl
        {
            get { return this.ssl; }
        }

        public bool UseDatabaseOids
        {
            get { return this.useDatabaseOids; }
        }

        #endregion

        #region · Constructors ·

        public PgConnectionOptions(string connectionString)
        {
            if (connectionString == null)
            {
                throw new InvalidOperationException("connectionString cannot be null.");
            }

            this.SetDefaultValues();
            this.ParseConnectionString(connectionString);
        }

        #endregion

        #region · Private Methods ·

        private void SetDefaultValues()
        {
            this.dataSource				= "localhost";			
            this.userID					= "postgres";
            this.password				= null;
            this.portNumber				= 5432;
            this.packetSize				= 8192;
            this.pooling				= true;
            this.connectionTimeout		= 15;
            this.connectionLifetime		= 0;
            this.minPoolSize			= 0;
            this.maxPoolSize			= 100;
            this.ssl                    = false;
            this.useDatabaseOids        = false;
        }

        private void ParseConnectionString(string connectionString)
        {
            Regex			search		= new Regex(@"([\w\s\d]*)\s*=\s*([^;]*)");
            MatchCollection	elements	= search.Matches(connectionString);

            foreach (Match element in elements)
            {
                if (!String.IsNullOrEmpty(element.Groups[2].Value))
                {
                    switch (element.Groups[1].Value.Trim().ToLower())
                    {
                        case "data source":
                        case "server":
                        case "host":
                            this.dataSource = element.Groups[2].Value.Trim();
                            break;

                        case "database":
                        case "initial catalog":
                            this.database = element.Groups[2].Value.Trim();
                            break;

                        case "user name":
                        case "user id":
                        case "user":
                            this.userID = element.Groups[2].Value.Trim();
                            break;

                        case "user password":
                        case "password":
                            this.password = element.Groups[2].Value.Trim();
                            break;

                        case "port number":
                            this.portNumber = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "connection timeout":
                            this.connectionTimeout = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "packet size":
                            this.packetSize = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "pooling":
                            this.pooling = Boolean.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "connection lifetime":
                            this.connectionLifetime = Int64.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "min pool size":
                            this.minPoolSize = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "max pool size":
                            this.maxPoolSize = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "ssl":
                            this.ssl = Boolean.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "use database oids":
                            this.useDatabaseOids = Boolean.Parse(element.Groups[2].Value.Trim());
                            break;
                    }
                }
            }

            if (this.UserID.Length == 0 || this.DataSource.Length == 0)
            {
                throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
            }
            else
            {
                if (this.PacketSize < 512 || this.PacketSize > 32767)
                {
                    string msg = String.Format("'Packet Size' value of {0} is not valid.\r\nThe value should be an integer >= 512 and <= 32767.", this.PacketSize);

                    throw new ArgumentException(msg);
                }
            }
        }

        #endregion
    }
}