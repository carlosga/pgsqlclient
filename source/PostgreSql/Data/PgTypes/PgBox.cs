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
    public struct PgBox
    {
        #region · Fields ·

        private PgPoint upperRight;
        private PgPoint	lowerLeft;

        #endregion

        #region · Properties ·

        public PgPoint UpperRight
        {
            get { return this.upperRight; }
        }

        public PgPoint LowerLeft
        {
            get { return this.lowerLeft; }
        }

        #endregion

        #region · Constructors ·

        public PgBox(PgPoint lowerLeft, PgPoint upperRight)
        {
            this.lowerLeft	= lowerLeft;
            this.upperRight	= upperRight;
        }

        public PgBox(double x1, double y1, double x2, double y2)
        {
            this.lowerLeft	= new PgPoint(x1, y1);
            this.upperRight	= new PgPoint(x2, y2);
        }

        #endregion

        #region · Operators ·

        public static bool operator ==(PgBox left, PgBox right)
        {
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static bool operator !=(PgBox left, PgBox right)
        {
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
        }

        #endregion

        #region · Overriden Methods ·

        public override string ToString()
        {
            return String.Format("(({0},{1}),({2},{3}))"
                               , this.lowerLeft.X
                               , this.lowerLeft.Y
                               , this.upperRight.X
                               , this.upperRight.Y);
        }

        public override int GetHashCode()
        {
            return (this.UpperRight.GetHashCode() ^ this.LowerLeft.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (obj is PgBox)
            {
                return ((PgBox)obj) == this;
            }

            return false;
        }

        #endregion

        #region · Static Methods ·

        public static PgBox Parse(string s)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
