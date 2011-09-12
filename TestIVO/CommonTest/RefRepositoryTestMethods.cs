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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO.CommonTest
{
    public sealed class RefRepositoryTestMethods
    {
        private IRefRepository rfrepo;

        internal RefRepositoryTestMethods(IRefRepository rfrepo)
        {
            this.rfrepo = rfrepo;
        }

        // FIXME!!!
        private static readonly CommitID cmID = new CommitID("0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a");

        internal async Task PersistRefTest()
        {
            Ref rf = new Ref.Builder((RefName)"v1.0", cmID);
            await rfrepo.PersistRef(rf);
        }

        internal async Task GetRefByNameTest()
        {
            Ref rf = new Ref.Builder((RefName)"v1.0", cmID);
            await rfrepo.PersistRef(rf);

            Ref rrf = await rfrepo.GetRefByName((RefName)"v1.0");
            Assert.IsNotNull(rrf);
            Assert.AreEqual(rf.Name.ToString(), rrf.Name.ToString());
            Assert.AreEqual(rf.CommitID, rrf.CommitID);
        }

        internal async Task DeleteRefByNameTest()
        {
            Ref rf = new Ref.Builder((RefName)"v1.0", cmID);
            await rfrepo.PersistRef(rf);

            Ref drf = await rfrepo.DeleteRefByName((RefName)"v1.0");
            Assert.IsNotNull(drf);
            Assert.AreEqual(rf.Name.ToString(), drf.Name.ToString());
            Assert.AreEqual(rf.CommitID, drf.CommitID);

            Ref rrf = await rfrepo.GetRefByName((RefName)"v1.0");
            Assert.IsNull(rrf);
        }
    }
}
