using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace IVO.Definition.Models
{
    public sealed partial class Ref
    {
        public StringBuilder WriteTo(StringBuilder sb)
        {
            sb.Append(this.CommitID.ToString());
            return sb;
        }
    }
}
