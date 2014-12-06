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

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientNotificationEventArgs 
        : EventArgs
    {
        #region · Fields ·

        private int	   processID;
        private string condition;
        private string aditional;

        #endregion

        #region · Properties ·

        public int ProcessID
        {
            get { return this.processID; }
        }

        public string Condition
        {
            get { return this.condition; }
        }

        public string Aditional
        {
            get { return this.aditional; }
        }

        #endregion

        #region · Constructors ·

        public PgClientNotificationEventArgs(int processID, string condition, string addtional)
        {
            this.processID	= processID;
            this.condition	= condition;
            this.aditional	= addtional;
        }

        #endregion
    }
}
