using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public static class Extensions
    {
        public static System.IO.Stream ToStream(this string contents)
        {
            return new System.IO.MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
