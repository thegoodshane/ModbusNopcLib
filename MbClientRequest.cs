// document public members

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    /// <summary>modbus client request</summary>
    public class MbClientRequest
    {
        internal MbClientRequest() { }

        /// <summary>
        /// identification of a remote slave connected on a serial line or on other buses
        /// </summary>
        public byte UnitIdentifier { get; internal set; }

        internal UInt16 TransactionIdentifier { get; set; } // for finding in buffer

        /// <summary>
        /// 6 digit modbus address: 1 digit primary table + 5 digit data item from 1 to 65535
        /// </summary>
        public string MbAddress { get; internal set; }

        /// <summary>modbus data type</summary>
        public MbDataType DataType { get; internal set; }
        
        /// <summary>
        /// value of the requested data item cast to the clr equivalent of the requested data type
        /// </summary>
        public object ResponseValue { get; internal set; }
    }
}
