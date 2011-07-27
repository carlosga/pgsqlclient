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
	public class PgTransactionTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void BeginTransactionTest()
		{
			Console.WriteLine("\r\nStarting transaction");
			PgTransaction transaction = Connection.BeginTransaction();
			transaction.Rollback();
		}

		[Test]
		public void BeginTransactionReadCommittedTest()
		{
			Console.WriteLine("\r\nStarting transaction - ReadCommitted");
			PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
			transaction.Rollback();
		}

		[Test]
		public void BeginTransactionSerializableTest()
		{
			Console.WriteLine("\r\nStarting transaction - Serializable");
			PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
			transaction.Rollback();
		}

		[Test]
		public void CommitTest()
		{
			Console.WriteLine("\r\nTestin transaction Commit");
			PgTransaction transaction = Connection.BeginTransaction();
			transaction.Commit();
			transaction.Dispose();
		}
		
		[Test]
		public void RollbackTest()
		{
			Console.WriteLine("\r\nTestin transaction Rollback");
			PgTransaction transaction = Connection.BeginTransaction();
			transaction.Rollback();
			transaction.Dispose();
        }

        #endregion
    }
}