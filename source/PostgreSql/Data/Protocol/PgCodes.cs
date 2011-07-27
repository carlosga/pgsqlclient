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
	internal static class PgCodes
	{
		// Protocol version 3.0
		public const int PROTOCOL_VERSION2_MAYOR	= 2;
		public const int PROTOCOL_VERSION2_MINOR	= 0;
		public const int PROTOCOL_VERSION2			= (PROTOCOL_VERSION2_MAYOR << 16) | PROTOCOL_VERSION2_MINOR;

		// Protocol version 3.0
		public const int PROTOCOL_VERSION3_MAYOR	= 3;
		public const int PROTOCOL_VERSION3_MINOR	= 0;
		public const int PROTOCOL_VERSION3			= (PROTOCOL_VERSION3_MAYOR << 16) | PROTOCOL_VERSION3_MINOR;

		// SSL Request code
		public const int SSL_REQUEST_MOST			= 1234;
		public const int SSL_REQUEST_LEAST			= 5679;
		public const int SSL_REQUEST				= (SSL_REQUEST_MOST << 16) | SSL_REQUEST_LEAST;

		// Cancel request code
		public const int CANCEL_REQUEST_MOST		= 1234;
		public const int CANCEL_REQUEST_LEAST		= 5678;
		public const int CANCEL_REQUEST				= (CANCEL_REQUEST_MOST << 16) | CANCEL_REQUEST_LEAST;

		// Backend & FrontEnd Message Formats
		public const int COPY_DATA					= 'd';
		public const int COPY_DONE					= 'c';		

		// Authentication values
		public const int AUTH_OK					= 0;
		public const int AUTH_KERBEROS_V4			= 1;
		public const int AUTH_KERBEROS_V5			= 2;
		public const int AUTH_CLEARTEXT_PASSWORD	= 3;
		public const int AUTH_CRYPT_PASSWORD		= 4;
		public const int AUTH_MD5_PASSWORD			= 5;
		public const int AUTH_SCM_CREDENTIAL		= 6;
			
		// Max keys for vector data type
		public const int INDEX_MAX_KEYS			= 32;

		//
		public const char NULL_TERMINATOR		= '\0';

		//
		public static readonly DateTime BASE_DATE = new DateTime(2000, 1, 1);

		// MD5 prefix
		public static string MD5_PREFIX			= "md5";

		// Format codes
		public const short TEXT_FORMAT			= 0;
		public const short BINARY_FORMAT		= 1;

		// Date & Time codes
		public const string DATE_STYLE			= "ISO";

		// Numeric data type
		public const int NUMERIC_SIGN_MASK		= 0xC000;
		public const int NUMERIC_POS            = 0x0000;
		public const int NUMERIC_NEG			= 0x4000;		
		public const int NUMERIC_NAN            = 0xC000;
		public const int NUMERIC_MAX_PRECISION	= 1000;
		public const int NUMERIC_DSCALE_MASK	= 0x3FFF;
		public const int NUMERIC_HDRSZ			= 10;
	}
}
