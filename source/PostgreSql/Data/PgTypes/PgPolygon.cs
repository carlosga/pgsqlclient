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

namespace PostgreSql.Data.PgTypes
{	
	[Serializable]
	public struct PgPolygon
	{
		#region · Fields ·

		private PgPoint[] points;

		#endregion

		#region · Properties ·

		public PgPoint[] Points
		{
			get { return points; }
		}

		#endregion

		#region · Constructors ·

		public PgPolygon(PgPoint[] points)
		{
			this.points	= (PgPoint[])points.Clone();
		}

		#endregion

		#region · Operators ·

		public static bool operator ==(PgPolygon left, PgPolygon right)
		{
			bool equals = false;

			if (left.Points.Length == right.Points.Length)
			{
				equals = true;
				for (int i = 0; i < left.Points.Length; i++)
				{
					if (left.Points[i] != right.Points[i])
					{
						equals = false;
						break;
					}
				}
			}

			return equals;
		}

		public static bool operator !=(PgPolygon left, PgPolygon right)
		{
			bool notequals = true;

			if (left.Points.Length == right.Points.Length)
			{
				notequals = false;
				for (int i = 0; i < left.Points.Length; i++)
				{
					if (left.Points[i] != right.Points[i])
					{
						notequals = true;
						break;
					}
				}
			}

			return notequals;
		}

		#endregion

		#region · Overriden Methods ·

		public override string ToString()
		{
			System.Text.StringBuilder b = new System.Text.StringBuilder();

			b.Append("(");

			for (int i = 0; i < this.points.Length; i++)
			{
				if (b.Length > 1)
				{
					b.Append(",");
				}
				b.Append(this.points[i].ToString());
			}

			b.Append(")");

			return b.ToString();
		}

		public override int GetHashCode()
		{
			return (this.points.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is PgPolygon)
			{
				return ((PgPolygon)obj) == this;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region · Static Methods ·

		public static PgPolygon Parse(string s)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
