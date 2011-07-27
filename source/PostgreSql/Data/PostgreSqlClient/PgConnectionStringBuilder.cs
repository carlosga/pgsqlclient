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
using System.Collections.Generic;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgConnectionStringBuilder
        : DbConnectionStringBuilder
    {
        #region · Static Fields ·

        static readonly Dictionary<string, string> Synonyms = InitializeSynonyms();

        #endregion

        #region · Static Methods ·

        static Dictionary<string, string> InitializeSynonyms()
        {
            Dictionary<string, string> synonyms = new Dictionary<string, string>(new CaseInsensitiveEqualityComparer());

            synonyms.Add("data source"          , "data source");
            synonyms.Add("server"               , "data source");
            synonyms.Add("host"                 , "data source");
            synonyms.Add("database"             , "initial catalog");
            synonyms.Add("initial catalog"      , "initial catalog");
            synonyms.Add("user id"              , "user id");
            synonyms.Add("user name"            , "user id");
            synonyms.Add("user"                 , "user id");
            synonyms.Add("user password"        , "password");
            synonyms.Add("password"             , "password");
            synonyms.Add("port number"          , "port number");
            synonyms.Add("port"                 , "port number");
            synonyms.Add("packet size"          , "packet size");
            synonyms.Add("connection timeout"   , "connection timeout");
            synonyms.Add("pooling"              , "pooling");
            synonyms.Add("connection lifetime"  , "connection lifetime");
            synonyms.Add("min pool size"        , "min pool size");
            synonyms.Add("max pool size"        , "max pool size");
            synonyms.Add("ssl"                  , "ssl");
            synonyms.Add("use database oids"    , "use database oids");

            return synonyms;
        }

        #endregion

        #region · Properties ·

        public string DataSource
        {
            get { return this.GetString("Data Source"); }
            set { this.SetValue("Data Source", value); }
        }

        public string InitialCatalog
        {
            get { return this.GetString("Initial Catalog"); }
            set { this.SetValue("Initial Catalog", value); }
        }

        public string UserID
        {
            get { return this.GetString("User ID"); }
            set { this.SetValue("User ID", value); }
        }

        public string Password
        {
            get { return this.GetString("Password"); }
            set { this.SetValue("Password", value); }
        }

        public int PortNumber
        {
            get { return this.GetInt32("Port Number"); }
            set { this.SetValue("Port Number", value); }
        }

        public int PacketSize
        {
            get { return this.GetInt32("Packet Size"); }
            set { this.SetValue("Packet Size", value); }
        }

        public int ConnectionTimeout
        {
            get { return this.GetInt32("Connection Timeout"); }
            set { this.SetValue("Connection Timeout", value); }
        }

        public bool Pooling
        {
            get { return this.GetBoolean("Pooling"); }
            set { this.SetValue("Pooling", value); }
        }

        public int ConnectionLifeTime
        {
            get { return this.GetInt32("Connection Lifetime"); }
            set { this.SetValue("Connection Lifetime", value); }
        }

        public int MinPoolSize
        {
            get { return this.GetInt32("Min Pool Size"); }
            set { this.SetValue("Min Pool Size", value); }
        }

        public int MaxPoolSize
        {
            get { return this.GetInt32("Max Pool Size"); }
            set { this.SetValue("Max Pool Size", value); }
        }

        public bool Ssl
        {
            get { return this.GetBoolean("Ssl"); }
            set { this.SetValue("Ssl", value); }
        }

        public bool UseDatabaseOids
        {
            get { return this.GetBoolean("use database oids"); }
            set { this.SetValue("use database oids", value); }
        }

        #endregion

        #region · Constructors ·

        public PgConnectionStringBuilder()
        {
        }

        public PgConnectionStringBuilder(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        #endregion

        #region · Private methods ·

        private int GetInt32(string keyword)
        {
            return Convert.ToInt32(this.GetKey(keyword));
        }

        private string GetString(string keyword)
        {
            return Convert.ToString(this.GetKey(keyword));
        }

        private bool GetBoolean(string keyword)
        {
            return Convert.ToBoolean(this.GetKey(keyword));
        }

        private void SetValue(string keyword, object value)
        {
            this[this.GetKey(keyword)] = value;
        }

        private string GetKey(string keyword)
        {
            string synonymKey = (string)Synonyms[keyword];

            // First check if there are yet a property for the requested keyword
            foreach (string key in this.Keys)
            {
                if (Synonyms.ContainsKey(key) && (string)Synonyms[key] == synonymKey)
                {
                    synonymKey = key;
                    break;
                }
            }

            return synonymKey;
        }

        #endregion
    }
}
