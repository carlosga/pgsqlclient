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
    public struct PgTimeSpan : IComparable
    {
        #region · Static Members ·

        public static readonly PgTimeSpan MaxValue = new PgTimeSpan(TimeSpan.MaxValue);
        public static readonly PgTimeSpan MinValue = new PgTimeSpan(TimeSpan.MinValue);
        public static readonly PgTimeSpan Null     = new PgTimeSpan(TimeSpan.Zero);

        #endregion

        #region · Fields ·

        private TimeSpan interval;

        #endregion

        #region · Properties ·

        public int Days
        {
            get { return this.interval.Days; }
        }

        public int Hours
        {
            get { return this.interval.Hours; }
        }

        public int Milliseconds
        {
            get { return this.interval.Milliseconds; }
        }

        public int Minutes
        {
            get { return this.interval.Minutes; }
        }

        public int Seconds
        {
            get { return this.interval.Seconds; }
        }

        public TimeSpan Value
        {
            get { return interval; }
        }

        #endregion

        #region · Constructors ·

        public PgTimeSpan(TimeSpan interval)
        {
            this.interval = interval;
        }

        #endregion

        #region · IComparable methods ·

        public int CompareTo(object obj)
        {
            return interval.CompareTo(obj);
        }

        #endregion

        #region · Static Methods ·

        public static bool GreatherThan(PgTimeSpan x, PgTimeSpan y)
        {
            return (x > y);
        }

        public static bool GreatherThanOrEqual(PgTimeSpan x, PgTimeSpan y)
        {
            return (x >= y);
        }

        public static bool LessThan(PgTimeSpan x, PgTimeSpan y)
        {
            return (x < y);
        }

        public static bool LessThanOrEqual(PgTimeSpan x, PgTimeSpan y)
        {
            return (x <= y);
        }

        public static bool NotEquals(PgTimeSpan x, PgTimeSpan y)
        {
            return (x != y);
        }

        public static PgTimeSpan Parse(string s)
        {
            return new PgTimeSpan(TimeSpan.Parse(s));
        }

        #endregion

        #region · Operators ·

        public static bool operator ==(PgTimeSpan left, PgTimeSpan right)
        {
            bool equals = false;

            if (left.Value == right.Value)
            {
                equals = true;
            }

            return equals;
        }

        public static bool operator !=(PgTimeSpan left, PgTimeSpan right)
        {
            bool notequals = false;

            if (left.Value != right.Value)
            {
                notequals = true;
            }

            return notequals;
        }

        public static bool operator >(PgTimeSpan left, PgTimeSpan right)
        {
            bool greater = false;

            if (left.Value > right.Value)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator >=(PgTimeSpan left, PgTimeSpan right)
        {
            bool greater = false;

            if (left.Value >= right.Value)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator <(PgTimeSpan left, PgTimeSpan right)
        {
            bool less = false;

            if (left.Value < right.Value)
            {
                less = true;
            }

            return less;
        }

        public static bool operator <=(PgTimeSpan left, PgTimeSpan right)
        {
            bool less = false;

            if (left.Value <= right.Value)
            {
                less = true;
            }

            return less;
        }

        public static explicit operator TimeSpan(PgTimeSpan x)
        {
            return x.Value;
        }

        public static explicit operator PgTimeSpan(string x)
        {
            return new PgTimeSpan(TimeSpan.Parse(x));
        }

        #endregion

        #region · Overriden Methods ·

        public override string ToString()
        {
            return interval.ToString();
        }

        public override int GetHashCode()
        {
            return interval.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is PgTimeSpan)
            {
                return ((PgTimeSpan)obj) == this;
            }

            return false;
        }

        #endregion
    }
}
