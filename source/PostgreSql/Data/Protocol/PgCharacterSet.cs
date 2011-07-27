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

using System.Text;

namespace PostgreSql.Data.Protocol
{
	internal sealed class PgCharacterSet
	{
		#region · Fields ·

		private string		name;
		private Encoding	encoding;

		#endregion

		#region · Properties ·

		public string Name
		{
			get { return this.name; }
		}

		public Encoding Encoding
		{
			get { return this.encoding; }
		}

		#endregion

		#region · Constructors ·

		public PgCharacterSet()
		{
		}

		public PgCharacterSet(string name, string systemCharSet)
		{
			this.name		= name;			
			this.encoding	= Encoding.GetEncoding(systemCharSet);
		}

		public PgCharacterSet(string name, int cp)
		{
			this.name		= name;			
			this.encoding	= Encoding.GetEncoding(cp);
		}

		#endregion
	}
}
