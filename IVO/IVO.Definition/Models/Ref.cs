using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IVO.Definition.Models
{
    public sealed partial class Ref
    {
        public void WriteTo(System.IO.Stream fs)
        {
            var bw = new BinaryWriter(fs);
            bw.WriteRaw(this.CommitID.ToString());
        }
    }
}
