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
	public class PgCommandBuilderTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void GetInsertCommandTest()
		{
			PgCommand command = new PgCommand("select * from public.test_table where int4_field = @int4_field and varchar_field = @varchar_field", Connection);
			PgDataAdapter adapter = new PgDataAdapter(command);
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("\r\nPgCommandBuilder - GetInsertCommand Method Test");
			
			Console.WriteLine(builder.GetInsertCommand().CommandText);

			builder.Dispose();
			adapter.Dispose();
			command.Dispose();
		}
		
		[Test]
		public void GetUpdateCommandTest()
		{
			PgCommand command = new PgCommand("select * from public.test_table where int4_field = @int4_field and varchar_field = @varchar_field", Connection);
			PgDataAdapter adapter = new PgDataAdapter(command);
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("\r\nPgCommandBuilder - GetUpdateCommand Method Test");
			
			Console.WriteLine(builder.GetUpdateCommand().CommandText);

			builder.Dispose();
			adapter.Dispose();
			command.Dispose();
		}
		
		[Test]
		public void GetDeleteCommandTest()
		{
			PgCommand command = new PgCommand("select * from public.test_table where int4_field = @int4_field and varchar_field = @varchar_field", Connection);
			PgDataAdapter adapter = new PgDataAdapter(command);
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("PgCommandBuilder - GetDeleteCommand Method Test");
			
			Console.WriteLine( builder.GetDeleteCommand().CommandText );

			builder.Dispose();
			adapter.Dispose();
			command.Dispose();
		}
		
		[Test]
		public void RefreshSchemaTest()
		{
			PgCommand command = new PgCommand("select * from public.test_table where int4_field = @int4_field and varchar_field = @varchar_field", Connection);
			PgDataAdapter adapter = new PgDataAdapter(command);
			PgCommandBuilder builder = new PgCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("\r\nPgCommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine(builder.GetInsertCommand().CommandText);
			Console.WriteLine(builder.GetUpdateCommand().CommandText);
			Console.WriteLine(builder.GetDeleteCommand().CommandText);
			
			adapter.SelectCommand.CommandText = "select int4_field, date_field from public.test_table where int4_field = @int4_field";
			
			builder.RefreshSchema();
			
			Console.WriteLine();
			Console.WriteLine("\r\nPgCommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine(builder.GetInsertCommand().CommandText);
			Console.WriteLine(builder.GetUpdateCommand().CommandText);
			Console.WriteLine(builder.GetDeleteCommand().CommandText);

			builder.Dispose();
			adapter.Dispose();
			command.Dispose();
		}
		
		[Test]
		public void CommandBuilderWithExpressionFieldTest()
		{
			PgCommand command = new PgCommand("select public.test_table.*, 0 AS EXPR_VALUE from public.test_table where int4_field = @int4_field and varchar_field = @varchar_field", Connection);
			PgDataAdapter adapter = new PgDataAdapter(command);
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			Console.WriteLine();
			Console.WriteLine("PgCommandBuilder - CommandBuilderWithExpressionFieldTest");
			
			Console.WriteLine(builder.GetUpdateCommand().CommandText);

			builder.Dispose();
			adapter.Dispose();
			command.Dispose();
		}

		[Test]
		public void DeriveParameters()
		{
			PgCommandBuilder builder = new PgCommandBuilder();
			
			PgCommand command = new PgCommand("DeriveCount", Connection);
			
			command.CommandType = CommandType.StoredProcedure;
						
			PgCommandBuilder.DeriveParameters(command);
			
			Console.WriteLine("\r\nPgCommandBuilder - DeriveParameters static Method Test");
			
			for (int i = 0; i < command.Parameters.Count; i++)
			{
				Console.WriteLine("Parameter name: {0}\tParameter Source Column:{1}\tDirection:{2}",
					command.Parameters[i].ParameterName,
					command.Parameters[i].SourceColumn,
					command.Parameters[i].Direction);
			}
		}

		[Test]
		public void DeriveParameters2()
		{
			PgTransaction transaction = Connection.BeginTransaction();

			PgCommandBuilder builder = new PgCommandBuilder();
			
			PgCommand command = new PgCommand("DeriveCount", Connection, transaction);
			
			command.CommandType = CommandType.StoredProcedure;
						
			PgCommandBuilder.DeriveParameters(command);
			
			Console.WriteLine("\r\nPgCommandBuilder - DeriveParameters static Method Test");
			
			for (int i = 0; i < command.Parameters.Count; i++)
			{
				Console.WriteLine("Parameter name: {0}\tParameter Source Column:{1}\tDirection:{2}",
					command.Parameters[i].ParameterName,
					command.Parameters[i].SourceColumn,
					command.Parameters[i].Direction);
			}

			transaction.Commit();
		}

		[Test]
		public void TestWithClosedConnection()
		{
			Connection.Close();

			PgCommand command = new PgCommand("select * from public.test_table where int4_field = @int4_field and varchar_field = @varchar_field", Connection);
			PgDataAdapter adapter = new PgDataAdapter(command);
			PgCommandBuilder builder = new PgCommandBuilder(adapter);
			
			Console.WriteLine();
			Console.WriteLine("\r\nPgCommandBuilder - RefreshSchema Method Test - Commands for original SQL statement: ");

			Console.WriteLine(builder.GetInsertCommand().CommandText);
			Console.WriteLine(builder.GetUpdateCommand().CommandText);
			Console.WriteLine(builder.GetDeleteCommand().CommandText);
			
			adapter.SelectCommand.CommandText = "select int4_field, date_field from public.test_table where int4_field = @int4_field";
			
			builder.RefreshSchema();
			
			Console.WriteLine();
			Console.WriteLine("\r\nPgCommandBuilder - RefreshSchema Method Test - Commands for new SQL statement: ");
						
			Console.WriteLine(builder.GetInsertCommand().CommandText);
			Console.WriteLine(builder.GetUpdateCommand().CommandText);
			Console.WriteLine(builder.GetDeleteCommand().CommandText);

			builder.Dispose();
			adapter.Dispose();
			command.Dispose();
        }

        #endregion
    }
}
