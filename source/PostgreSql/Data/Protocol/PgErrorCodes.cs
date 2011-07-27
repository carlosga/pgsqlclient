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

namespace PostgreSql.Data.Protocol
{
    internal static class PgErrorCodes
    {
        #region · Constants ·

        public const char SEVERITY  = 'S';
        public const char CODE      = 'C';
        public const char MESSAGE   = 'M';
        public const char DETAIL    = 'D';
        public const char HINT      = 'H';
        public const char POSITION  = 'P';
        public const char WHERE     = 'W';
        public const char FILE      = 'F';
        public const char LINE      = 'L';
        public const char ROUTINE   = 'R';		
        public const char END       = '\0';

        #endregion
    }
}
