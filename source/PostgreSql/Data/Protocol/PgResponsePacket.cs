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
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgResponsePacket
	{
		#region · Fields ·

		private char			    message;
		private Stream			    stream;
		private BinaryReader	    packet;
		private Encoding		    encoding;
        private PgTypeCollection    dataTypes;

		#endregion

        #region · Properties ·

		public char Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

		public int Length
		{
			get { return (int)this.stream.Length; }
		}

		public int Position
		{
			get { return (int)this.stream.Position; }
		}

		public bool EOF
		{
			get
			{
				if (this.Position < this.Length)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public bool IsReadyForQuery
		{
			get { return (this.Message == PgBackendCodes.READY_FOR_QUERY); }
		}

		public bool IsCommandComplete
		{
			get { return (this.Message == PgBackendCodes.COMMAND_COMPLETE); }
		}

		public bool IsPortalSuspended
		{
			get { return (this.Message == PgBackendCodes.PORTAL_SUSPENDED); }
		}

		public bool IsNoData
		{
			get { return (this.Message == PgBackendCodes.NODATA); }
		}

		public bool IsCloseComplete
		{
			get { return (this.Message == PgBackendCodes.CLOSE_COMPLETE); }
		}

		public bool IsRowDescription
		{
			get { return (this.Message == PgBackendCodes.ROW_DESCRIPTION); }
		}

		#endregion

		#region · Constructors ·

		public PgResponsePacket(PgTypeCollection dataTypes, char message, Encoding encoding, byte[] contents) 
		{
            this.dataTypes  = dataTypes;
			this.stream     = new MemoryStream(contents);
			this.packet     = new BinaryReader(this.stream);
			this.encoding   = encoding;
			this.message    = message;
		}

		#endregion

		#region · Binary Types ·

		public byte[] ReadBytes(int count)
		{
			return this.packet.ReadBytes(count);
		}

		#endregion

		#region · String Types ·

		public char ReadChar()
		{
			return this.packet.ReadChar();
		}

		public char[] ReadChars(int count)
		{
			return this.packet.ReadChars(count);
		}

		public string ReadNullString()
		{
			StringBuilder cString = new StringBuilder();
			char c;
			
			while ((c = this.packet.ReadChar()) != PgCodes.NULL_TERMINATOR)
			{
				cString.Append(c);
			}
			
			return cString.ToString();
		}

		public string ReadString(int length)
		{
			byte[] buffer = new byte[length];
						
			this.packet.Read(buffer, 0, length);
			
			return this.encoding.GetString(buffer);
		}

		public string ReadString()
		{
			int length = this.ReadInt32();
			byte[] buffer = new byte[length];
						
			this.packet.Read(buffer, 0, length);
			
			return this.encoding.GetString(buffer);
		}

		#endregion

		#region · Boolean Types ·

		public bool ReadBoolean()
		{
			return this.packet.ReadBoolean();
		}

		#endregion

		#region · Numeric Types ·

		public byte ReadByte()
		{
			return this.packet.ReadByte();
		}

		public short ReadInt16()
		{
			return IPAddress.HostToNetworkOrder(this.packet.ReadInt16());
		}

		public int ReadInt32()
		{
			return IPAddress.HostToNetworkOrder(this.packet.ReadInt32());
		}

		public long ReadInt64()
		{
			return IPAddress.HostToNetworkOrder(this.packet.ReadInt64());
		}

		public float ReadSingle()
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(this.ReadInt32()), 0);
		}

		public float ReadCurrency()
		{
			float value = (float)this.ReadInt32();

			return (value / 100);
		}
		
		public double ReadDouble()
		{
			return BitConverter.ToDouble(BitConverter.GetBytes(this.ReadInt64()), 0);
		}

		#endregion

		#region · Date & Time Types ·

		public DateTime ReadDate()
		{
			return PgCodes.BASE_DATE.AddDays(this.ReadInt32());
		}

		public TimeSpan ReadInterval()
		{
			double	intervalTime	= this.ReadDouble();
			int		intervalMonth	= this.ReadInt32();

			TimeSpan interval = TimeSpan.FromSeconds(intervalTime);
			
			return interval.Add(TimeSpan.FromDays(intervalMonth * 30));
		}

		public DateTime ReadTime(int length)
		{
            return DateTime.ParseExact(this.ReadString(length), PgTypeStringFormats.TimeFormats, CultureInfo.CurrentCulture, DateTimeStyles.None);
		}

		public DateTime ReadTimeWithTZ(int length)
		{
			return DateTime.Parse(this.ReadString(length));
		}

        public DateTime ReadTimestamp(int length)
        {
			return DateTime.Parse(this.ReadString(length));
		}

        public DateTime ReadTimestampWithTZ(int length)
        {
			return DateTime.Parse(this.ReadString(length));
		}

		#endregion

		#region · Array & Vector Types ·

		public Array ReadArray(PgType type, int length)
		{
			if (type.FormatCode == 0)
			{
				return this.ReadStringArray(type, length);
			}
			else
			{
				// Read number of dimensions
				int dimensions	= this.ReadInt32();

				// Create arrays for the lengths and lower bounds
				int[] lengths		= new int[dimensions];
				int[] lowerBounds	= new int[dimensions];
				
				// Read flags value
				int flags = this.ReadInt32();
				if (flags != 0)
				{
					throw new NotSupportedException("Invalid flags value");
				}
				
				// Read array element type
                PgType elementType = this.dataTypes[this.ReadInt32()];

				// Read array lengths and lower bounds
				for (int i = 0; i < dimensions; i++)
				{
					lengths[i]		= this.ReadInt32();
					lowerBounds[i]	= this.ReadInt32();
				}

				// Read Array data
				if (elementType.SystemType.IsPrimitive)
				{
					return this.ReadPrimitiveArray(elementType, length, dimensions, flags, lengths, lowerBounds);
				}				
				else
				{
					return this.ReadNonPrimitiveArray(elementType, length, dimensions, flags, lengths, lowerBounds);
				}
			}
		}

		public Array ReadVector(PgType type, int length)
		{
            PgType  elementType = this.dataTypes[type.ElementType];
			Array	data		= null;
			
			data = Array.CreateInstance(elementType.SystemType, (length / elementType.Size));

			for (int i = 0; i < data.Length; i++ )
			{
				data.SetValue(this.ReadValue(elementType, elementType.Size), i);
			}

			return data;
		}

		#endregion

		#region · Geometric Types ·

		public PgPoint ReadPoint()
		{
			return new PgPoint(this.ReadDouble(), this.ReadDouble());
		}

		public PgCircle ReadCircle()
		{
			return new PgCircle(this.ReadPoint(), this.ReadDouble());
		}

		public PgLine ReadLine()
		{
			return new PgLine(this.ReadPoint(), this.ReadPoint());
		}

		public PgLSeg ReadLSeg()
		{
			return new PgLSeg(this.ReadPoint(), this.ReadPoint());
		}

		public PgBox ReadBox()
		{
			PgPoint upperRight	= this.ReadPoint();
			PgPoint lowerLeft	= this.ReadPoint();

			return new PgBox(lowerLeft, upperRight);
		}

		public PgPolygon ReadPolygon()
		{
			PgPoint[] points = new PgPoint[this.ReadInt32()];

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = this.ReadPoint();
			}

			return new PgPolygon(points);
		}

		public PgPath ReadPath()
		{
			bool		isClosedPath	= this.ReadBoolean();
			PgPoint[]	points			= new PgPoint[this.ReadInt32()];

			for (int i = 0; i < points.Length; i++)
			{
				points[i] = this.ReadPoint();
			}

			return new PgPath(isClosedPath, points);
		}

		#endregion

        #region · Common Methods ·

        public object ReadValue(PgType type, int length)
		{
			switch (type.DataType)
			{
				case PgDataType.Array:
					return this.ReadArray(type, length);

				case PgDataType.Vector:
					return this.ReadVector(type, length);
					
				case PgDataType.Binary:
					return this.ReadBytes(length);

				case PgDataType.Char:
                    return this.ReadString(length).TrimEnd();

                case PgDataType.VarChar:
                case PgDataType.Refcursor:
                    return this.ReadString(length);

				case PgDataType.Boolean:
					return this.ReadBoolean();

				case PgDataType.Byte:
					return this.ReadByte();
					
				case PgDataType.Decimal:
					return Decimal.Parse(this.ReadString(length), NumberFormatInfo.InvariantInfo);

				case PgDataType.Currency:
					return this.ReadCurrency();

				case PgDataType.Float:
					return this.ReadSingle();					

				case PgDataType.Double:
					return this.ReadDouble();					

				case PgDataType.Int2:
					return this.ReadInt16();

				case PgDataType.Int4:
					return this.ReadInt32();					

				case PgDataType.Int8:
					return this.ReadInt64();

				case PgDataType.Interval:
					return this.ReadInterval();

				case PgDataType.Date:
					return this.ReadDate();	

				case PgDataType.Time:
					return this.ReadTime(length);

				case PgDataType.TimeWithTZ:
                    return this.ReadTimeWithTZ(length);

                case PgDataType.Timestamp:
                    return this.ReadTimestamp(length);

                case PgDataType.TimestampWithTZ:
                    return this.ReadTimestampWithTZ(length);

                case PgDataType.Point:
					return this.ReadPoint();

				case PgDataType.Circle:
					return this.ReadCircle();

				case PgDataType.Line:
					return this.ReadLine();

				case PgDataType.LSeg:
					return this.ReadLSeg();
				
				case PgDataType.Box:
					return this.ReadBox();

				case PgDataType.Polygon:
					return this.ReadPolygon();

				case PgDataType.Path:
					return this.ReadPath();

				default:
					return this.ReadBytes(length);
			}
		}

		public object ReadValueFromString(PgType type, int length)
		{
            if (type.IsArray)
            {
                return this.ReadStringArray(type, length);
            }

			string stringValue = this.ReadString(length);

			switch (type.DataType)
			{
				case PgDataType.Binary:
					return null;

				case PgDataType.Char:
                    return stringValue.TrimEnd();

                case PgDataType.VarChar:
                case PgDataType.Refcursor:
                case PgDataType.Text:
                    return stringValue;

				case PgDataType.Boolean:
					switch (stringValue.ToLower())
					{
						case "t":
						case "true":
						case "y":
						case "yes":
						case "1":
							return true;

						default:
							return false;
					}

				case PgDataType.Byte:
					return Byte.Parse(stringValue);
					
				case PgDataType.Decimal:
					return Decimal.Parse(stringValue, NumberFormatInfo.InvariantInfo);

				case PgDataType.Currency:
				case PgDataType.Float:
					return Single.Parse(stringValue, NumberFormatInfo.InvariantInfo);
				
				case PgDataType.Double:
					return Double.Parse(stringValue, NumberFormatInfo.InvariantInfo);

				case PgDataType.Int2:
					return Int16.Parse(stringValue, NumberFormatInfo.InvariantInfo);

				case PgDataType.Int4:
					return Int32.Parse(stringValue, NumberFormatInfo.InvariantInfo);

				case PgDataType.Int8:
					return Int64.Parse(stringValue, NumberFormatInfo.InvariantInfo);

				case PgDataType.Interval:
					return null;
				
				case PgDataType.Date:				
				case PgDataType.Timestamp:
				case PgDataType.Time:
				case PgDataType.TimeWithTZ:
				case PgDataType.TimestampWithTZ:			
					return DateTime.Parse(stringValue);

				case PgDataType.Point:
					return PgPoint.Parse(stringValue);

				case PgDataType.Circle:
					return PgCircle.Parse(stringValue);

				case PgDataType.Line:
					return PgLine.Parse(stringValue);

				case PgDataType.LSeg:
					return PgLSeg.Parse(stringValue);
				
				case PgDataType.Box:
					return PgBox.Parse(stringValue);

				case PgDataType.Polygon:
					return PgPolygon.Parse(stringValue);

				case PgDataType.Path:
					return PgPath.Parse(stringValue);

                case PgDataType.Box2D:
                    return PgBox2D.Parse(stringValue);

                case PgDataType.Box3D:
                    return PgBox3D.Parse(stringValue);

				default:
					return this.packet.ReadBytes(length);
			}
		}

		#endregion

		#region · Array Handling Methods ·

		private Array ReadPrimitiveArray(PgType elementType, int length, 
			int dimensions, int flags, int[] lengths, int[] lowerBounds)
		{
			Array data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);

			// Read array data
			byte[] sourceArray = this.DecodeArrayData(elementType, data.Length, length);
			
			Buffer.BlockCopy(sourceArray, 0, data, 0, sourceArray.Length);

			return data;
		}

		private Array ReadNonPrimitiveArray(PgType elementType, int length, 
			int dimensions, int flags, int[] lengths, int[] lowerBounds)
		{
			Array data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);

			for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); i++)
			{
				int	elementLen = this.ReadInt32();
				data.SetValue(this.ReadValue(elementType, elementType.Size), i);
			}

			return data;
		}

		private Array ReadStringArray(PgType type, int length)
		{
            PgType  elementType = this.dataTypes[type.ElementType];
			Array	data		= null;

			string contents = this.ReadString(length);
			contents = contents.Substring(1, contents.Length - 2);

			string[] elements = contents.Split(',');

			data = Array.CreateInstance(elementType.SystemType, elements.Length);

			for (int i = 0; i < elements.Length; i++)
			{
				data.SetValue(elements[i], i);
			}

			return data;
		}

		private byte[] DecodeArrayData(PgType type, int elementCount, int length)
		{
			byte[] data = new byte[length];

			int element = 0;
			int index	= 0;
			while (element < elementCount)
			{
				byte[] elementData = null;
				int elementLen = this.ReadInt32();

				switch (type.DataType)
				{
					case PgDataType.Boolean:
						elementData = BitConverter.GetBytes(this.ReadBoolean());
						break;
					
					case PgDataType.Float:
						elementData = BitConverter.GetBytes(this.ReadSingle());
						break;

					case PgDataType.Double:
						elementData = BitConverter.GetBytes(this.ReadDouble());
						break;

					case PgDataType.Int2:
						elementData = BitConverter.GetBytes(this.ReadInt16());
						break;

					case PgDataType.Int4:
						elementData = BitConverter.GetBytes(this.ReadInt32());
						break;

					case PgDataType.Int8:
						elementData = BitConverter.GetBytes(this.ReadInt64());
						break;
				}

				// Copy element data to dest array
				elementData.CopyTo(data, index);

				// Increment counters
				element++;
				index += elementData.Length;
			}

			byte[] destArray = new byte[index];

			System.Array.Copy(data, 0, destArray, 0, destArray.Length);

			return destArray;
		}

		#endregion

        #region · Internal Methods ·

        internal byte[] ToArray()
		{
			return ((MemoryStream)this.stream).ToArray();
        }

        #endregion
    }
}
