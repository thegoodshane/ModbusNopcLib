using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

#if DEBUG
[assembly: InternalsVisibleTo("ModbusTests")]
#endif

namespace Modbus
{
    internal class MbRequestPdu // mb_req_pdu, modbus request protocol data unit
    {
        internal MbRequestPdu() { }

        // for convenience
        internal MbRequestPdu(byte functionCode, UInt16 startingAddress, UInt16 quantity)
        {
            FunctionCode = functionCode;
            StartingAddress = startingAddress;
            Quantity = quantity;
        }

        internal byte FunctionCode { get; set; }

        internal UInt16 StartingAddress { get; set; }

        internal UInt16 Quantity { get; set; }

        internal byte[] Encode()
        {
            byte[] startingAddressBytes = BitConverter.GetBytes(StartingAddress),
                quantityBytes = BitConverter.GetBytes(Quantity);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(startingAddressBytes);
                Array.Reverse(quantityBytes);
            }

            return new byte[5]
            {
                FunctionCode,
                startingAddressBytes[0],
                startingAddressBytes[1],
                quantityBytes[0],
                quantityBytes[1]
            };
        }
    }
}
