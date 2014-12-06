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
using System.Threading;

namespace PostgreSql.Data.PostgreSqlClient
{
    internal sealed class PgPoolManager
    {
        #region � Static Fields �

        private static readonly PgPoolManager instance = new PgPoolManager();

        #endregion

        #region � Static Properties �

        public static PgPoolManager Instance
        {
            get { return PgPoolManager.instance; }
        }

        #endregion

        #region � Fields �

        private Hashtable pools;
        private Hashtable handlers;
        private object	  syncObject;

        #endregion

        #region � Properties �

        public int PoolsCount
        {
            get
            {
                if (this.pools != null)
                {
                    return this.pools.Count;
                }

                return 0;
            }
        }

        #endregion

        #region � Private Properties �

        private Hashtable Pools
        {
            get
            {
                if (this.pools == null)
                {
                    this.pools = Hashtable.Synchronized(new Hashtable());
                }

                return this.pools;
            }
        }

        private Hashtable Handlers
        {
            get
            {
                if (this.handlers == null)
                {
                    this.handlers = Hashtable.Synchronized(new Hashtable());
                }

                return this.handlers;
            }
        }

        private object SyncObject
        {
            get
            {
                if (this.syncObject == null)
                {
                    Interlocked.CompareExchange(ref this.syncObject, new object(), null);
                }

                return this.syncObject;
            }
        }

        #endregion

        #region � Constructors �

        private PgPoolManager()
        {
        }

        #endregion

        #region � Methods �

        public PgConnectionPool GetPool(string connectionString)
        {
            PgConnectionPool pool = this.FindPool(connectionString);

            if (pool == null)
            {
                pool = this.CreatePool(connectionString);
            }

            return pool;
        }

        public PgConnectionPool FindPool(string connectionString)
        {
            PgConnectionPool pool = null;

            lock (this.SyncObject)
            {
                int hashCode = connectionString.GetHashCode();

                if (this.Pools.ContainsKey(hashCode))
                {
                    pool = (PgConnectionPool)this.Pools[hashCode];
                }
            }

            return pool;
        }

        public PgConnectionPool CreatePool(string connectionString)
        {
            PgConnectionPool pool = null;

            lock (this.SyncObject)
            {
                pool = this.FindPool(connectionString);

                if (pool == null)
                {
                    lock (this.Pools.SyncRoot)
                    {
                        int hashcode = connectionString.GetHashCode();

                        // Create an empty pool	handler
                        EmptyPoolEventHandler handler = new EmptyPoolEventHandler(this.OnEmptyPool);

                        this.Handlers.Add(hashcode, handler);

                        // Create the new connection pool
                        pool = new PgConnectionPool(connectionString);

                        this.Pools.Add(hashcode, pool);

                        pool.EmptyPool += handler;
                    }
                }
            }

            return pool;
        }

        public void ClearAllPools()
        {
            lock (this.SyncObject)
            {
                lock (this.Pools.SyncRoot)
                {
                    PgConnectionPool[] tempPools = new PgConnectionPool[this.Pools.Count];

                    this.Pools.Values.CopyTo(tempPools, 0);

                    foreach (PgConnectionPool pool in tempPools)
                    {
                        // Clear pool
                        pool.Clear();
                    }

                    // Clear Hashtables
                    this.Pools.Clear();
                    this.Handlers.Clear();
                }
            }
        }

        public void ClearPool(string connectionString)
        {
            lock (this.SyncObject)
            {
                lock (this.Pools.SyncRoot)
                {
                    int hashCode = connectionString.GetHashCode();

                    if (this.Pools.ContainsKey(hashCode))
                    {
                        PgConnectionPool pool = (PgConnectionPool)this.Pools[hashCode];

                        // Clear pool
                        pool.Clear();
                    }
                }
            }
        }

        public int GetPooledConnectionCount(string connectionString)
        {
            PgConnectionPool pool = this.FindPool(connectionString);

            return (pool != null) ? pool.Count : 0;
        }

        #endregion

        #region � Event Handlers �

        private void OnEmptyPool(object sender, EventArgs e)
        {
            lock (this.Pools.SyncRoot)
            {
                int hashCode = (int)sender;

                if (this.Pools.ContainsKey(hashCode))
                {
                    PgConnectionPool pool = (PgConnectionPool)this.Pools[hashCode];

                    lock (pool.SyncObject)
                    {
                        EmptyPoolEventHandler handler = (EmptyPoolEventHandler)this.Handlers[hashCode];

                        pool.EmptyPool -= handler;

                        this.Pools.Remove(hashCode);
                        this.Handlers.Remove(hashCode);

                        pool    = null;
                        handler = null;
                    }
                }
            }
        }

        #endregion
    }
}
