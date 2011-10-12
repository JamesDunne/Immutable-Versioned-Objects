using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IVO.Definition.Models
{
    public sealed partial class Stage
    {
        public StringBuilder WriteTo(StringBuilder sb)
        {
            sb.AppendFormat("name {0}\n", Name);
            sb.AppendFormat("tree {0}\n", TreeID);

            return sb;
        }
    }
}
