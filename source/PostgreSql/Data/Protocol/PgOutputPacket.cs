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
    internal sealed class PgOutputPacket
	{
		#region · Fields ·

		private MemoryStream	    stream;
		private BinaryWriter	    packet;
		private Encoding		    encoding;
        private PgTypeCollection    dataTypes;

		#endregion

		#region · Properties ·

		public int Position
		{
			get { return (int)this.stream.Position; }
		}

		public int Length
		{
			get { return (int)this.stream.Length; }
		}

		#endregion

		#region · Constructors ·

		public PgOutputPacket(PgTypeCollection dataTypes) 
			: this(dataTypes, Encoding.Default)
		{
		}

		public PgOutputPacket(PgTypeCollection dataTypes, Encoding encoding) 
		{
			this.stream		= new MemoryStream();
			this.packet		= new BinaryWriter(this.stream);
			this.encoding	= encoding;
            this.dataTypes = dataTypes;

			this.Write(new byte[0]);
		}

		#endregion

		#region · Stream Methods ·

		public byte[] ToArray()
		{
			return this.stream.ToArray();
		}

		public void Reset()
		{
			this.stream.SetLength(0);
			this.stream.Position = 0;
		}

		#endregion

		#region · String Types ·

		public void Write(char ch)
		{
			this.packet.Write(ch);
		}

		public void Write(char[] chars)
		{
			this.packet.Write(chars);
		}

		public void WriteNullString(string value)
		{
			if (!value.EndsWith(PgCodes.NULL_TERMINATOR.ToString()))
			{
				value += PgCodes.NULL_TERMINATOR;
			}

			this.Write(this.encoding.GetBytes(value));
		}

        public void WriteString(string value)
        {
			byte[] buffer = this.encoding.GetBytes(value);

            this.Write(buffer.Length);
            this.Write(buffer);
        }

		#endregion

		#region · Numeric Types ·

		public void Write(byte value)
		{
			this.packet.Write(value);
		}

		public void Write(short value)
		{
			this.packet.Write((short)IPAddress.HostToNetworkOrder(value));
		}

		public void Write(int value)
		{
			this.packet.Write((int)IPAddress.HostToNetworkOrder(value));
		}

		public void Write(long value)
		{
			this.packet.Write((long)IPAddress.HostToNetworkOrder(value));
		}

		public void Write(float value)
		{
			this.packet.Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
		}

		public void Write(double value)
		{
			this.Write(BitConverter.ToInt64(BitConverter.GetBytes(value), 0));
		}

		#endregion

		#region · Boolean Types ·

		public void Write(bool value)
		{
			this.packet.Write(value);
		}

		#endregion

		#region · Date & Time Types ·

		public void WriteDate(DateTime date)
		{
			this.Write(date.Subtract(PgCodes.BASE_DATE).Days);
		}

		public void WriteInterval(TimeSpan interval)
		{
			int months	= (interval.Days / 30);
			int days	= (interval.Days % 30);

			this.Write(interval.Subtract(TimeSpan.FromDays(months * 30)).TotalSeconds);
			this.Write(months);
		}

		public void WriteTime(DateTime time)
		{
            this.WriteString(time.ToString("HH:mm:ss.fff"));
        }

		public void WriteTimeWithTZ(DateTime time)
		{
            this.WriteString(time.ToString("HH:mm:ss.fff zz"));
        }

		public void WriteTimestamp(DateTime timestamp)
		{
            this.WriteString(timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff"));
        }

		public void WriteTimestampWithTZ(DateTime timestamp)
		{
            this.WriteString(timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff zz"));
        }

		#endregion

		#region · Geometric Types ·

		public void Write(PgPoint point)
		{
			this.Write(point.X);
			this.Write(point.Y);
		}

		public void Write(PgCircle circle)
		{
			this.Write(circle.Center);
			this.Write(circle.Radius);
		}

		public void Write(PgLine line)
		{
			this.Write(line.StartPoint);
			this.Write(line.EndPoint);
		}

		public void Write(PgLSeg lseg)
		{
			this.Write(lseg.StartPoint);
			this.Write(lseg.EndPoint);
		}

		public void Write(PgBox box)
		{
			this.Write(box.UpperRight);
			this.Write(box.LowerLeft);
		}

		public void Write(PgPolygon polygon)
		{
			this.Write(polygon.Points.Length);

			for (int i = 0; i < polygon.Points.Length; i++)
			{
				this.Write(polygon.Points[i]);
			}
		}

		public void Write(PgPath path)
		{
			this.Write(path.IsClosedPath);
			this.Write(path.Points.Length);

			for (int i = 0; i < path.Points.Length; i++)
			{
				this.Write(path.Points[i]);
			}
		}

		#endregion

		#region · Parameters ·

		public void Write(PgParameter parameter)
		{
			int size = parameter.DataType.Size;

			if (parameter.Value == System.DBNull.Value || parameter.Value == null)
			{
				// -1 indicates a NULL argument value
				this.Write((int)-1);
			}
			else
			{
				if (parameter.DataType.DataType == PgDataType.Array	||
					parameter.DataType.DataType == PgDataType.Vector)
				{
					// Handle this type as Array values
					System.Array array = (System.Array)parameter.Value;
					
					// Get array elements type info
                    PgType elementType = this.dataTypes[parameter.DataType.ElementType];
					size = elementType.Size;

					// Create a new packet for write array parameter information
                    PgOutputPacket packet = new PgOutputPacket(this.dataTypes);

					// Write the number of dimensions
					packet.Write(array.Rank);

					// Write flags (always 0)
					packet.Write((int)0);

					// Write base type of the array elements
					packet.Write(parameter.DataType.ElementType);

					// Write lengths and lower bounds 
					for (int i = 0; i < array.Rank; i ++)
					{
						packet.Write(array.GetLength(i));
						packet.Write(array.GetLowerBound(i) + 1);
					}

					// Write array values
					foreach (object element in array)
					{
						this.WriteParameter(packet, elementType.DataType, size, element);
					}

					// Write parameter size
					this.Write(packet.Length);

					// Write parameter data
					this.Write(packet.ToArray());
				}
				else
				{
					this.WriteParameter(this, parameter.DataType.DataType, size, parameter.Value);
				}
			}
		}

		#endregion

		#region · Packet Methods ·

		public byte[] GetSimplePacketBytes()
		{
            PgOutputPacket packet = new PgOutputPacket(this.dataTypes);

			// Write packet contents
			packet.Write((int)(this.Length + 4));
			packet.Write(this.ToArray());

			return packet.ToArray();
		}

		public byte[] GetPacketBytes(char format)
		{
            PgOutputPacket packet = new PgOutputPacket(this.dataTypes);

			packet.Write((byte)format);
			packet.Write((int)(this.Length + 4));
			packet.Write(this.ToArray());

			return packet.ToArray();
		}

		#endregion

		#region · Methods ·

		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public void Write(byte[]buffer, int index, int count)
		{
			this.packet.Write(buffer, index, count);
		}

		#endregion

		#region · Private Methods ·

		private void WriteParameter(PgOutputPacket packet, PgDataType dataType, int size, object value)
        {
            switch (dataType)
            {
                case PgDataType.Binary:
                    packet.Write(((byte[])value).Length);
                    packet.Write((byte[])value);
                    break;

                case PgDataType.Byte:
					packet.Write(size);
					packet.Write((byte)value);
					break;

				case PgDataType.Boolean:
                    packet.Write(size);
                    packet.Write(Convert.ToByte((bool)value));
                    break;

                case PgDataType.Char:
                case PgDataType.VarChar:
                case PgDataType.Text:
                    packet.WriteString(value.ToString());
                    break;

				case PgDataType.Int2:
                    packet.Write(size);
                    packet.Write(Convert.ToInt16(value));
                    break;

                case PgDataType.Int4:
                    packet.Write(size);
                    packet.Write(Convert.ToInt32(value));
                    break;

                case PgDataType.Int8:
                    packet.Write(size);
                    packet.Write(Convert.ToInt64(value));
                    break;

                case PgDataType.Interval:
                    packet.Write(size);
                    packet.WriteInterval(TimeSpan.Parse(value.ToString()));
                    break;

                case PgDataType.Decimal:
                    {
                        string paramValue = Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
                        packet.Write(encoding.GetByteCount(paramValue));
                        packet.Write(paramValue.ToCharArray());
                    }
                    break;

                case PgDataType.Double:
                    packet.Write(size);
                    packet.Write(Convert.ToDouble(value));
                    break;

                case PgDataType.Float:
                    {
                        string paramValue = Convert.ToSingle(value).ToString(CultureInfo.InvariantCulture);
                        packet.Write(encoding.GetByteCount(paramValue));
                        packet.Write(paramValue.ToCharArray());
                    }
                    break;

                case PgDataType.Currency:
                    packet.Write(size);
                    packet.Write(Convert.ToInt32(Convert.ToSingle(value) * 100));
                    break;

                case PgDataType.Date:
                    packet.Write(size);
                    packet.WriteDate(Convert.ToDateTime(value));
                    break;

                case PgDataType.Time:
                    packet.WriteTime(Convert.ToDateTime(value));
                    break;

                case PgDataType.TimeWithTZ:
                    packet.WriteTimeWithTZ(Convert.ToDateTime(value));
                    break;

                case PgDataType.Timestamp:
                    packet.WriteTimestamp(Convert.ToDateTime(value));
                    break;

                case PgDataType.TimestampWithTZ:
                    packet.WriteTimestampWithTZ(Convert.ToDateTime(value));
                    break;

                case PgDataType.Point:
                    packet.Write(size);
                    packet.Write((PgPoint)value);
                    break;

                case PgDataType.Circle:
                    packet.Write(size);
                    packet.Write((PgCircle)value);
                    break;

                case PgDataType.Line:
                    packet.Write(size);
                    packet.Write((PgLine)value);
                    break;

                case PgDataType.LSeg:
                    packet.Write(size);
                    packet.Write((PgLSeg)value);
                    break;

                case PgDataType.Box:
                    packet.Write(size);
                    packet.Write((PgBox)value);
                    break;

                case PgDataType.Polygon:
                    PgPolygon polygon = (PgPolygon)value;

                    packet.Write((int)((size * polygon.Points.Length) + 4));
                    packet.Write(polygon);
                    break;

                case PgDataType.Path:
                    PgPath path = (PgPath)value;

                    packet.Write((int)((size * path.Points.Length) + 5));
                    packet.Write(path);
                    break;
            }
        }

        #endregion
    }
}