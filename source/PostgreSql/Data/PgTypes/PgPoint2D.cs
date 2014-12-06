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
 *  Copyright (c) 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Globalization;

namespace PostgreSql.Data.PgTypes
{
    [Serializable]
    public struct PgPoint2D
    {
        #region · Fields ·

        private double x;
        private double y;

        #endregion

        #region · Properties ·

        public double X
        {
            get { return this.x; }
        }
        
        public double Y
        {
            get { return this.y; }
        }

        #endregion

        #region · Constructors ·

        public PgPoint2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        #endregion

        #region · Operators ·

        public static bool operator ==(PgPoint2D left, PgPoint2D right)
        {
            return (left.X == right.X && left.Y == right.Y);
        }

        public static bool operator !=(PgPoint2D left, PgPoint2D right)
        {
            return (left.X != right.X || left.Y != right.Y);
        }

        #endregion

        #region · Overriden Methods ·

        public override string ToString()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            return String.Format(culture, "{0} {1}", this.x, this.y);
        }

        public override int GetHashCode()
        {
            return (this.x.GetHashCode() ^ this.y.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is PgPoint2D)
            {
                return ((PgPoint2D)obj) == this;
            }

            return false;
        }

        #endregion

        #region · Static Methods ·

        public static PgPoint2D Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s cannot be null");
            }

            if (s.IndexOf("(") > 0)
            {
                s = s.Substring(s.IndexOf("("), s.Length - s.IndexOf("("));
            }

            string[] pointCoords = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (pointCoords == null || pointCoords.Length != 2)
            {
                throw new ArgumentException("s is not a valid point.");
            }

            double x = Double.Parse(pointCoords[0], System.Globalization.CultureInfo.InvariantCulture);
            double y = Double.Parse(pointCoords[1], System.Globalization.CultureInfo.InvariantCulture);

            return new PgPoint2D(x, y);
        }

        #endregion
    }
}