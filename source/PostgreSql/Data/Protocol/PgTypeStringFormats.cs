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
	internal static class PgTypeStringFormats
	{
		#region · Properties ·

		public static readonly string[] TimeFormats = new string[]
		{
			"HH:mm:ss",
			"HH:mm:ss.f",
			"HH:mm:ss.ff",
			"HH:mm:ss.fff",
			"HH:mm:ss.ffff",
			"HH:mm:ss.fffff",
			"HH:mm:ss.ffffff",
			"HH:mm:sszz",
			"HH:mm:ss.fzz",
			"HH:mm:ss.ffzz",
			"HH:mm:ss.fffzz",
			"HH:mm:ss.ffffzz",
			"HH:mm:ss.fffffzz",
			"HH:mm:ss.ffffffzz",
			"HH:mm:sszz",
			"HH:mm:ss.f zz",
			"HH:mm:ss.ff zz",
			"HH:mm:ss.fff zz",
			"HH:mm:ss.ffff zz",
			"HH:mm:ss.fffff zz",
			"HH:mm:ss.ffffff zz",
		};

		public static readonly string[] DateTimeFormats = new string[]
		{
			"yyyy-MM-dd HH:mm:ss",
			"yyyy-MM-dd HH:mm:ss.f",
			"yyyy-MM-dd HH:mm:ss.ff",
			"yyyy-MM-dd HH:mm:ss.fff",
			"yyyy-MM-dd HH:mm:ss.ffff",
			"yyyy-MM-dd HH:mm:ss.fffff",
			"yyyy-MM-dd HH:mm:ss.ffffff",
			"yyyy-MM-dd HH:mm:sszz",
			"yyyy-MM-dd HH:mm:ss.fzz",
			"yyyy-MM-dd HH:mm:ss.ffzz",
			"yyyy-MM-dd HH:mm:ss.fffzz",
			"yyyy-MM-dd HH:mm:ss.ffffzz",
			"yyyy-MM-dd HH:mm:ss.fffffzz",
			"yyyy-MM-dd HH:mm:ss.ffffffzz",
			"yyyy-MM-dd HH:mm:ss zz",
			"yyyy-MM-dd HH:mm:ss.f zz",
			"yyyy-MM-dd HH:mm:ss.ff zz",
			"yyyy-MM-dd HH:mm:ss.fff zz",
			"yyyy-MM-dd HH:mm:ss.ffff zz",
			"yyyy-MM-dd HH:mm:ss.fffff zz",
			"yyyy-MM-dd HH:mm:ss.ffffff zz",
		};

		#endregion
	}
}
