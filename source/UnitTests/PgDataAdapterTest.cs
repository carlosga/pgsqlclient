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
	public class PgDataAdapterTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void FillTest()
		{
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table WHERE date_field = @date_field", Connection);
			PgDataAdapter	adapter = new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@date_field", PgDbType.Date, 4, "date_field").Value = DateTime.Now;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");
			
			Console.WriteLine();
			Console.WriteLine("DataAdapter - Fill Method - Test");

			foreach (DataTable table in ds.Tables)
			{
				foreach (DataColumn col in table.Columns)
				{
					Console.Write(col.ColumnName + "\t\t");
				}
				
				Console.WriteLine();
				
				foreach (DataRow row in table.Rows)
				{
					for (int i = 0; i < table.Columns.Count; i++)
					{
						Console.Write(row[i] + "\t\t");
					}

					Console.WriteLine("");
				}
			}

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void FillMultipleTest()
		{
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table WHERE date_field = @date_field", Connection);
			PgDataAdapter	adapter = new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@date_field", PgDbType.Date, 4, "date_field").Value = DateTime.Now;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds1 = new DataSet();
			DataSet ds2 = new DataSet();
			
			adapter.Fill(ds1, "public.test_table");
			adapter.Fill(ds2, "public.test_table");
			
			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
		}

		[Test]
		public void InsertTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			DataRow newRow = ds.Tables["public.test_table"].NewRow();

			newRow["int4_field"]		= 100;
			newRow["char_field"]		= "PostgreSQL";
			newRow["varchar_field"]		= "PostgreSQL CLient";
			newRow["int8_field"]		= 100000;
			newRow["int2_field"]		= 100;
			newRow["double_field"]		= 100.01;			
			newRow["date_field"]		= DateTime.Now;
			newRow["time_Field"]		= DateTime.Now;
			newRow["timestamp_field"]	= DateTime.Now;

			ds.Tables["public.test_table"].Rows.Add(newRow);

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();
		}

		[Test]
		public void UpdateCharTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 1;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["char_field"] = "PostgreSQL";

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();
		}

		[Test]
		public void UpdateVarCharTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 10;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["varchar_field"]	= "PostgreSQL Client";

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();
		}

		[Test]
		public void UpdateInt2Test()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 40;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["int2_field"] = System.Int16.MaxValue;

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT int2_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 40;

			short val = (short)command.ExecuteScalar();

            Assert.AreEqual(System.Int16.MaxValue, val, "int2_field has not correct value");
		}

		[Test]
		public void UpdateInt8Test()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 20;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["int8_field"]	= System.Int32.MaxValue;

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT int8_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 20;

			long val = (long)command.ExecuteScalar();

            Assert.AreEqual(System.Int32.MaxValue, val, "int8_field has not correct value");
		}

		[Test]
		public void UpdateDoubleTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 50;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["double_field"]	= System.Int32.MaxValue;

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT double_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 50;

			double val = (double)command.ExecuteScalar();

            Assert.AreEqual(System.Int32.MaxValue, val, "double_field has not correct value");
		}

		[Test]
		public void UpdateMoneyField()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 27;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["money_field"] = 200.20;
			
			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT money_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 27;

			float val = (float)command.ExecuteScalar();

            Assert.AreEqual(200.20, val, "money_field has not correct value");
		}

		[Test]
		public void UpdateNumericTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 60;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0]["numeric_field"]	= System.Int16.MaxValue;
			
			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT numeric_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 60;

			decimal val = (decimal)command.ExecuteScalar();
			
			if (val != (decimal)System.Int16.MaxValue)
			{
                Assert.Fail("numeric_field has not correct value");
			}
		}

		[Test]
		public void UpdateDateTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter = new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 70;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			DateTime dt = DateTime.Now;

			ds.Tables["public.test_table"].Rows[0]["date_field"] = dt;

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();			
			transaction.Commit();

			command = new PgCommand("SELECT date_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 70;

			DateTime val = (DateTime)command.ExecuteScalar();

            Assert.AreEqual(dt.Day, val.Day, "date_field has not correct day");
            Assert.AreEqual(dt.Month, val.Month, "date_field has not correct month");
            Assert.AreEqual(dt.Year, val.Year, "date_field has not correct year");
		}

		[Test]
		public void UpdateTimeTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 80;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			DateTime dt = DateTime.Now;

			ds.Tables["public.test_table"].Rows[0]["time_field"] = dt;

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT time_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 80;

			DateTime val = (DateTime)command.ExecuteScalar();

            Assert.AreEqual(dt.Hour, val.Hour, "time_field has not correct hour");
            Assert.AreEqual(dt.Minute, val.Minute, "time_field has not correct minute");
            Assert.AreEqual(dt.Second, val.Second, "time_field has not correct second");
		}

		[Test]
		public void UpdateTimeStampTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command		= new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter		= new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 90;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			DateTime dt = DateTime.Now;

			ds.Tables["public.test_table"].Rows[0]["timestamp_field"] = dt;

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();

			command = new PgCommand("SELECT timestamp_field FROM public.test_table WHERE int4_field = @int4_field", Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 90;

			DateTime val = (DateTime)command.ExecuteScalar();

            Assert.AreEqual(dt.Day, val.Day, "timestamp_field has not correct day");
            Assert.AreEqual(dt.Month, val.Month, "timestamp_field has not correct month");
            Assert.AreEqual(dt.Year, val.Year, "timestamp_field has not correct year");
            Assert.AreEqual(dt.Hour, val.Hour, "timestamp_field has not correct hour");
            Assert.AreEqual(dt.Minute, val.Minute, "timestamp_field has not correct minute");
            Assert.AreEqual(dt.Second, val.Second, "timestamp_field has not correct second");
		}
	
		[Test]
		public void DeleteTest()
		{
			PgTransaction	transaction = Connection.BeginTransaction();
			PgCommand		command = new PgCommand("SELECT * FROM public.test_table WHERE int4_field = @int4_field", Connection, transaction);
			PgDataAdapter	adapter = new PgDataAdapter(command);

			adapter.SelectCommand.Parameters.Add("@int4_field", PgDbType.Int4, 4, "int4_field").Value = 35;
			
			PgCommandBuilder builder = new PgCommandBuilder(adapter);

			DataSet ds = new DataSet();
			adapter.Fill(ds, "public.test_table");

			ds.Tables["public.test_table"].Rows[0].Delete();

			adapter.Update(ds, "public.test_table");

			adapter.Dispose();
			builder.Dispose();
			command.Dispose();
			transaction.Commit();
        }

        #endregion
    }
}