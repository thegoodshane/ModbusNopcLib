using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    internal class MbTcpResponse
    {
        internal MbapHeader Header { get; set; }

        internal IMbResponse Pdu { get; set; }

        internal MbTcpResponse(byte[] encodedMbTcpResponse)
        {
            Decode(encodedMbTcpResponse);
        }

        private void Decode(byte[] encodedMbTcpResponse)
        {
            byte[] encodedHeader = new byte[7];

            Array.Copy(encodedMbTcpResponse, encodedHeader, 7);

            Header = new MbapHeader(encodedHeader);

            byte[] encodedPdu = new byte[Header.Length - 1]; // length - unit identifier

            Array.Copy(encodedMbTcpResponse, 7, encodedPdu, 0, encodedPdu.Length);

            byte functionCode = encodedPdu[0];

            if (functionCode < 128) // normal response
            {
                Pdu = new MbResponsePdu(encodedPdu);
            }
            else // exception response
            {
                Pdu = new MbExceptionPdu(encodedPdu);
            }
        }
    }
}
