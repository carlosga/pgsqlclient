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

namespace PostgreSql.Data.Protocol
{
	internal class PgBackendCodes
	{
		// Backend Message Formats
		public const char AUTHENTICATION		 = 'R';
		public const char BACKEND_KEY_DATA		 = 'K';
		public const char BIND_COMPLETE			 = '2';
		public const char CLOSE_COMPLETE		 = '3';
		public const char COMMAND_COMPLETE		 = 'C';
		public const char COPY_IN_RESPONSE		 = 'G';
		public const char COPY_OUT_RESPONSE		 = 'H';
		public const char DATAROW				 = 'D';
		public const char EMPTY_QUERY_RESPONSE	 = 'I';
		public const char ERROR_RESPONSE		 = 'E';
		public const char FUNCTION_CALL_RESPONSE = 'V';
		public const char NODATA				 = 'n';
		public const char NOTICE_RESPONSE		 = 'N';
		public const char NOTIFICATION_RESPONSE	 = 'A';
		public const char PARAMETER_DESCRIPTION	 = 't';
		public const char PARAMETER_STATUS		 = 'S';
		public const char PARSE_COMPLETE		 = '1';
		public const char PORTAL_SUSPENDED		 = 's';
		public const char READY_FOR_QUERY		 = 'Z';
		public const char ROW_DESCRIPTION		 = 'T';
	}
}
