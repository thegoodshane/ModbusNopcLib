using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus
{
    /// <summary>modbus client exception</summary>
    public class MbClientException : Exception
    {
        internal MbClientException(string message)
            : base(message) { }
    }
}
