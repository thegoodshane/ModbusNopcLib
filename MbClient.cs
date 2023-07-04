using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modbus
{
    /// <summary>
    /// modbus client decoupled from any networking implementation. 
    /// only one client should use any single tcp connection (1 client : 1 tcp). 
    /// all modbus/tcp packets should be sent via tcp to registered port 502. 
    /// modbus/tcp max packet size is 260 bytes.
    /// </summary>
    public class MbClient
    {
        #region Dictionaries

        // key: primary table, value: function code
        private static readonly Dictionary<byte, byte> tableFunctionMap =
            new Dictionary<byte, byte>()
        {
            { 0, 1 },   // coils, read coils
            { 1, 2 },   // discrete inputs, read discrete inputs
            { 3, 4 },   // input registers, read input registers
            { 4, 3 }    // holding registers, read holding registers
        };

        // key: data type, value: quantity of bits or words
        private static readonly Dictionary<MbDataType, UInt16> typeSizeMap =
            new Dictionary<MbDataType, ushort>()
        {
            { MbDataType.Boolean, 1 },
            { MbDataType.Word, 1 },
            { MbDataType.Short, 1 },
            { MbDataType.Dword, 2 },
            { MbDataType.Long, 2 },
            { MbDataType.Float, 2 },
            { MbDataType.Double, 4 }
        };

        #endregion

        internal List<MbClientRequest> requests = new List<MbClientRequest>();

        // So, at a time, on a TCP connection, this identifier must be unique.
        // can't be used to find the oldest transaction because numbers wrap
        internal UInt16 transactionIdentifier;

        // NumberMaxOfClientTransaction
        /// <summary>maximum number of simultaneous pending requests</summary>
        public int MaxTransactions { get; private set; }

        /// <summary>create a modbus client with 16 max transactions</summary>
        public MbClient()
        {
            MaxTransactions = 16;
        }

        /// <summary>create a modbus client and set the max transactions</summary>
        /// <param name="maxTransactions">maximum number of simultaneous pending requests</param>
        /// <exception cref="ArgumentOutOfRangeException" />
        public MbClient(int maxTransactions)
        {
            if (maxTransactions < 1 || maxTransactions > 16)
            {
                throw new ArgumentOutOfRangeException("max transactions must be from 1 to 16");
            }

            MaxTransactions = maxTransactions;
        }

        /// <summary>
        /// build a modbus client request
        /// </summary>
        /// <param name="unitIdentifier">
        /// identification of a remote slave connected on a serial line or on other buses
        /// </param>
        /// <param name="mbAddress">
        /// 6 digit modbus address: 1 digit primary table + 5 digit data item from 1 to 65535
        /// </param>
        /// <param name="dataType">modbus data type</param>
        /// <returns>a packet for transport via tcp to the modbus device</returns>
        /// <exception cref="ArgumentException" />
        public byte[] BuildRequest(byte unitIdentifier, string mbAddress, MbDataType dataType)
        {
            byte functionCode;
            UInt16 startingAddress;
            ParseMbAddress(mbAddress, out functionCode, out startingAddress);

            // MODBUS data numbered X is addressed in the MODBUS PDU X-1
            startingAddress--;

            var request = new MbClientRequest()
            {
                UnitIdentifier = unitIdentifier,
                TransactionIdentifier = transactionIdentifier,
                MbAddress = mbAddress,
                DataType = dataType
            };

            QueueRequest(request);

            var tcpRequest = new MbTcpRequest()
            {
                Header = new MbapHeader(transactionIdentifier, unitIdentifier),
                Pdu = new MbRequestPdu(functionCode, startingAddress, typeSizeMap[dataType])
            };

            transactionIdentifier++; // increment after using in both requests

            return tcpRequest.Encode();
        }

        //public byte[] BuildRequest<T>(byte unitIdentifier, string mbAddress)

        internal void QueueRequest(MbClientRequest request)
        {
            requests.Add(request);

            // if the buffer is too full
            if (requests.Count > MaxTransactions)
            {
                // find and remove the oldest request
                requests.RemoveAt(0);
            }
        }

        /// <returns>MbClientRequest or null</returns>
        /// <exception cref="InvalidOperationException" />
        internal MbClientRequest DequeueRequest(UInt16 transactionIdentifier)
        {
            // find pending transaction
            var request = requests.SingleOrDefault
                (rq => rq.TransactionIdentifier == transactionIdentifier);
            
            requests.Remove(request);

            return request;
        }

        /// <summary>
        /// process a modbus device response
        /// </summary>
        /// <param name="responseBytes">the response packet received from the modbus device</param>
        /// <returns>the original request fulfilled with a response</returns>
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MbClientException" />
        /// <exception cref="MbException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentOutOfRangeException" /> 
        public MbClientRequest ProcessResponse(byte[] responseBytes)
        {
            var tcpResponse = new MbTcpResponse(responseBytes);

            var header = tcpResponse.Header;

            var request = DequeueRequest(header.TransactionIdentifier);

            ValidateHeader(header, request);

            ValidatePdu(tcpResponse.Pdu, request);

            MbResponsePdu pdu = (MbResponsePdu)tcpResponse.Pdu;

            request.ResponseValue = ToValueType(pdu.Value, request.DataType);

            return request;
        }

        /// <exception cref="ArgumentException" />
        internal static void ParseMbAddress(
            string mbAddress,
            out byte functionCode,
            out UInt16 startingAddress
            )
        {
            // 0, 1, 3, 4 followed by 5 digits
            var pattern = @"^(0|1|3|4)(\d{5})$";

            if (!Regex.IsMatch(mbAddress, pattern))
            {
                throw new ArgumentException(
                    "address must be 6 digits beginning with the primary table number"
                    );
            }

            var primaryTable = Byte.Parse(mbAddress.Substring(0, 1));
            functionCode = tableFunctionMap[primaryTable];
            startingAddress = UInt16.Parse(mbAddress.Substring(1, 5));
        }

        /// <exception cref="ArgumentOutOfRangeException" />
        internal static object ToValueType(byte[] value, MbDataType dataType)
        {
            switch (dataType)
            {
                case MbDataType.Boolean: return BitConverter.ToBoolean(value, 0);
                case MbDataType.Word: return BitConverter.ToUInt16(value, 0);
                case MbDataType.Short: return BitConverter.ToInt16(value, 0);
                case MbDataType.Dword: return BitConverter.ToUInt32(value, 0);
                case MbDataType.Long: return BitConverter.ToInt32(value, 0);
                case MbDataType.Float: return BitConverter.ToSingle(value, 0);
                case MbDataType.Double: return BitConverter.ToDouble(value, 0);
                default: throw new ArgumentOutOfRangeException("invalid data type");
            }
        }

        /// <exception cref="MbClientException" />
        internal static void ValidateHeader(MbapHeader header, MbClientRequest request)
        {
            if (request == null)
            {
                throw new MbClientException("no pending transaction");
            }

            if (header.ProtocolIdentifier != 0)
            {
                throw new MbClientException("other protocol");
            }

            if (header.UnitIdentifier != request.UnitIdentifier)
            {
                throw new MbClientException("incorrect response");
            }
        }

        /// <exception cref="MbException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="MbClientException" />
        internal static void ValidatePdu(IMbResponse pdu, MbClientRequest request)
        {
            if (pdu is MbExceptionPdu) // modbus exception response
            {
                var exceptionPdu = (MbExceptionPdu)pdu;

                var exceptionCode = exceptionPdu.ExceptionCode;

                throw new MbException(exceptionCode);
            }

            var normalPdu = (MbResponsePdu)pdu;

            byte functionCode;
            UInt16 startingAddress;
            ParseMbAddress(request.MbAddress, out functionCode, out startingAddress);

            if (normalPdu.FunctionCode != functionCode)
            {
                throw new MbClientException("incorrect response");
            }
        }
    }
}
