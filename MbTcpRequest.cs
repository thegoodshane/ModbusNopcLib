using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    internal class MbTcpRequest
    {
        internal MbTcpRequest() { }

        internal MbTcpRequest(MbapHeader header, MbRequestPdu pdu) // for convenience
        {
            Header = header;
            Pdu = pdu;
        }

        internal MbapHeader Header { get; set; }

        internal MbRequestPdu Pdu { get; set; }

        internal byte[] Encode() // returns encoded application data unit (adu)
        {
            return Header.Encode()
                .Concat(Pdu.Encode())
                .ToArray();
        }
    }
}
