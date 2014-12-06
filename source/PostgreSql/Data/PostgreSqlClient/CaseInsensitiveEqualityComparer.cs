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
using System.Collections.Generic;
using System.Globalization;

namespace PostgreSql.Data.PostgreSqlClient
{
    internal sealed class CaseInsensitiveEqualityComparer
        : EqualityComparer<string>
    {
        #region · Overriden Methods ·

        public override bool Equals(string x, string y)
        {
            return x.CaseInsensitiveCompare(y);
        }

        public override int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }

        #endregion
    }
}
