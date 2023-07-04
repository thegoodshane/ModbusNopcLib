using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    internal class MbapHeader // modbus application protocol header
    {
        internal MbapHeader() { } // for request

        // for convenient request
        internal MbapHeader(UInt16 transactionIdentifier, byte unitIdentifier)
        {
            TransactionIdentifier = transactionIdentifier;
            UnitIdentifier = unitIdentifier;
        }

        internal MbapHeader(byte[] encodedMbapHeader) // for response
        {
            Decode(encodedMbapHeader);
        }

        internal UInt16 TransactionIdentifier { get; set; }

        internal UInt16 ProtocolIdentifier { get; private set; }

        internal UInt16 Length { get; private set; }

        internal byte UnitIdentifier { get; set; }

        internal byte[] Encode()
        {
            ProtocolIdentifier = 0;
            Length = 6;

            byte[] transactionIdentifierBytes = BitConverter.GetBytes(TransactionIdentifier),
                protocolIdentifierBytes = BitConverter.GetBytes(ProtocolIdentifier),
                lengthBytes = BitConverter.GetBytes(Length);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(transactionIdentifierBytes);
                Array.Reverse(protocolIdentifierBytes); // unnecessary
                Array.Reverse(lengthBytes);
            }

            return new byte[7]
            {
                transactionIdentifierBytes[0],
                transactionIdentifierBytes[1],
                protocolIdentifierBytes[0],
                protocolIdentifierBytes[1],
                lengthBytes[0],
                lengthBytes[1],
                UnitIdentifier
            };
        }

        private void Decode(byte[] encodedMbapHeader)
        {
            byte[] transactionIdentifierBytes = new byte[2],
                protocolIdentifierBytes = new byte[2],
                lengthBytes = new byte[2];

            transactionIdentifierBytes[0] = encodedMbapHeader[0];
            transactionIdentifierBytes[1] = encodedMbapHeader[1];
            protocolIdentifierBytes[0] = encodedMbapHeader[2];
            protocolIdentifierBytes[1] = encodedMbapHeader[3];
            lengthBytes[0] = encodedMbapHeader[4];
            lengthBytes[1] = encodedMbapHeader[5];
            UnitIdentifier = encodedMbapHeader[6];

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(transactionIdentifierBytes);
                Array.Reverse(protocolIdentifierBytes);
                Array.Reverse(lengthBytes);
            }

            TransactionIdentifier = BitConverter.ToUInt16(transactionIdentifierBytes, 0);
            ProtocolIdentifier = BitConverter.ToUInt16(protocolIdentifierBytes, 0);
            Length = BitConverter.ToUInt16(lengthBytes, 0);
        }
    }
}
