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
using NUnit.Framework;

using PostgreSql.Data.PostgreSqlClient;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
	[TestFixture]
	public class PgGeometricTypesTest : PgBaseTest
    {
        #region · Unit Tests ·

        [Test]
		public void PointTest()
		{
			PgCommand command = new PgCommand("select point_field from public.geometric_table where pk = @pk", Connection);
			try
			{
				command.Parameters.Add("@pk", PgDbType.Int4).Value = 50;

				PgPoint point = (PgPoint)command.ExecuteScalar();

				Console.WriteLine("Point value: {0}", point.ToString());

                Assert.AreEqual(50, point.X, "Invalid X coord in point");
                Assert.AreEqual(60, point.Y, "Invalid Y coord in point");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Test]
		public void BoxTest()
		{
			PgCommand command = new PgCommand("select box_field from public.geometric_table where pk = @pk", Connection);
			try
			{
				command.Parameters.Add("@pk", PgDbType.Int4).Value = 70;

				PgBox box = (PgBox)command.ExecuteScalar();

				Console.WriteLine("Box value: {0}", box.ToString());

                Assert.AreEqual(0, box.LowerLeft.X, "Invalid X coord in Lower Left corner");
                Assert.AreEqual(70, box.LowerLeft.Y, "Invalid Y coord in Lower Left corner");

                Assert.AreEqual(70, box.UpperRight.X, "Invalid X coord in Upper Right corner");
                Assert.AreEqual(70, box.UpperRight.Y, "Invalid Y coord in Upper Right corner");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Test]
		public void CircleTest()
		{
			PgCommand command = new PgCommand("select circle_field from public.geometric_table where pk = @pk", Connection);
			try
			{
				command.Parameters.Add("@pk", PgDbType.Int4).Value = 30;

				PgCircle circle = (PgCircle)command.ExecuteScalar();

				Console.WriteLine("Circle value: {0}", circle.ToString());

                Assert.AreEqual(30, circle.Center.X, "Invalid X coord in circle");
                Assert.AreEqual(0, circle.Center.Y, "Invalid Y coord in circle");
                Assert.AreEqual(30, circle.Radius, "Invalid RADIUS coord in circle");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Test]
		public void LineSegmentTest()
		{
			PgCommand command = new PgCommand("select lseg_field from public.geometric_table where pk = @pk", Connection);
			try
			{
				command.Parameters.Add("@pk", PgDbType.Int4).Value = 20;

				PgLSeg lseg = (PgLSeg)command.ExecuteScalar();

				Console.WriteLine("LSeg value: {0}", lseg.ToString());

                Assert.AreEqual(-1, lseg.StartPoint.X, "Invalid X coord in start point");
                Assert.AreEqual(0, lseg.StartPoint.Y, "Invalid Y coord in start point");

                Assert.AreEqual(1, lseg.EndPoint.X, "Invalid X coord in end point");
                Assert.AreEqual(0, lseg.EndPoint.Y, "Invalid Y coord in end point");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Test]
		public void PathTest()
		{
			PgCommand command = new PgCommand("select path_field from public.geometric_table where pk = @pk", Connection);
			try
			{
				command.Parameters.Add("@pk", PgDbType.Int4).Value = 10;

				PgPath path = (PgPath)command.ExecuteScalar();

				Console.WriteLine("Path value: {0}", path.ToString());

                Assert.AreEqual(0, path.Points[0].X, "Invalid X coord in path point 0");
                Assert.AreEqual(0, path.Points[0].Y, "Invalid Y coord in path point 0");

                Assert.AreEqual(1, path.Points[1].X, "Invalid X coord in path point 1");
                Assert.AreEqual(0, path.Points[1].Y, "Invalid Y coord in path point 1");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Test]
		public void PolygonTest()
		{
			PgCommand command = new PgCommand("select polygon_field from public.geometric_table where pk = @pk", Connection);
			try
			{
				command.Parameters.Add("@pk", PgDbType.Int4).Value = 10;

				PgPolygon polygon = (PgPolygon)command.ExecuteScalar();

				Console.WriteLine("Polygon value: {0}", polygon.ToString());

                Assert.AreEqual(1, polygon.Points[0].X, "Invalid X coord in polygon point 0");
                Assert.AreEqual(1, polygon.Points[0].Y, "Invalid Y coord in polygon point 0");

                Assert.AreEqual(0, polygon.Points[1].X, "Invalid X coord in polygon point 1");
                Assert.AreEqual(0, polygon.Points[1].Y, "Invalid Y coord in polygon point 1");
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				command.Dispose();
			}
		}

		[Ignore("Test not implemented.")]
		public void BoxArrayTest()
		{
		}

		[Ignore("Test not implemented.")]
		public void PointArrayTest()
		{
		}

		[Ignore("Test not implemented.")]
		public void LineSegmentArrayTest()
		{
		}

		[Ignore("Test not implemented.")]
		public void PathArrayTest()
		{
		}

		[Ignore("Test not implemented.")]
		public void PolygonArrayTest()
		{
		}

		[Ignore("Test not implemented.")]
		public void CircleArrayTest()
		{
        }

        #endregion
    }
}
