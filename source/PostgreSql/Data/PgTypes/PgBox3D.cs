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
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PgTypes
{
    [Serializable]
    public struct PgBox3D
    {
        #region · Fields ·

        private PgPoint3D upperRight;
        private PgPoint3D lowerLeft;

        #endregion

        #region · Properties ·

        public PgPoint3D UpperRight
        {
            get { return this.upperRight; }
        }

        public PgPoint3D LowerLeft
        {
            get { return this.lowerLeft; }
        }

        #endregion

        #region · Constructors ·

        public PgBox3D(PgPoint3D lowerLeft, PgPoint3D upperRight)
        {
            this.lowerLeft  = lowerLeft;
            this.upperRight = upperRight;
        }

        public PgBox3D(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            this.lowerLeft  = new PgPoint3D(x1, y1, z1);
            this.upperRight = new PgPoint3D(x2, y2, z2);
        }

        #endregion

        #region · Operators ·

        public static bool operator ==(PgBox3D left, PgBox3D right)
        {
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static bool operator !=(PgBox3D left, PgBox3D right)
        {
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
        }

        #endregion

        #region · Overriden Methods ·

        public override string ToString()
        {
            return String.Format("BOX3D({0},{1})", this.lowerLeft.ToString(), this.upperRight.ToString());
        }

        public override int GetHashCode()
        {
            return (this.UpperRight.GetHashCode() ^ this.LowerLeft.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj is PgBox3D)
            {
                return ((PgBox3D)obj) == this;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region · Static Methods ·

        public static PgBox3D Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s cannot be null");
            }

            if (s.IndexOf("(") > 0)
            {
                s = s.Substring(s.IndexOf("(") + 1, s.IndexOf(")") - s.IndexOf("(") - 1);
            }

            string[] delimiters = new string[] { "," };

            string[] boxPoints = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            PgPoint3D left  = PgPoint3D.Parse(boxPoints[0]);
            PgPoint3D right = PgPoint3D.Parse(boxPoints[1]);

            return new PgBox3D(left, right);
        }

        #endregion
    }
}
