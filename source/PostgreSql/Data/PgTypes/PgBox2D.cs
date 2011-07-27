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

namespace PostgreSql.Data.PgTypes
{
    [Serializable]
    public struct PgBox2D
    {
        #region · Fields ·

        private PgPoint2D upperRight;
        private PgPoint2D lowerLeft;

        #endregion

        #region · Properties ·

        public PgPoint2D UpperRight
        {
            get { return this.upperRight; }
        }

        public PgPoint2D LowerLeft
        {
            get { return this.lowerLeft; }
        }

        #endregion

        #region · Constructors ·

        public PgBox2D(PgPoint2D lowerLeft, PgPoint2D upperRight)
        {
            this.lowerLeft = lowerLeft;
            this.upperRight = upperRight;
        }

        public PgBox2D(double x1, double y1, double x2, double y2)
        {
            this.lowerLeft  = new PgPoint2D(x1, y1);
            this.upperRight = new PgPoint2D(x2, y2);
        }

        #endregion

        #region · Operators ·

        public static bool operator ==(PgBox2D left, PgBox2D right)
        {
            if (left.UpperRight == right.UpperRight &&
                left.LowerLeft == right.LowerLeft)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(PgBox2D left, PgBox2D right)
        {
            if (left.UpperRight != right.UpperRight ||
                left.LowerLeft != right.LowerLeft)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region · Overriden Methods ·

        public override string ToString()
        {
            return String.Format("BOX({0},{1})", this.lowerLeft.ToString(), this.upperRight.ToString());
        }

        public override int GetHashCode()
        {
            return (this.UpperRight.GetHashCode() ^ this.LowerLeft.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj is PgBox2D)
            {
                return ((PgBox2D)obj) == this;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region · Static Methods ·

        public static PgBox2D Parse(string s)
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

            PgPoint2D left  = PgPoint2D.Parse(boxPoints[0]);
            PgPoint2D right = PgPoint2D.Parse(boxPoints[1]);

            return new PgBox2D(left, right);
        }

        #endregion
    }
}
