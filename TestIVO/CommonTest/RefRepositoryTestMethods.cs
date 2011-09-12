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
        private IStreamedBlobRepository blrepo;
        private ITreeRepository trrepo;
        private ICommitRepository cmrepo;
        private ITagRepository tgrepo;
        private IRefRepository rfrepo;

        internal RefRepositoryTestMethods(ICommitRepository cmrepo, IStreamedBlobRepository blrepo, ITreeRepository trrepo, ITagRepository tgrepo, IRefRepository rfrepo)
        {
            this.cmrepo = cmrepo;
            this.blrepo = blrepo;
            this.trrepo = trrepo;
            this.tgrepo = tgrepo;
            this.rfrepo = rfrepo;
        }

        internal async Task PersistRefTest()
        {
            Tree tr = new Tree.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, Tree>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Ref rf = new Ref.Builder((RefName)"v1.0", cm.ID);
            await rfrepo.PersistRef(rf);
        }

        internal async Task GetRefByNameTest()
        {
            Tree tr = new Tree.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, Tree>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Ref rf = new Ref.Builder((RefName)"v1.0", cm.ID);
            await rfrepo.PersistRef(rf);

            Ref rrf = await rfrepo.GetRefByName((RefName)"v1.0");
            Assert.IsNotNull(rrf);
            Assert.AreEqual(rf.Name.ToString(), rrf.Name.ToString());
            Assert.AreEqual(rf.CommitID, rrf.CommitID);
        }

        internal async Task DeleteRefByNameTest()
        {
            Tree tr = new Tree.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, Tree>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Ref rf = new Ref.Builder((RefName)"v1.0", cm.ID);
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
