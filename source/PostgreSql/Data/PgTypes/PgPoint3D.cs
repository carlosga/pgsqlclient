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
    public struct PgPoint3D
    {
        #region · Fields ·

        private double x;
        private double y;
        private double z;

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

        public double Z
        {
            get { return this.z; }
        }

        #endregion

        #region · Constructors ·

        public PgPoint3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        #endregion

        #region · Operators ·

        public static bool operator ==(PgPoint3D left, PgPoint3D right)
        {
            return (left.X == right.X && left.Y == right.Y && left.Z == right.Z);
        }

        public static bool operator !=(PgPoint3D left, PgPoint3D right)
        {
            return (left.X != right.X || left.Y != right.Y || left.Z != right.Z);
        }

        #endregion

        #region · Overriden Methods ·

        public override string ToString()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            return String.Format(culture, "{0} {1} {2}", this.x, this.y, this.z);
        }

        public override int GetHashCode()
        {
            return (this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is PgPoint3D)
            {
                return ((PgPoint3D)obj) == this;
            }

            return false;
        }

        #endregion

        #region · Static Methods ·

        public static PgPoint3D Parse(string s)
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

            if (pointCoords == null || pointCoords.Length != 3)
            {
                throw new ArgumentException("s is not a valid point.");
            }

            double x = Double.Parse(pointCoords[0], System.Globalization.CultureInfo.InvariantCulture);
            double y = Double.Parse(pointCoords[1], System.Globalization.CultureInfo.InvariantCulture);
            double z = Double.Parse(pointCoords[2], System.Globalization.CultureInfo.InvariantCulture);

            return new PgPoint3D(x, y, z);
        }

        #endregion
    }
}