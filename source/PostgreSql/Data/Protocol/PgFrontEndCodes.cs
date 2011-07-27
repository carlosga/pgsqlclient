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
    internal static class PgFrontEndCodes
    {
        #region · Constants ·

        public const char BIND				= 'B';
        public const char CLOSE				= 'C';		
        public const char COPY_FAIL			= 'f';
        public const char DESCRIBE			= 'D';
        public const char EXECUTE			= 'E';		
        public const char FLUSH				= 'H';
        public const char FUNCTION_CALL		= 'F';
        public const char PARSE				= 'P';
        public const char PASSWORD_MESSAGE  = 'p';
        public const char QUERY				= 'Q';
        public const char SYNC				= 'S';
        public const char TERMINATE			= 'X';

        #endregion
    }
}
