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

namespace PostgreSql.Data.PostgreSqlClient
{
	[Serializable]
	public sealed class PgError
	{
		#region · Fields ·

		private string	severity;
		private string	code;
		private string	message;			
		private string	detail;
		private string	hint;
		private string	where;
		private string	position;
		private string	file;
		private int		line;
		private string	routine;
		
		#endregion

		#region · Properties ·

		public string Severity
		{
			get { return this.severity; }
			set { this.severity = value; }
		}
		
		public string Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

		public string Code
		{
			get { return this.code; }
			set { this.code = value; }
		}

		public string Detail
		{
			get { return this.detail; }
			set { this.detail = value; }
		}

		public string Hint
		{
			get { return this.hint; }
			set { this.hint = value; }
		}

		public string Where
		{
			get { return this.where; }
			set { this.where = value; }
		}

		public string Position
		{
			get { return this.position; }
			set { this.position = value; }
		}

		public string File
		{
			get { return this.file; }
			set { this.file = value; }
		}

		public int Line
		{
			get { return this.line; }
			set { this.line = value; }
		}

		public string Routine
		{
			get { return this.routine; }
			set { this.routine = value; }
		}

		#endregion

		#region · Constructors ·
		
		internal PgError()
		{
		}

		internal PgError(string message)
		{						
			this.message	= message;
		}

		internal PgError(string severity, string code, string message)
		{
			this.severity	= severity;
			this.code		= code;
			this.message	= message;			
		}

		#endregion
	}
}
