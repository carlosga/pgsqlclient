/* PgSqlClient - ADO.NET Data Provider for PostgreSQL 7.4+
 * Copyright (c) 2003-2006 Carlos Guzman Alvarez
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Data;
using PostgreSql.Data.PostgreSqlClient;
using NUnit.Framework;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
	[TestFixture]
	public class PgDatabaseSchemaTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void Aggregates()
		{
			DataTable aggregates = Connection.GetSchema("Aggregates", null);
		}
		
		[Test]
		public void Casts()
		{
			DataTable casts = Connection.GetSchema("Casts", null);
		}

		[Test]
		public void CheckConstraints()
		{
			DataTable checkConstraints = Connection.GetSchema("CheckConstraints", null);
		}

		[Test]
		public void Columns()
		{
			DataTable columns = Connection.GetSchema("Columns", null);
		}

		[Test]
		public void Databases()
		{
			DataTable databases = Connection.GetSchema("Databases", null);
		}

        [Test]
        public void DataSourceInformation()
        {
            DataTable dataSourceInformation = Connection.GetSchema("DataSourceInformation", null);
        }

        [Test]
        public void DataTypes()
        {
            DataTable providerTypes = Connection.GetSchema("DataTypes", null);
        }

        [Test]
		public void ForeignKeys()
		{
			DataTable foreignKeys = Connection.GetSchema("ForeignKeys", null);
		}

        [Test]
        public void ForeignKeyColumns()
        {
            DataTable foreignKeys = Connection.GetSchema("ForeignKeyColumns", null);
        }

        [Test]
		public void Functions()
		{
			DataTable functions = Connection.GetSchema("Functions", null);
		}

		[Test]
		public void Groups()
		{
			DataTable groups = Connection.GetSchema("Groups", null);
		}

		[Test]
		public void Indexes()
		{
			DataTable indexes = Connection.GetSchema("Indexes", null);
		}

        [Test]
        public void IndexColumns()
        {
            DataTable indexes = Connection.GetSchema("Indexes", null);

            foreach (DataRow index in indexes.Rows)
            {
                string catalog      = !index.IsNull("TABLE_CATALOG") ? (string)index["TABLE_CATALOG"] : null;
                string schema       = !index.IsNull("TABLE_SCHEMA") ? (string)index["TABLE_SCHEMA"] : null;
                string tableName    = !index.IsNull("TABLE_NAME") ? (string)index["TABLE_NAME"] : null;
                string indexName    = !index.IsNull("INDEX_NAME") ? (string)index["INDEX_NAME"] : null;

                DataTable indexColumns = Connection.GetSchema("IndexColumns", new string[] { catalog, schema, tableName, indexName });
            }
        }

        [Test]
		public void PrimaryKeys()
		{
			DataTable primaryKeys = Connection.GetSchema("PrimaryKeys", null);
		}


        [Test]
        public void ReservedWords()
        {
            DataTable reservedWords = Connection.GetSchema("ReservedWords", null);
        }

        [Test]
        public void Restrictions()
        {
            DataTable restrictions = Connection.GetSchema("Restrictions", null);
        }

        [Test]
        public void Schemas()
        {
            DataTable schemas = Connection.GetSchema("Schemas");
        }

        [Test]
        public void Sequences()
        {
            DataTable sequences = Connection.GetSchema("Sequences");
        }

        [Test]
        public void SqlLanguages()
        {
            DataTable sqlLanguages = Connection.GetSchema("SqlLanguages");
        }

		[Test]
		public void Tables()
		{
			DataTable tables = Connection.GetSchema("Tables", null);
		}

        [Test]
        public void Triggers()
        {
            DataTable triggers = Connection.GetSchema("Triggers", null);
        }

		[Test]
		public void ViewColumns()
		{
            DataTable viewColumns = Connection.GetSchema("ViewColumns", null);
		}

		[Test]
		public void Views()
		{
			DataTable views = Connection.GetSchema("Views", null);
        }

        #endregion
    }
}
