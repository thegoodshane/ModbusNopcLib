using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    // mb_excep_rsp_pdu, modbus exception response protocol data unit
    internal class MbExceptionPdu : IMbResponse
    {
        internal MbExceptionPdu(byte[] encodedMbExceptionPdu)
        {
            Decode(encodedMbExceptionPdu);
        }

        internal byte ErrorCode { get; private set; }

        internal byte ExceptionCode { get; private set; }

        private void Decode(byte[] encodedMbExceptionPdu)
        {
            ErrorCode = encodedMbExceptionPdu[0];

            ExceptionCode = encodedMbExceptionPdu[1];
        }
    }
}
