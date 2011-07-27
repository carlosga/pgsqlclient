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
using System.Security.Cryptography;

using PostgreSql.Data.PostgreSqlClient;
using NUnit.Framework;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
	[TestFixture]
	public class PgArrayTest : PgBaseTest
    {
        #region · Fields ·

        private int testArrayLength = 100;

        #endregion

        #region · Unit Tests ·

        [Test]
		public void Int2ArrayTest()
		{
			int id_value = System.DateTime.Now.Millisecond;

			string selectText = "SELECT int2_array FROM public.test_table WHERE int4_field = " + id_value.ToString();
			string insertText = "INSERT INTO public.test_table (int4_field, int2_array) values (@int4_field, @int2_array)";

			byte[] bytes = new byte[this.testArrayLength*2];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(bytes);

			short[] insert_values = new short[this.testArrayLength];
			Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

			Console.WriteLine("Executing insert command");

			PgCommand command = new PgCommand(insertText, Connection);
			command.Parameters.Add("@int4_field", PgDbType.Int4).Value = id_value;
			command.Parameters.Add("@int2_array", PgDbType.Array).Value = insert_values;
			
			int updated = command.ExecuteNonQuery();

            Assert.AreEqual(1, updated, "Invalid number of inserted rows");

			Console.WriteLine("Checking inserted values");

			// Check that inserted values are correct
			PgCommand select = new PgCommand(selectText, Connection);
			PgDataReader reader = select.ExecuteReader();
			if (reader.Read())
			{
				if (!reader.IsDBNull(0))
				{
					short[] select_values = new short[insert_values.Length];
					System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);
										
					for (int i = 0; i < insert_values.Length; i++)
					{
						if (insert_values[i] != select_values[i])
						{
							throw new Exception("differences at index " + i.ToString());
						}
					}
				}
			}

			Console.WriteLine("Finishing test");
			reader.Close();
        }

        #endregion
    }
}
