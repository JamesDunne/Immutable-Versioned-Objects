using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IVO.Definition.Models
{
    public sealed partial class Stage
    {
        public void WriteTo(Stream fs)
        {
            var bw = new BinaryWriter(fs, Encoding.UTF8);

            bw.WriteRaw(String.Format("name {0}\n", Name));
            bw.WriteRaw(String.Format("tree {0}\n", TreeID));
            bw.Flush();
        }
    }
}
