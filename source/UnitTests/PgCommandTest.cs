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
	public class PgCommandTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void ExecuteNonQueryTest()
		{
			Console.WriteLine("\r\nPgCommandTest.ExecuteNonQueryTest");

			string commandText = "update public.test_table set char_field = @char_field, varchar_field = @varchar_field where int4_field = @int4_field";

			PgTransaction transaction = Connection.BeginTransaction();
			PgCommand command = new PgCommand(commandText, Connection, transaction);

			try
			{
				// Add command parameters
				command.Parameters.Add("@char_field", PgDbType.Char);
				command.Parameters.Add("@varchar_field", PgDbType.VarChar);
				command.Parameters.Add("@int4_field", PgDbType.Int4);

				for (int i = 0; i < 100; i++)
				{
					command.Parameters["@char_field"].Value		= "Row " + i.ToString();
					command.Parameters["@varchar_field"].Value	= "Row Number" + i.ToString();
					command.Parameters["@int4_field"].Value		= i;

					command.ExecuteNonQuery();
				}

				// Commit transaction
				transaction.Commit();

				ExecuteReaderTest();
			}
			catch (PgException)
			{
				transaction.Rollback();
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Test]
		public void ExecuteReaderTest()
		{
			Console.WriteLine("\r\nPgCommandTest.ExecuteReaderTest");

			PgCommand		command = new PgCommand("SELECT * FROM public.test_table ORDER BY date_field", Connection);
			PgDataReader	reader	= command.ExecuteReader();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				Console.Write("{0}\t\t", reader.GetName(i));
			}
			Console.Write("\r\n");

			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);

				for (int i = 0; i < values.Length; i++)
				{
					Console.Write("{0}\t\t", values[i]);
				}
				Console.Write("\r\n");
			}

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void ExecuteScalarTest()
		{
			PgCommand command = Connection.CreateCommand();
			
			command.CommandText = "SELECT char_field FROM public.test_table where int4_field = @int4_field";
									
			command.Parameters.Add("@int4_field", 2);
						
			string charFieldValue = command.ExecuteScalar().ToString();
			
			Console.WriteLine("Scalar value: {0}", charFieldValue);

			command.Dispose();
		}

		[Test]
		public void PrepareTest()
		{							
			PgCommand command = Connection.CreateCommand();
			
			command.CommandText = "SELECT char_field FROM public.test_table where int4_field = @int4_field";
									
			command.Parameters.Add("@int4_field", 2);						
			command.Prepare();
			command.Dispose();
		}

		[Test]
		public void NamedParametersTest()
		{
			PgCommand command = Connection.CreateCommand();
			
			command.CommandText = "SELECT char_field FROM public.test_table where int4_field = @int4_field or char_field = @char_field";
									
			command.Parameters.Add("@int4_field", 2);
			command.Parameters.Add("@char_field", "IRow 20");
						
			PgDataReader reader = command.ExecuteReader();
			
			int count = 0;
			while (reader.Read())
			{
				Console.WriteLine(reader.GetValue(0));
				count++;
			}

			Console.WriteLine("\r\n Record fetched {0} \r\n", count);

			reader.Close();
			command.Dispose();
		}

		[Test]
		public void ExecuteStoredProcTest()
		{
			PgCommand command	= new PgCommand("TestCount", Connection);
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.Add("@CountResult", PgDbType.Int8).Direction = ParameterDirection.Output;

			command.ExecuteNonQuery();

			Console.WriteLine("ExecuteStoredProcTest - Count result {0}", command.Parameters[0].Value);

			command.Dispose();
		}

		[Test]
		public void RecordsAffectedTest()
		{
			// Execute a SELECT command
			PgCommand selectCommand = new PgCommand("SELECT * FROM public.test_table WHERE int4_field = 100", Connection);
			int recordsAffected = selectCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected by SELECT command: {0}", recordsAffected);
			selectCommand.Dispose();
			
			Assert.IsTrue(recordsAffected == -1);

			// Execute a DELETE command
			PgCommand deleteCommand = new PgCommand("DELETE FROM public.test_table WHERE int4_field = 45", Connection);
			recordsAffected = deleteCommand.ExecuteNonQuery();
			Console.WriteLine("\r\nRecords Affected by DELETE command: {0}", recordsAffected);			
			deleteCommand.Dispose();

            Assert.IsTrue(recordsAffected == 1);
		}

		[Test]
		public void TestError782096()
		{
			Console.WriteLine("\r\nPgCommandTest.TestError782096");

			PgCommand		command = new PgCommand("SELECT * FROM public.test_table ORDER BY date_field", Connection);
			PgDataReader	reader	= command.ExecuteReader();

			for (int i = 0; i < reader.FieldCount; i++)
			{
				Console.Write("{0}\t\t", reader.GetName(i));
			}

			Console.Write("\r\n");

			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);

				for (int i = 0; i < values.Length; i++)
				{
					Console.Write("{0}\t\t", values[i]);
				}
				Console.Write("\r\n");
			}

			command.Dispose();
			reader.Close();
        }

        [Test]
        public void TestCase3()
        {
            PgCommand cmd = Connection.CreateCommand();
            cmd.CommandText = @"SELECT int4_field FROM test_table WHERE varchar_field = @parameter";
            cmd.Parameters.Add("@parameter", "IRow Number10");
            cmd.ExecuteScalar();
            cmd.Dispose();
        }

        #endregion
    }
}
