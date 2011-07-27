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
	// This is a exact copy of PgDbType enum for
	// allow a better and more simple handling of
	// data types in the protocol implementation.
	internal enum PgDataType
	{
		Array			,
		Binary			,
		Boolean			,
		Box				,
        Box2D           ,
        Box3D           ,
		Byte			,
		Char			,
		Circle			,
		Currency		,
		Date			,
		Decimal			,
		Double			,
		Float			,
		Int2			,
		Int4			,
		Int8			,
		Interval		,
		Line			,
		LSeg			,
		Numeric			,
		Path			,
		Point			,
		Polygon			,
        Refcursor       ,
		Text			,
		Time			,
		TimeWithTZ		,
		Timestamp		,
		TimestampWithTZ	,
		VarChar			,
		Vector
	}
}
