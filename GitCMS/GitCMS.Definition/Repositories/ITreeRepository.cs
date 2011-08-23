using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCMS.Definition.Models;

namespace GitCMS.Definition.Repositories
{
    public interface ITreeRepository
    {
        Tree CreateTree(Tree tr);
    }
}
