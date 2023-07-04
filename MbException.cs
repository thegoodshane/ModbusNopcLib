using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    /// <summary>modbus exception</summary>
    public class MbException : Exception
    {
        internal MbException(byte exceptionCode)
        {
            ExceptionCode = exceptionCode;
        }

        /// <summary>modbus exception code</summary>
        public byte ExceptionCode { get; set; }

        /// <summary>modbus exception name</summary>
        public override string Message
        {
            get
            {
                switch (ExceptionCode)
                {
                    case 1: return "illegal function";
                    case 2: return "illegal data address";
                    case 3: return "illegal data value";
                    case 4: return "server device failure";
                    default: return "unknown exception code";
                }
            }
        }
    }
}
