using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    internal static class Extensions
    {
        /// <summary>
        /// Write a UTF-8 encoded string (no length prefix).
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="value"></param>
        internal static void WriteRaw(this BinaryWriter bw, string value)
        {
            bw.Write(Encoding.UTF8.GetBytes(value));
        }
    }
}