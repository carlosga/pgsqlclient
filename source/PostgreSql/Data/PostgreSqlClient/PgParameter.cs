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

using PostgreSql.Data.PgTypes;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    [ParenthesizePropertyName(true)]
    public sealed class PgParameter 
        : DbParameter, ICloneable
    {
        #region · Fields ·
                
        private ParameterDirection	  direction;
        private DataRowVersion		  sourceVersion;
        private bool				  isNullable;
        private bool                  sourceColumnNullMapping;
        private string				  parameterName;
        private string				  sourceColumn;
        private object				  value;
        private byte				  precision;
        private byte				  scale;
        private int					  size;
        private PgDbType			  providerType;
        private bool                  isTypeSet;
        private PgParameterCollection parent;

        #endregion

        #region · Properties ·

        [DefaultValue("")]
        public override string ParameterName 
        {
            get { return this.parameterName; }
            set { this.parameterName = value; }
        }

        [Category("Data")]
        [DefaultValue((byte)0)]
        public override byte Precision
        {
            get { return this.precision; }
            set { this.precision = value; }
        }

        [Category("Data")]
        [DefaultValue((byte)0)]
        public override byte Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        [Category("Data")]
        [DefaultValue(0)]
        public override int Size
        {
            get { return this.size; }
            set { this.size = value; }
        }

        [Browsable(false)]
        [Category("Data")]
        [RefreshProperties(RefreshProperties.All)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override DbType DbType 
        {
            get { return this.PgDbTypeToDbType(this.providerType); }
            set { this.PgDbType = this.DbTypeToPgType(value); }
        }

        [RefreshProperties(RefreshProperties.All)]
        [Category("Data")]
        [DefaultValue(PgDbType.VarChar)]
        public PgDbType PgDbType
        {
            get { return this.providerType; }
            set 
            { 
                this.providerType   = value;
                this.isTypeSet      = true;
            }
        }

        [Category("Data")]
        [DefaultValue(ParameterDirection.Input)]
        public override ParameterDirection Direction 
        {
            get { return this.direction; }
            set { this.direction = value; }
        }

        [Browsable(false)]
        [DesignOnly(true)] 
        [DefaultValue(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override bool IsNullable
        {
            get { return this.isNullable; }
            set { this.isNullable = value; }  
        }

        [Category("Data")]
        [DefaultValue("")]
        public override string SourceColumn
        {
            get { return this.sourceColumn; }
            set { this.sourceColumn = value; }
        }

        [Category("Data")]
        [DefaultValue(DataRowVersion.Current)]
        public override DataRowVersion SourceVersion 
        {
            get { return this.sourceVersion; }
            set { this.sourceVersion = value; }
        }

        [Category("Data")]
        [TypeConverter(typeof(StringConverter))]
        [DefaultValue(null)]
        public override object Value 
        {
            get { return this.value; }
            set
            {
                if (value == null)
                {
                    value = System.DBNull.Value;
                }
                
                this.value = value;
                
                if (!this.isTypeSet)
                {
                    this.SetPgTypeFromValue(this.value);
                }
            }
        }

        public override bool SourceColumnNullMapping
        {
            get { return this.sourceColumnNullMapping; }
            set { this.sourceColumnNullMapping = value; }
        }

        #endregion

        #region · Internal Properties ·

        internal PgParameterCollection Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }

        #endregion

        #region · Constructors ·

        public PgParameter()
        {
            this.direction	   = ParameterDirection.Input;
            this.sourceVersion = DataRowVersion.Current;
            this.isNullable	   = false;
            this.providerType  = PgDbType.VarChar;
        }

        public PgParameter(string parameterName, object value)  : this()
        {
            this.parameterName = parameterName;
            this.value		   = value;
        }

        public PgParameter(string parameterName, PgDbType dbType) : this()
        {
            this.parameterName = parameterName;
            this.providerType  = dbType;
        }

        public PgParameter(string parameterName, PgDbType dbType, int size) : this()
        {
            this.parameterName = parameterName;
            this.PgDbType      = dbType;
            this.size		   = size;
        }

        public PgParameter(string	parameterName
                         , PgDbType	dbType
                         , int		size
                         , string	sourceColumn) 
            : this()
        {
            this.parameterName = parameterName;
            this.PgDbType      = dbType;
            this.size		   = size;
            this.sourceColumn  = sourceColumn;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public PgParameter(string		      parameterName
                         , PgDbType	          dbType
                         , int			      size
                         , ParameterDirection direction
                         , bool		          isNullable
                         , byte		          precision
                         , byte		          scale
                         , string		      sourceColumn
                         , DataRowVersion     sourceVersion
                         , object		      value)
        {
            this.parameterName = parameterName;
            this.PgDbType      = dbType;
            this.size		   = size;
            this.direction	   = direction;
            this.isNullable	   = isNullable;
            this.precision	   = precision;
            this.scale		   = scale;
            this.sourceColumn  = sourceColumn;
            this.sourceVersion = sourceVersion;
            this.value		   = value;
        }

        #endregion

        #region · ICloneable Methods ·

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region · Methods ·

        public override string ToString()
        {
            return this.parameterName;
        }
    
        #endregion

        #region · Internal Methods ·

        internal string ConvertToPgString()
        {
            bool   addQuotes   = true;
            string returnValue = String.Empty;

            switch (this.providerType)
            {
                case PgDbType.Array:
                    break;

                case PgDbType.Binary:
                    break;

                case PgDbType.Boolean:
                    returnValue = Convert.ToBoolean(this.value).ToString().ToLower();
                    break;

                case PgDbType.Box:
                    returnValue = ((PgBox)this.value).ToString();
                    break;

                case PgDbType.Byte:
                    returnValue = Convert.ToByte(this.value).ToString();
                    break;

                case PgDbType.Char:
                case PgDbType.VarChar:
                case PgDbType.Text:
                    returnValue = Convert.ToString(this.value);
                    break;

                case PgDbType.Circle:
                    returnValue = ((PgCircle)this.value).ToString();
                    break;

                case PgDbType.Currency:					
                    returnValue = "$" + Convert.ToSingle(this.value).ToString();
                    break;

                case PgDbType.Date:
                    returnValue = Convert.ToDateTime(this.value).ToString("MM/dd/yyyy");
                    break;

                case PgDbType.Decimal:
                case PgDbType.Numeric:
                    returnValue = Convert.ToDecimal(this.value).ToString();
                    break;

                case PgDbType.Double:
                    returnValue = Convert.ToDouble(this.value).ToString();
                    break;

                case PgDbType.Float:
                    returnValue = Convert.ToSingle(this.value).ToString();
                    break;

                case PgDbType.Int2:
                    returnValue = Convert.ToInt16(this.value).ToString();
                    break;

                case PgDbType.Int4:
                    returnValue = Convert.ToInt32(this.value).ToString();
                    break;

                case PgDbType.Int8:
                    returnValue = Convert.ToInt64(this.value).ToString();
                    break;

                case PgDbType.Interval:
                    break;

                case PgDbType.Line:
                    returnValue = ((PgLine)this.value).ToString();
                    break;

                case PgDbType.LSeg:
                    returnValue = ((PgLSeg)this.value).ToString();
                    break;
                
                case PgDbType.Path:
                    returnValue = ((PgPath)this.value).ToString();
                    break;

                case PgDbType.Point:
                    returnValue = ((PgPoint)this.value).ToString();
                    break;

                case PgDbType.Polygon:
                    returnValue = ((PgPolygon)this.value).ToString();
                    break;
                
                case PgDbType.Time:
                    returnValue = Convert.ToDateTime(this.value).ToString("HH:mm:ss");
                    break;

                case PgDbType.Timestamp:
                    returnValue = Convert.ToDateTime(this.value).ToString("MM/dd/yyy HH:mm:ss");
                    break;

                case PgDbType.TimestampWithTZ:
                    returnValue = Convert.ToDateTime(this.value).ToString("MM/dd/yyy HH:mm:ss zz");
                    break;

                case PgDbType.TimeWithTZ:
                    returnValue = Convert.ToDateTime(this.value).ToString("HH:mm:ss zz");
                    break;

                case PgDbType.Vector:
                    break;

                default:
                    returnValue = this.value.ToString();
                    break;
            }

            if (addQuotes)
            {
                returnValue = "'" + returnValue + "'";
            }

            return returnValue;
        }

        #endregion

        #region · Protected Methods ·

        public override void ResetDbType()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region · Private Methods ·

        private void SetPgTypeFromValue(object value)
        {
            if (value == null)
            {
                value = System.DBNull.Value;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    this.providerType = PgDbType.Byte;
                    break;

                case TypeCode.Boolean:
                    this.providerType = PgDbType.Boolean;
                    break;

                case TypeCode.Object:
                    PgDbType = PgDbType.Binary;
                    break;
            
                case TypeCode.String:
                case TypeCode.Char:
                    this.providerType = PgDbType.Char;
                    break;

                case TypeCode.Int16:
                    this.providerType = PgDbType.Int2;
                    break;

                case TypeCode.Int32:
                    this.providerType = PgDbType.Int4;
                    break;

                case TypeCode.Int64:
                    this.providerType = PgDbType.Int8;
                    break;

                case TypeCode.Single:
                    this.providerType = PgDbType.Float;
                    break;

                case TypeCode.Double:
                    this.providerType = PgDbType.Double;
                    break;

                case TypeCode.Decimal:
                    this.providerType = PgDbType.Decimal;
                    break;

                case TypeCode.DateTime:
                    this.providerType = PgDbType.Timestamp;
                    break;

                case TypeCode.DBNull:
                    break;

                case TypeCode.Empty:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                default:
                    throw new SystemException("Value is of invalid data type.");
            }
        }

        private DbType PgDbTypeToDbType(PgDbType PgDbType)
        {
            switch (this.providerType)
            {
                case PgDbType.Boolean:
                    return DbType.Boolean;

                case PgDbType.Byte:
                    return DbType.Byte;

                case PgDbType.Binary:
                    return DbType.Binary;

                case PgDbType.Char:
                case PgDbType.Text:				
                case PgDbType.VarChar:
                    return DbType.String;

                case PgDbType.Int2:
                    return DbType.Int16;

                case PgDbType.Int4:
                    return DbType.Int32;

                case PgDbType.Int8:
                    return DbType.Int64;

                case PgDbType.Date:
                    return DbType.Date;

                case PgDbType.Time:
                    return DbType.Time;

                case PgDbType.Timestamp:
                    return DbType.DateTime;

                case PgDbType.Decimal:
                case PgDbType.Numeric:
                    return DbType.Decimal;
            
                case PgDbType.Float:
                    return DbType.Single;

                case PgDbType.Double:
                    return DbType.Double;

                default:
                    throw new InvalidOperationException("Invalid data type specified.");
            }			
        }

        private PgDbType DbTypeToPgType(DbType dbType)
        {			
            switch (dbType)
            {
                case DbType.Byte:
                    return PgDbType.Byte;

                case DbType.Boolean:
                    return PgDbType.Boolean;									

                case DbType.AnsiString:
                case DbType.String:
                    return PgDbType.VarChar;
                
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return PgDbType.Char;
                
                case DbType.Binary:
                case DbType.Object:
                    return PgDbType.Binary;
                
                case DbType.Date:
                    return PgDbType.Date;

                case DbType.Time:
                    return PgDbType.Time;

                case DbType.DateTime:
                    return PgDbType.Timestamp;

                case DbType.Int16:
                case DbType.UInt16:
                    return PgDbType.Int2;

                case DbType.Int32:
                case DbType.UInt32:
                    return PgDbType.Int4;

                case DbType.Int64:
                case DbType.UInt64:
                    return PgDbType.Int8;

                case DbType.Single:
                    return PgDbType.Float;

                case DbType.Decimal:
                    return PgDbType.Decimal;

                case DbType.Double:
                    return PgDbType.Double;

                case DbType.Currency:
                    return PgDbType.Currency;

                case DbType.Guid:
                case DbType.VarNumeric:
                case DbType.SByte:
                default:
                    throw new InvalidOperationException("Invalid data type specified.");
            }
        }

        #endregion
    }
}