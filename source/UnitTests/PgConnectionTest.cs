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
	public class PgConnectionTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void BeginTransactionTest()
		{
			PgTransaction transaction = Connection.BeginTransaction();
			transaction.Rollback();
		}

		[Test]
		public void BeginTransactionReadCommittedTest()
		{
			PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
			transaction.Rollback();
		}

		[Test]
		public void BeginTransactionSerializableTest()
		{
			PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
			transaction.Rollback();
		}

		[Test]
		public void DatabaseTest()
		{
			Console.WriteLine("Actual database : {0}", Connection.Database);
		}

		[Test]
		public void DataSourceTest()
		{
			Console.WriteLine("Actual server : {0}", Connection.DataSource);
		}

		[Test]
		public void ConnectionTimeOutTest()
		{
			Console.WriteLine("Actual connection timeout : {0}", Connection.ConnectionTimeout);
		}

		[Test]
		public void ServerVersionTest()
		{
			Console.WriteLine("PostgreSQL Server version : {0}", Connection.ServerVersion);
		}

		[Test]
		public void PacketSizeTest()
		{
			Console.WriteLine("Actual opacket size : {0}", Connection.PacketSize);
		}

		[Test]
		public void CreateCommandTest()
		{
			PgCommand command = Connection.CreateCommand();
        }

        #endregion
    }
}