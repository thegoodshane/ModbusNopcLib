using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    /// <summary>modbus data type</summary>
    public enum MbDataType // named MbClientDataType instead?
    {
        /// <summary>single bit</summary>
        Boolean,

        /// <summary>unsigned 16 bit value</summary>
        Word,

        /// <summary>signed 16 bit value</summary>
        Short,

        /// <summary>unsigned 32 bit value</summary>
        Dword,

        /// <summary>signed 32 bit value</summary>
        Long,

        /// <summary>32 bit floating point value</summary>
        Float,

        /// <summary>64 bit floating point value</summary>
        Double
    }
}
