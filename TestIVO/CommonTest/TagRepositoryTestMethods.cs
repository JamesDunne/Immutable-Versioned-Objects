using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Repositories;

namespace TestIVO.CommonTest
{
    class TagRepositoryTestMethods
    {
        private ITagRepository tgrepo;

        internal TagRepositoryTestMethods(ITagRepository tgrepo)
        {
            this.tgrepo = tgrepo;
        }

    }
}
