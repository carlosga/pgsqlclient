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
	public struct PgLine
	{
		#region � Fields �

		private PgPoint startPoint;
		private PgPoint	endPoint;

		#endregion

		#region � Properties �

		public PgPoint StartPoint
		{
			get { return startPoint; }
		}
		
		public PgPoint EndPoint
		{
			get { return endPoint; }
		}

		#endregion

		#region � Constructors �

		public PgLine(PgPoint startPoint, PgPoint endPoint)
		{
			this.startPoint = startPoint;
			this.endPoint	= endPoint;
		}

		public PgLine(double x1, double y1, double x2, double y2)
		{
			this.startPoint	= new PgPoint(x1, y1);
			this.endPoint	= new PgPoint(x2, y2);
		}

		#endregion

		#region � Operators �

		public static bool operator ==(PgLine left, PgLine right)
		{
			if (left.StartPoint == right.StartPoint && 
				left.EndPoint == right.EndPoint)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool operator !=(PgLine left, PgLine right)
		{
			if (left.StartPoint != right.StartPoint ||
				left.EndPoint != right.EndPoint)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region � Overriden Methods �

		public override string ToString()
		{
			System.Text.StringBuilder b = new System.Text.StringBuilder();
			b.AppendFormat("(({0},{1}),({2},{3}))", 
				this.startPoint.X	, this.startPoint.Y,
				this.endPoint.X		, this.endPoint.Y);

			return b.ToString();
		}

		public override int GetHashCode()
		{
			return this.startPoint.GetHashCode() ^ this.endPoint.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj != null && obj is PgLine)
			{
				return ((PgLine)obj) == this;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region � Static Methods �

		public static PgLine Parse(string s)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
