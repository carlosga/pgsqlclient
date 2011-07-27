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
using System.Data;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgType
    {
        #region · Fields ·

        private int				oid;
        private string			name;
        private PgDataType		dataType;
        private Type			systemType;
        private int				elementType;
        private PgTypeFormat	formatCode;
        private int				size;
        private string          delimiter;
        private string          prefix;

        #endregion

        #region · Properties ·

        public int Oid
        {
            get { return this.oid; }
        }

        public PgDataType DataType
        {
            get { return this.dataType; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public Type SystemType
        {
            get { return this.systemType; }
        }
        
        public int ElementType
        {
            get { return this.elementType; }
        }

        public PgTypeFormat FormatCode
        {
            get { return this.formatCode; }
        }

        public int Size
        {
            get { return this.size; }
        }

        public bool IsNumeric
        {
            get
            {
                bool returnValue = false;

                if (this.dataType == PgDataType.Currency ||
                    this.dataType == PgDataType.Int2 ||
                    this.dataType == PgDataType.Int4 ||
                    this.dataType == PgDataType.Int8 ||
                    this.dataType == PgDataType.Float ||
                    this.dataType == PgDataType.Double ||
                    this.dataType == PgDataType.Decimal ||
                    this.dataType == PgDataType.Byte)
                {
                    returnValue = true;
                }

                return returnValue;
            }
        }

        public bool IsArray
        {
            get
            {
                bool returnValue = false;

                if (this.dataType == PgDataType.Array)
                {
                    returnValue = true;
                }

                return returnValue;
            }
        }

        public bool IsLong
        {
            get
            {
                bool returnValue = false;

                if (this.dataType == PgDataType.Binary)
                {
                    returnValue = true;
                }

                return returnValue;
            }
        }

        public bool IsRefCursor
        {
            get { return (this.DataType == PgDataType.Refcursor); }
        }

        public string Delimiter
        {
            get { return this.delimiter; }
        }

        public string Prefix
        {
            get { return this.prefix; }
        }

        #endregion

        #region · Constructors ·

        public PgType(int oid, string name, PgDataType dataType, int elementType, PgTypeFormat formatCode, int size)
            : this(oid, name, dataType, elementType, formatCode, size, "")
        {
        }

        public PgType(
            int             oid, 
            string          name, 
            PgDataType      dataType, 
            int             elementType, 
            PgTypeFormat    formatCode, 
            int             size, 
            string          delimiter)
            : this(oid, name, dataType, elementType, formatCode, size, delimiter, "")
        {
        }

        public PgType(
            int             oid,
            string          name,
            PgDataType      dataType,
            int             elementType,
            PgTypeFormat    formatCode,
            int             size,
            string          delimiter,
            string          prefix)
        {
            this.oid            = oid;
            this.name           = name;
            this.dataType       = dataType;
            this.elementType    = elementType;
            this.formatCode     = formatCode;
            this.size           = size;
            this.systemType     = this.InferSystemType();
            this.delimiter      = delimiter;
            this.prefix         = prefix;
        }

        #endregion

        #region · Internal Methods ·

        internal void UpdateOid(int newOid)
        {
            this.oid = newOid;
        }

        #endregion

        #region · Private Methods ·

        private Type InferSystemType()
        {
            switch (this.dataType)
            {
                case PgDataType.Array:
                case PgDataType.Binary:
                case PgDataType.Vector:
                    return Type.GetType("System.Array");
                
                case PgDataType.Boolean:
                    return Type.GetType("System.Boolean");

                case PgDataType.Box:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgBox");

                case PgDataType.Circle:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgCircle");

                case PgDataType.Line:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgLine");

                case PgDataType.LSeg:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgLSeg");

                case PgDataType.Path:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgPath");

                case PgDataType.Point:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgPoint");

                case PgDataType.Polygon:
                    return Type.GetType("PostgreSql.Data.PgTypes.PgPolygon");
                
                case PgDataType.Byte:
                    return Type.GetType("System.Byte");
                
                case PgDataType.Char:
                case PgDataType.Text:
                case PgDataType.VarChar:
                    return Type.GetType("System.String");
                
                case PgDataType.Currency:
                case PgDataType.Decimal:
                case PgDataType.Numeric:
                    return Type.GetType("System.Decimal");
                
                case PgDataType.Date:
                case PgDataType.Time:
                case PgDataType.TimeWithTZ:
                case PgDataType.Timestamp:
                case PgDataType.TimestampWithTZ:
                    return Type.GetType("System.DateTime");

                case PgDataType.Double:
                    return Type.GetType("System.Double");
                
                case PgDataType.Float:
                    return Type.GetType("System.Single");
                
                case PgDataType.Int2:
                    return Type.GetType("System.Int16");
                
                case PgDataType.Int4:
                    return Type.GetType("System.Int32");
                
                case PgDataType.Int8:
                    return Type.GetType("System.Int64");

                case PgDataType.Refcursor:
                    return typeof(DataTable);

                default:
                    return Type.GetType("System.Object");
            }
        }

        #endregion
    }
}
