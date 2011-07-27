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
	public class PgDataReaderTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void ReadTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();						
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction);
			
			Console.WriteLine("\r\nDataReader - Read Method - Test");
			
			PgDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader.GetValue(i) + "\t");
				}
			
				Console.WriteLine();
			}

			reader.Close();
			command.Dispose();
			transaction.Rollback();
		}

		[Test]
		public void GetValuesTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();						
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction);
			
			Console.WriteLine("\r\nDataReader - Read Method - Test");
			
			PgDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);

				for(int i = 0; i < values.Length; i++)
				{
					Console.Write(values[i] + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			transaction.Rollback();	
			command.Dispose();
		}

		[Test]
		public void IndexerByIndexTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();						
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction);
			
			Console.WriteLine("\r\nDataReader - Read Method - Test");
			
			PgDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader[i] + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			transaction.Rollback();				
			command.Dispose();
		}

		[Test]
		public void IndexerByNameTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();						
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction);
			
			Console.WriteLine("\r\nDataReader - Read Method - Test");
			
			PgDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				for (int i = 0; i < reader.FieldCount; i++)
				{
					Console.Write(reader[reader.GetName(i)] + "\t");					
				}
			
				Console.WriteLine();
			}

			reader.Close();
			transaction.Rollback();				
			command.Dispose();
		}

		[Test]
		public void GetSchemaTableTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();						
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction);
	
			PgDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);		
		
			DataTable schema = reader.GetSchemaTable();
			
			Console.WriteLine();
			Console.WriteLine("DataReader - GetSchemaTable Method- Test");

			DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			foreach (DataColumn myCol in schema.Columns)
			{
				Console.Write("{0}\t\t", myCol.ColumnName);
			}

			Console.WriteLine();
			
			foreach (DataRow myRow in currRows)
			{
				foreach (DataColumn myCol in schema.Columns)
				{
					Console.Write("{0}\t\t", myRow[myCol]);
				}
				
				Console.WriteLine();
			}
			
			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}
		
		[Test]
		public void GetSchemaTableWithExpressionFieldTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command = new PgCommand("SELECT *, 0 AS VALOR FROM public.test_table", Connection, transaction);

			PgDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
		
			DataTable schema = reader.GetSchemaTable();
			
			Console.WriteLine();
			Console.WriteLine("DataReader - GetSchemaTable Method- Test");

			DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

			foreach (DataColumn myCol in schema.Columns)
			{
				Console.Write("{0}\t\t", myCol.ColumnName);
			}

			Console.WriteLine();
			
			foreach (DataRow myRow in currRows)
			{
				foreach (DataColumn myCol in schema.Columns)
				{
					Console.Write("{0}\t\t", myRow[myCol]);
				}
				
				Console.WriteLine();
			}
			
			reader.Close();
			transaction.Rollback();
			command.Dispose();
		}

        #endregion
    }
}