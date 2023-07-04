using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    // mb_rsp_pdu, modbus response protocol data unit
    internal class MbResponsePdu : IMbResponse
    {
        internal MbResponsePdu(byte[] encodedMbResponsePdu)
        {
            Decode(encodedMbResponsePdu);
        }

        internal byte FunctionCode { get; private set; }

        internal byte ByteCount { get; private set; }

        internal byte[] Value { get; private set; }

        private void Decode(byte[] encodedMbResponsePdu)
        {
            FunctionCode = encodedMbResponsePdu[0];

            ByteCount = encodedMbResponsePdu[1];

            Value = new byte[encodedMbResponsePdu.Length - 2];

            Array.Copy(encodedMbResponsePdu, 2, Value, 0, Value.Length);

            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < Value.Length - 1; i += 2) // for each word
                {
                    Array.Reverse(Value, i, 2); // swap each byte
                }
            }
        }
    }
}
