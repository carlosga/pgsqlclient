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
	public struct PgCircle
	{
		#region · Fields ·

		private PgPoint center;
		private double	radius;

		#endregion

		#region · Properties ·

		public PgPoint Center
		{
			get { return center; }
		}
		
		public double Radius
		{
			get { return radius; }
		}

		#endregion

		#region · Constructors ·

		public PgCircle(PgPoint center, double radius)
		{
			this.center = center;
			this.radius = radius;
		}

		public PgCircle(double x, double y, double radius)
		{
			this.center = new PgPoint(x, y);
			this.radius	= radius;
		}

		#endregion

		#region · Operators ·

		public static bool operator ==(PgCircle left, PgCircle right)
		{
			if (left.Center == right.Center && left.Radius == right.Radius)
			{
				return true;
			}
			else
			{
				return true;
			}
		}

		public static bool operator !=(PgCircle left, PgCircle right)
		{
			if (left.Center != right.Center || left.Radius != right.Radius)
			{
				return true;
			}
			else
			{
				return true;
			}
		}

		#endregion

		#region · Overriden Methods ·

		public override string ToString()
		{
			return String.Format("<{0},{1}>", this.center, this.radius);
		}

		public override int GetHashCode()
		{
			return this.center.GetHashCode() ^ this.radius.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is PgCircle)
			{
				return ((PgCircle)obj) == this;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region · Static Methods ·

		public static PgCircle Parse(string s)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
