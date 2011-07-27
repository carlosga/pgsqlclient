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
using System.IO;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;

#if (CUSTOM_SSL)
using SecureSocketLayer.Net.Security;
#else
using System.Net.Security;
using System.Collections.Generic;
using System.Collections.Concurrent;
#endif

namespace PostgreSql.Data.Protocol
{
	internal sealed class PgDatabase
	{
		#region · Static Properties ·

        //public static readonly PgTypeCollection DataTypes = InitializeDataTypes();
		public static readonly PgCharactersetCollection Charactersets = new PgCharactersetCollection();

		#endregion

		#region · Static Methods ·

		public static PgTypeCollection InitializeDataTypes()
		{
            PgTypeCollection dataTypes = new PgTypeCollection();

			dataTypes.Add(16	, "bool"		, PgDataType.Boolean	, 0, PgTypeFormat.Binary, 1);
			dataTypes.Add(17	, "bytea"		, PgDataType.Binary		, 0, PgTypeFormat.Binary, Int32.MaxValue);
			dataTypes.Add(18	, "char"		, PgDataType.Char		, 0, PgTypeFormat.Text, 0);
			dataTypes.Add(19	, "name"		, PgDataType.VarChar	, 0, PgTypeFormat.Text, 0);
			dataTypes.Add(20	, "int8"		, PgDataType.Int8		, 0, PgTypeFormat.Binary, 8);
			dataTypes.Add(21	, "int2"		, PgDataType.Int2		, 0, PgTypeFormat.Binary, 2);
			dataTypes.Add(22	, "int2vector"	, PgDataType.Vector		, 21, PgTypeFormat.Binary, 2);
			dataTypes.Add(23	, "int4"		, PgDataType.Int4		, 0, PgTypeFormat.Binary, 4);
			dataTypes.Add(24	, "regproc"		, PgDataType.VarChar	, 0, PgTypeFormat.Text, 0);
            dataTypes.Add(25    , "text"        , PgDataType.Text       , 0, PgTypeFormat.Text, Int32.MaxValue);
			dataTypes.Add(26	, "oid"			, PgDataType.Int4		, 0, PgTypeFormat.Binary, 4);
			dataTypes.Add(30	, "oidvector"	, PgDataType.Vector		, 26, PgTypeFormat.Binary, 4);			
			dataTypes.Add(600	, "point"		, PgDataType.Point		, 701, PgTypeFormat.Binary, 16, ",");
			dataTypes.Add(601	, "lseg"		, PgDataType.LSeg		, 600, PgTypeFormat.Binary, 32, ",");
			dataTypes.Add(602	, "path"		, PgDataType.Path		, 0, PgTypeFormat.Binary, 16, ",");
			dataTypes.Add(603	, "box"			, PgDataType.Box		, 600, PgTypeFormat.Binary, 32, ";");
			dataTypes.Add(604	, "polygon"		, PgDataType.Polygon	, 0, PgTypeFormat.Binary, 16, ",");
			dataTypes.Add(628	, "line"		, PgDataType.Line		, 701, PgTypeFormat.Binary, 32, ",");
			dataTypes.Add(629	, "_line"		, PgDataType.Array		, 628, PgTypeFormat.Binary, 32);
			dataTypes.Add(718	, "circle"		, PgDataType.Circle		, 0, PgTypeFormat.Binary, 24, ",");
			dataTypes.Add(719	, "_circle"		, PgDataType.Array		, 718, PgTypeFormat.Binary, 24);
            dataTypes.Add(700   , "float4"      , PgDataType.Float      , 0, PgTypeFormat.Text, 4);
			dataTypes.Add(701	, "float8"		, PgDataType.Double		, 0, PgTypeFormat.Binary, 8);
			dataTypes.Add(705	, "unknown"		, PgDataType.Text		, 0, PgTypeFormat.Binary, 0);
			dataTypes.Add(790	, "money"		, PgDataType.Currency	, 0, PgTypeFormat.Binary, 4);
			dataTypes.Add(829	, "macaddr"		, PgDataType.VarChar	, 0, PgTypeFormat.Text, 6);
			dataTypes.Add(869	, "inet"		, PgDataType.VarChar	, 0, PgTypeFormat.Text, 0);
			dataTypes.Add(1000	, "_bool"		, PgDataType.Array		, 16, PgTypeFormat.Binary, 1);
            dataTypes.Add(1002  , "_char"       , PgDataType.Array      , 18, PgTypeFormat.Binary, 0);
			dataTypes.Add(1005	, "_int2"		, PgDataType.Array		, 21, PgTypeFormat.Binary, 2);
			dataTypes.Add(1007	, "_int4"		, PgDataType.Array		, 23, PgTypeFormat.Binary, 4);
			dataTypes.Add(1009	, "_text"		, PgDataType.Array		, 25, PgTypeFormat.Binary, 0);
			dataTypes.Add(1016	, "_int8"		, PgDataType.Array		, 20, PgTypeFormat.Binary, 8);
			dataTypes.Add(1017	, "_point"		, PgDataType.Array		, 600, PgTypeFormat.Binary, 16);
			dataTypes.Add(1018	, "_lseg"		, PgDataType.Array		, 601, PgTypeFormat.Binary, 32);
			dataTypes.Add(1019	, "_path"		, PgDataType.Array		, 602, PgTypeFormat.Binary, -1);
			dataTypes.Add(1020	, "_box"		, PgDataType.Array		, 603, PgTypeFormat.Binary, 32);
			dataTypes.Add(1021	, "_float4"		, PgDataType.Array		, 700, PgTypeFormat.Binary, 4);
			dataTypes.Add(1027	, "_polygon"	, PgDataType.Array		, 604, PgTypeFormat.Binary, 16);
            dataTypes.Add(1028  , "_oid"        , PgDataType.Array      , 26, PgTypeFormat.Binary, 4);
			dataTypes.Add(1033	, "aclitem"		, PgDataType.VarChar	, 0, PgTypeFormat.Text, 12);
			dataTypes.Add(1034	, "_aclitem"	, PgDataType.Array		, 1033, PgTypeFormat.Text, 0);
			dataTypes.Add(1042	, "bpchar"		, PgDataType.Char	    , 0, PgTypeFormat.Text, 0);
			dataTypes.Add(1043	, "varchar"		, PgDataType.VarChar	, 0, PgTypeFormat.Text, 0);
			dataTypes.Add(1082	, "date"		, PgDataType.Date		, 0, PgTypeFormat.Binary, 4);
			dataTypes.Add(1083	, "time"		, PgDataType.Time		, 0, PgTypeFormat.Text, 8);
			dataTypes.Add(1114	, "timestamp"	, PgDataType.Timestamp	, 0, PgTypeFormat.Text, 8);
			dataTypes.Add(1184	, "timestamptz"	, PgDataType.TimestampWithTZ, 0, PgTypeFormat.Binary, 8);
			dataTypes.Add(1186	, "interval"	, PgDataType.Interval	, 0, PgTypeFormat.Binary, 12);
			dataTypes.Add(1266	, "timetz"		, PgDataType.TimeWithTZ	, 0, PgTypeFormat.Binary, 12);
			dataTypes.Add(1560	, "bit"			, PgDataType.Byte		, 0, PgTypeFormat.Text, 1);
			dataTypes.Add(1562	, "varbit"		, PgDataType.Byte		, 0, PgTypeFormat.Binary, 0);
			dataTypes.Add(1700	, "numeric"		, PgDataType.Decimal	, 0, PgTypeFormat.Text, 8);
            dataTypes.Add(1790  , "refcursor"   , PgDataType.Refcursor  , 0, PgTypeFormat.Text, 0);
            dataTypes.Add(2205  , "regclass"    , PgDataType.VarChar    , 0, PgTypeFormat.Text, 0);
			//dataTypes.Add(2277	, "anyarray"	, PgDataType.Array		, 0, PgTypeFormat.Binary, 8);

            // PostGIS datatypes
            dataTypes.Add(17321 , "box3d"       , PgDataType.Box3D      , 0, PgTypeFormat.Text, 48, ",", "BOX3D");
            dataTypes.Add(17335 , "box2d"       , PgDataType.Box2D      , 0, PgTypeFormat.Text, 16, ",", "BOX");
            // dataTypes.Add(-1    , "polygon2d"   , PgDataType.Box2D      , 0, PgTypeFormat.Text, 16, ",", "POLYGON");

            return dataTypes;
		}

		public static void InitializeCharSets()
		{
			if (Charactersets.Count > 0)
			{
				return;
			}

			Charactersets.Add("SQL_ASCII"	, "ascii");			// ASCII
			Charactersets.Add("EUC_JP"		, "euc-jp");		// Japanese EUC
			Charactersets.Add("EUC_CN"		, "euc-cn");		// Chinese EUC
			Charactersets.Add("UNICODE"		, "UTF-8");			// Unicode (UTF-8)			
            Charactersets.Add("UTF8"        , "UTF-8");			// UTF-8
			Charactersets.Add("LATIN1"		, "iso-8859-1");	// ISO 8859-1/ECMA 94 (Latin alphabet no.1)			
			Charactersets.Add("LATIN2"		, "iso-8859-2");	// ISO 8859-2/ECMA 94 (Latin alphabet no.2)			
			Charactersets.Add("LATIN4"		, 1257);			// ISO 8859-4/ECMA 94 (Latin alphabet no.4)
			Charactersets.Add("ISO_8859_7"	, 1253);			// ISO 8859-7/ECMA 118 (Latin/Greek)			
			Charactersets.Add("LATIN9"		, "iso-8859-15");	// ISO 8859-15 (Latin alphabet no.9)
			Charactersets.Add("KOI8"		, "koi8-r");		// KOI8-R(U)
			Charactersets.Add("WIN"			, "windows-1251");	// Windows CP1251
            Charactersets.Add("WIN1251"     , "windows-1251");// Windows CP1251
			Charactersets.Add("WIN1256"		, "windows-1256");	// Windows CP1256 (Arabic)			
			Charactersets.Add("WIN1258"		, "windows-1258");	// TCVN-5712/Windows CP1258 (Vietnamese)
			Charactersets.Add("WIN1256"		, "windows-874");	// Windows CP874 (Thai)
		}

		#endregion

        #region · Callback Fields ·

        private NotificationCallback notification;
        private InfoMessageCallback infoMessage;

        private RemoteCertificateValidationCallback userCertificateValidationCallback;
        private LocalCertificateSelectionCallback userCertificateSelectionCallback;

        #endregion

		#region · Callback Properties ·

		public NotificationCallback Notification
		{
			get { return this.notification; }
			set { this.notification = value; }
		}

		public InfoMessageCallback InfoMessage
		{
			get { return this.infoMessage; }
			set { this.infoMessage = value; }
		}

        public RemoteCertificateValidationCallback UserCertificateValidationCallback
        {
            get { return this.userCertificateValidationCallback; }
            set { this.userCertificateValidationCallback = value; }
        }

        public LocalCertificateSelectionCallback UserCertificateSelectionCallback
        {
            get { return this.userCertificateSelectionCallback; }
            set { this.userCertificateSelectionCallback = value; }
        }

		#endregion

		#region · Fields ·

        private ConcurrentDictionary<string, string>    parameterStatus;
        private Encoding                                encoding;
        private Socket                                  socket;
		private NetworkStream		                    networkStream;
		private	SslStream			                    secureStream;
		private BinaryReader		                    receive;
		private BinaryWriter		                    send;		
		private PgConnectionOptions	                    options;
        private int                                     handle;
        private int                                     secretKey;
        private char                                    transactionStatus;
        private PgTypeCollection                        dataTypes;

		#endregion

		#region · Properties ·

		public int Handle
		{
			get { return this.handle; }
		}

		public int SecretKey
		{
			get { return this.secretKey; }
		}

        public ConcurrentDictionary<string, string> ParameterStatus
		{
			get 
			{ 
				if (this.parameterStatus == null)
				{
                    this.parameterStatus = new ConcurrentDictionary<string, string>();
				}

				return this.parameterStatus; 
			}
		}

		public PgConnectionOptions Options
		{
			get { return this.options; }
		}

		public Encoding	Encoding
		{
			get { return this.encoding; }
		}

        public PgTypeCollection DataTypes
        {
            get
            {
                if (this.dataTypes == null)
                {
                    this.dataTypes = InitializeDataTypes();
                }

                return this.dataTypes;
            }
        }

		#endregion

		#region · Internal Properties ·

		internal SslStream SecureStream
		{
			get { return this.secureStream; }
		}

		internal BinaryReader Receive
		{
			get { return this.receive; } 
		}

		internal BinaryWriter Send
		{
			get { return this.send; } 
		}

		#endregion

		#region · Constructors ·

		public PgDatabase(string connectionString)
			: this(new PgConnectionOptions(connectionString))
		{
		}

        public PgDatabase(PgConnectionOptions options)
		{
			this.options	= options;
			this.encoding	= Encoding.Default;

			GC.SuppressFinalize(this);
		}

		#endregion

		#region · Database Methods ·

		public void Connect()
		{
			try
			{
                PgDatabase.InitializeCharSets();

				this.InitializeSocket();
				
				lock (this)
				{
					if (this.options.Ssl)
					{
						// Send SSL request message
						if (this.SslRequest())
						{
							this.secureStream = new SslStream(
                                this.networkStream, 
                                false, 
                                this.UserCertificateValidationCallback,
                                this.UserCertificateSelectionCallback);

							this.SecureStream.AuthenticateAsClient(this.options.DataSource);

                            this.receive	= new BinaryReader(this.SecureStream);
							this.send		= new BinaryWriter(this.SecureStream);
						}
					}

                    // Clear parameter status
                    this.ParameterStatus.Clear();

					// Send Startup message
					PgOutputPacket packet = new PgOutputPacket(this.DataTypes, this.Encoding);
					
					packet.Write(PgCodes.PROTOCOL_VERSION3);
					packet.WriteNullString("user");
					packet.WriteNullString(this.options.UserID);

					if (this.options.Database != null &&  this.options.Database.Length > 0)
					{
						packet.WriteNullString("database");
						packet.WriteNullString(this.options.Database);
					}

					packet.WriteNullString("DateStyle");
					packet.WriteNullString(PgCodes.DATE_STYLE);
					packet.Write((byte)0);	// Terminator

					this.SendSimplePacket(packet);

					PgResponsePacket response = null;

					do
					{
						response = this.ReceiveResponsePacket();
						this.ProcessResponsePacket(response);
					}
					while (!response.IsReadyForQuery);
				}
			}
			catch (IOException ex)
			{
				this.Detach();
				throw new PgClientException(ex.Message);
			}
			catch (PgClientException)
			{
				this.Detach();
				throw;
			}
		}

		public void Disconnect()
		{
			try
			{			
				// Send packet to the server
				PgOutputPacket packet = new PgOutputPacket(this.DataTypes);				
				this.SendPacket(packet, PgFrontEndCodes.TERMINATE);

				this.Detach();
			}
			catch (IOException ex)
			{
				throw new PgClientException(ex.Message);
			}
			catch (PgClientException)
			{
				throw;
			}
		}

		#endregion

		#region · Send Methods ·

		internal void SendPacket(PgOutputPacket packet, char type)
		{
			this.Write(packet.GetPacketBytes(type));
		}

		internal void SendSimplePacket(PgOutputPacket packet)
		{
			this.Write(packet.GetSimplePacketBytes());
		}

		private void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		private void Write(byte[] buffer, int index, int count)
		{
			try
			{
				this.send.Write(buffer, index, count);
				this.send.Flush();
			}
			catch (IOException)
			{
				throw;
			}
		}

		#endregion

		#region · Response Methods ·

		public PgResponsePacket ReceiveResponsePacket()
		{
			PgResponsePacket responsePacket = null;

			lock (this)
			{
				responsePacket = this.ReceiveStandardPacket();

				switch (responsePacket.Message)
				{
					case PgBackendCodes.ERROR_RESPONSE:
						// Read the error message and trow the exception
						PgClientException ex = this.ProcessErrorPacket(responsePacket);

						// Perform a sync
						this.Sync();

						// Throw the PostgreSQL exception
						throw ex;

					case PgBackendCodes.NOTICE_RESPONSE:
						// Read the notice message and raise an InfoMessage event
						this.InfoMessage(this.ProcessErrorPacket(responsePacket));
						break;

					case PgBackendCodes.NOTIFICATION_RESPONSE:
						this.ProcessNotificationResponse(responsePacket);
						break;
				}
			}

			return responsePacket;
		}

		private PgResponsePacket ReceiveStandardPacket()
		{
			PgResponsePacket responsePacket = null;

			try
			{
				char	type	= this.receive.ReadChar();			
				int		length	= IPAddress.HostToNetworkOrder(this.receive.ReadInt32()) - 4;

				// Read the message data
				byte[]	buffer		= new byte[length];
				int		received	= 0;

				while (received < length)
				{
					received += this.receive.Read(buffer, received, length - received);
				}

				responsePacket = new PgResponsePacket(this.DataTypes, type, this.Encoding, buffer);				
			}
			catch (IOException)
			{
				throw;
			}

			return responsePacket;
		}

		private void ProcessResponsePacket(PgResponsePacket packet)
		{
			switch (packet.Message)
			{
				case PgBackendCodes.AUTHENTICATION:
					this.ProcessAuthPacket(packet);
					break;

				case PgBackendCodes.PARAMETER_STATUS:
					this.ProcessParameterStatus(packet);
					break;
				
				case PgBackendCodes.READY_FOR_QUERY:
					this.transactionStatus = packet.ReadChar();
					break;

				case PgBackendCodes.BACKEND_KEY_DATA:
					// BackendKeyData					
					this.handle		= packet.ReadInt32();
					this.secretKey	= packet.ReadInt32();
					break;
			}
		}

		private void ProcessParameterStatus(PgResponsePacket packet)
		{
			string parameterName  = packet.ReadNullString();
			string parameterValue = packet.ReadNullString();

            this.ParameterStatus.GetOrAdd(parameterName, parameterValue);

			switch (parameterName)
			{
				case "client_encoding":
					this.encoding = Charactersets[parameterValue].Encoding;
					break;
			}
		}

		private void ProcessAuthPacket(PgResponsePacket packet)
		{
			// Authentication response
			int authType = packet.ReadInt32();

			PgOutputPacket outPacket = new PgOutputPacket(this.DataTypes, this.Encoding);

			switch (authType)
			{
				case PgCodes.AUTH_OK:
					// Authentication successful
					return;
						
				case PgCodes.AUTH_KERBEROS_V4:
					// Kerberos V4 authentication is required
					break;

				case PgCodes.AUTH_KERBEROS_V5:
					// Kerberos V5 authentication is required
    				break;

				case PgCodes.AUTH_CLEARTEXT_PASSWORD:
					// Cleartext password is required
					outPacket.WriteNullString(this.options.Password);
					break;

				case PgCodes.AUTH_CRYPT_PASSWORD:
					// crypt()-encrypted password is required
					break;

				case PgCodes.AUTH_MD5_PASSWORD:
					// MD5-encrypted password is required
					
					// First read salt to use when encrypting the password
					byte[] salt = packet.ReadBytes(4);

					// Second calculate md5 of password + user
					string userHash = MD5Authentication.GetMD5Hash(
						this.Encoding.GetBytes(this.options.UserID), this.options.Password);

					// Third calculate real MD5 hash
					string hash = MD5Authentication.GetMD5Hash(salt, userHash);

					// Finally write the md5 hash to the packet
					outPacket.WriteNullString(PgCodes.MD5_PREFIX + hash);
					break;

				case PgCodes.AUTH_SCM_CREDENTIAL:
					// SCM credentials message is required
					break;
			}

			// Send the packet to the server
			this.SendPacket(outPacket, PgFrontEndCodes.PASSWORD_MESSAGE);
		}

		private PgClientException ProcessErrorPacket(PgResponsePacket packet)
		{
			char			type	= ' ';
			PgClientError	error	= new PgClientError();

			while (type != PgErrorCodes.END)
			{
				type = packet.ReadChar();
				switch (type)
				{
					case PgErrorCodes.SEVERITY:
						error.Severity = packet.ReadNullString();
						break;

					case PgErrorCodes.CODE:
						error.Code = packet.ReadNullString();
						break;

					case PgErrorCodes.MESSAGE:
						error.Message = packet.ReadNullString();
						break;

					case PgErrorCodes.DETAIL:
						error.Detail = packet.ReadNullString();
						break;

					case PgErrorCodes.HINT:
						error.Hint = packet.ReadNullString();
						break;

					case PgErrorCodes.POSITION:
						error.Position = packet.ReadNullString();
						break;

					case PgErrorCodes.WHERE:
						error.Where = packet.ReadNullString();
						break;

					case PgErrorCodes.FILE:
						error.File = packet.ReadNullString();
						break;

					case PgErrorCodes.LINE:
						error.Line = Convert.ToInt32(packet.ReadNullString());
						break;

					case PgErrorCodes.ROUTINE:
						error.Routine = packet.ReadNullString();
						break;
				}
			}

			PgClientException exception = new PgClientException(error.Message);

			exception.Errors.Add(error);

			return exception;
		}

		private void ProcessNotificationResponse(PgResponsePacket packet)
		{
			int		processID	= packet.ReadInt32();
			string	condition	= packet.ReadNullString();
			string	additional	= packet.ReadNullString();

			if (this.Notification != null)
			{
				this.Notification(processID, condition, additional);
			}
		}

		#endregion

		#region · Transaction Methods ·

		public void BeginTransaction(IsolationLevel isolationLevel)
		{
			string sql = "START TRANSACTION ISOLATION LEVEL ";

			switch (isolationLevel)
			{
				case IsolationLevel.ReadUncommitted:
					throw new NotSupportedException("Read uncommitted transaction isolation is not supported");
					
				case IsolationLevel.RepeatableRead:
					throw new NotSupportedException("Repeatable read transaction isolation is not supported");
					
				case IsolationLevel.Serializable:
					sql += "SERIALIZABLE";
					break;

				case IsolationLevel.ReadCommitted:
                default:
					sql += "READ COMMITTED";
					break;
			}

			PgStatement stmt = CreateStatement(sql);
			stmt.Query();

			if (stmt.Tag != "START TRANSACTION")
			{
				throw new PgClientException("A transaction is currently active. Parallel transactions are not supported.");
			}

			this.transactionStatus = stmt.TransactionStatus;
		}

		public void CommitTransaction()
		{
			PgStatement stmt = CreateStatement("COMMIT TRANSACTION");		
			stmt.Query();

			if (stmt.Tag != "COMMIT")
			{
				throw new PgClientException("There are no transaction for commit.");
			}

			this.transactionStatus = stmt.TransactionStatus;
		}

		public void RollbackTransction()
		{
			PgStatement stmt = CreateStatement("ROLLBACK TRANSACTION");
			stmt.Query();

			if (stmt.Tag != "ROLLBACK")
			{
				throw new PgClientException("There are no transaction for rollback.");
			}

			this.transactionStatus = stmt.TransactionStatus;
		}

		#endregion

		#region · Client Methods ·

		public void Flush()
		{
			lock (this)
			{
				try
				{
					PgOutputPacket packet = new PgOutputPacket(this.DataTypes, this.Encoding);

					// Send packet to the server
					this.SendPacket(packet, PgFrontEndCodes.FLUSH);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public void Sync()
		{
			lock (this)
			{
				try
				{
					PgOutputPacket packet = new PgOutputPacket(this.DataTypes, this.Encoding);

					// Send packet to the server
					this.SendPacket(packet, PgFrontEndCodes.SYNC);

					// Receive response
					PgResponsePacket response = null;
					
					do
					{
						response = this.ReceiveResponsePacket();
						this.ProcessResponsePacket(response);
					}
					while (!response.IsReadyForQuery);
				}
				catch
				{
					PgResponsePacket response = null;
					
					do
					{
						response = this.ReceiveResponsePacket();
						this.ProcessResponsePacket(response);
					}
					while (!response.IsReadyForQuery);

					throw;
				}
			}
		}

		public void CancelRequest()
		{
			lock (this)
			{
				try
				{
					PgOutputPacket packet = new PgOutputPacket(this.DataTypes);

					packet.Write((int)16);
					packet.Write(PgCodes.CANCEL_REQUEST);
					packet.Write(this.Handle);
					packet.Write(this.SecretKey);

					// Send packet to the server
					this.SendSimplePacket(packet);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		public bool SslRequest()
		{
			bool sslAvailable = false;

			lock (this)
			{
				try
				{
					PgOutputPacket packet = new PgOutputPacket(this.DataTypes);

					packet.Write(PgCodes.SSL_REQUEST);

					// Send packet to the server
					this.SendSimplePacket(packet);

					// Receive server response
					switch (Convert.ToChar(this.networkStream.ReadByte()))
					{
						case 'S':
							sslAvailable = true;
							break;

						default:
							sslAvailable = false;
							break;
					}
				}
				catch
				{
					throw;
				}
			}

			return sslAvailable;
		}

		#endregion

		#region · Methods ·

		public void SendInfoMessage(PgClientException exception) 
		{
			if (this.InfoMessage != null)
			{
				this.InfoMessage(exception);
			}
		}

		public PgStatement CreateStatement()
		{
			return new PgStatement(this);
		}

		public PgStatement CreateStatement(string stmtText)
		{
			return new PgStatement(this, stmtText);
		}

		public PgStatement CreateStatement(string parseName, string portalName)
		{
			return new PgStatement(this, parseName, portalName);
		}

		public PgStatement CreateStatement(string parseName, string portalName, string stmtText)
		{
			return new PgStatement(this, parseName, portalName, stmtText);
		}

		#endregion

		#region · Private Methods ·

		private void InitializeSocket()
		{
			IPAddress hostadd = GetIPAddress(this.options.DataSource, AddressFamily.InterNetwork);

			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Set Receive Buffer size.
			this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, this.options.PacketSize);

			// Set Send Buffer size.
			this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, this.options.PacketSize);
			
			// Disables the Nagle algorithm for send coalescing.
			this.socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

			// Make the socket to connect to the Server
			this.socket.Connect(new IPEndPoint(hostadd, this.options.PortNumber));					
			this.networkStream = new NetworkStream(socket, true);

			// Create objects for read & write
			this.receive = new BinaryReader(new BufferedStream(this.networkStream));
			this.send	 = new BinaryWriter(new BufferedStream(this.networkStream));

			// The socket and stream shouldn't be automatically collected by the GC
			GC.SuppressFinalize(this.socket);
			GC.SuppressFinalize(this.networkStream);
			GC.SuppressFinalize(this.receive);
			GC.SuppressFinalize(this.send);
		}

        private IPAddress GetIPAddress(string dataSource, AddressFamily addressFamily)
        {
            IPAddress[] addresses = Dns.GetHostEntry(this.options.DataSource).AddressList;

            foreach (IPAddress address in addresses)
            {
                if (address.AddressFamily == addressFamily)
                {
                    return address;
                }
            }

            return addresses[0];
        }

		private void Detach()
		{
			// Close streams
			if (this.secureStream != null)
			{
				try
				{
					this.secureStream.Close();
				}
				catch
				{
				}
			}
			if (this.networkStream != null)
			{
				this.networkStream.Close();
			}
			if (this.socket != null)
			{
				this.socket.Close();
			}
		}

		#endregion
	}
}
