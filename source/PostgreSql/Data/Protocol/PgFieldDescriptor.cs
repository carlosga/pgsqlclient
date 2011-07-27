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
    internal sealed class PgFieldDescriptor
    {
        #region · Fields ·

        private string			fieldName;
        private int				oidTable;
        private short			oidNumber;		
        private PgType			dataType;
        private short			dataTypeSize;
        private int				typeModifier;
        private PgTypeFormat	formatCode;

        #endregion

        #region · Properties ·

        public string FieldName
        {
            get { return this.fieldName; }
            set { this.fieldName = value; }
        }

        public int OidTable
        {
            get { return this.oidTable; }
            set { this.oidTable = value; }
        }

        public short OidNumber
        {
            get { return this.oidNumber; }
            set { this.oidNumber = value; }
        }
    
        public PgType DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        public short DataTypeSize
        {
            get { return this.dataTypeSize; }
            set { this.dataTypeSize = value; }
        }

        public int TypeModifier
        {
            get { return this.typeModifier; }
            set { this.typeModifier = value; }
        }

        public PgTypeFormat FormatCode
        {
            get { return this.formatCode; }
            set { this.formatCode = value; }
        }

        #endregion

        #region · Constructors ·

        public PgFieldDescriptor()
        {
        }

        #endregion
    }
}
