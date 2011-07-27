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
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;

namespace PostgreSql.Data.PostgreSqlClient
{	
    [DefaultEvent("RowUpdated")]
    public sealed class PgDataAdapter 
        : DbDataAdapter, ICloneable
    {
        #region · Events ·

        private static readonly object EventRowUpdated = new object();
        private static readonly object EventRowUpdating = new object(); 

        public event PgRowUpdatedEventHandler RowUpdated
        {
            add { Events.AddHandler(EventRowUpdated, value); }
            remove { Events.RemoveHandler(EventRowUpdated, value); }
        }

        [Category("DataCategory_Update")]
        public event PgRowUpdatingEventHandler RowUpdating
        {
            add { Events.AddHandler(EventRowUpdating, value); }
            remove { Events.RemoveHandler(EventRowUpdating, value); }
        }

        #endregion

        #region · Properties ·

        [Category("DataCategory_Update")]
        [DefaultValue(null)]
        public new PgCommand SelectCommand 
        {
            get { return (PgCommand)base.SelectCommand; }
            set { base.SelectCommand = value; }
        }

        [Category("DataCategory_Update")]
        [DefaultValue(null)]
        public new PgCommand InsertCommand 
        {
            get { return (PgCommand)base.InsertCommand; }
            set { base.InsertCommand = value; }
        }
        
        [Category("DataCategory_Fill")]
        [DefaultValue(null)]
        public new PgCommand UpdateCommand 
        {
            get { return (PgCommand)base.UpdateCommand; }
            set { base.UpdateCommand = value; }
        }

        [Category("DataCategory_Update")]
        [DefaultValue(null)]
        public new PgCommand DeleteCommand 
        {
            get { return (PgCommand)base.DeleteCommand; }
            set { base.DeleteCommand = value; }
        }

        #endregion

        #region · Constructors ·

        public PgDataAdapter() 
            : base()
        {
            GC.SuppressFinalize(this);
        }

        public PgDataAdapter(PgCommand selectCommand) 
            : this()
        {
            this.SelectCommand = selectCommand;
        }
        
        public PgDataAdapter(string commandText, PgConnection connection) 
            : this()
        {
            this.SelectCommand = new PgCommand(commandText, connection);
        }

        public PgDataAdapter(string commandText, string connectionString) 
            : this()
        {
            this.SelectCommand = new PgCommand(commandText, new PgConnection(connectionString));
        }

        #endregion

        #region · Protected Methods ·
        
        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new PgRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            PgRowUpdatedEventHandler handler = (PgRowUpdatedEventHandler) Events[EventRowUpdated];
            if ((null != handler) && (value is PgRowUpdatedEventArgs)) 
            {
                handler(this, (PgRowUpdatedEventArgs) value);
            }
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new PgRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            PgRowUpdatingEventHandler handler = (PgRowUpdatingEventHandler) Events[EventRowUpdating];
            if ((null != handler) && (value is PgRowUpdatingEventArgs)) 
            {
                handler(this, (PgRowUpdatingEventArgs) value);
            }
        }

        #endregion
    }
}
