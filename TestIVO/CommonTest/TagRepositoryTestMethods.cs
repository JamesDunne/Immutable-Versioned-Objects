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
    public sealed class TagRepositoryTestMethods
    {
        private IStreamedBlobRepository blrepo;
        private ITreeRepository trrepo;
        private ICommitRepository cmrepo;
        private ITagRepository tgrepo;
        private IRefRepository rfrepo;

        internal TagRepositoryTestMethods(ICommitRepository cmrepo, IStreamedBlobRepository blrepo, ITreeRepository trrepo, ITagRepository tgrepo, IRefRepository rfrepo)
        {
            this.cmrepo = cmrepo;
            this.blrepo = blrepo;
            this.trrepo = trrepo;
            this.tgrepo = tgrepo;
            this.rfrepo = rfrepo;
        }

        internal async Task PersistTagTest()
        {
            TreeNode tr = new TreeNode.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, TreeNode>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Tag tg = new Tag.Builder((TagName)"v1.0", cm.ID, "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);
        }

        internal async Task DeleteTagTest()
        {
            TreeNode tr = new TreeNode.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, TreeNode>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Tag tg = new Tag.Builder((TagName)"v1.0", cm.ID, "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            var retgPre = await tgrepo.GetTag(tg.ID);
            Assert.IsFalse(retgPre.HasErrors);

            Tag rtgPre = retgPre.Value;
            Assert.IsNotNull(rtgPre);
            Assert.AreEqual(tg.ID, rtgPre.ID);
            Assert.AreEqual(tg.Name.ToString(), rtgPre.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtgPre.CommitID);
            Assert.AreEqual(tg.Tagger, rtgPre.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtgPre.DateTagged.ToString());

            await tgrepo.DeleteTag(tg.ID);

            var retgPost = await tgrepo.GetTag(tg.ID);
            Assert.IsTrue(retgPost.HasErrors);
            Assert.AreEqual(1, retgPost.Errors.Count);
        }

        internal async Task DeleteTagByNameTest()
        {
            TreeNode tr = new TreeNode.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, TreeNode>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Tag tg = new Tag.Builder((TagName)"v1.0", cm.ID, "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            var retgPre = await tgrepo.GetTag(tg.ID);
            Assert.IsFalse(retgPre.HasErrors);

            Tag rtgPre = retgPre.Value;
            Assert.IsNotNull(rtgPre);
            Assert.AreEqual(tg.ID, rtgPre.ID);
            Assert.AreEqual(tg.Name.ToString(), rtgPre.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtgPre.CommitID);
            Assert.AreEqual(tg.Tagger, rtgPre.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtgPre.DateTagged.ToString());

            await tgrepo.DeleteTagByName((TagName)"v1.0");

            var retgPost = await tgrepo.GetTag(tg.ID);
            Assert.IsTrue(retgPost.HasErrors);
            Assert.AreEqual(1, retgPost.Errors.Count);
        }

        internal async Task GetTagTest()
        {
            TreeNode tr = new TreeNode.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID,TreeNode>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Tag tg = new Tag.Builder((TagName)"v1.0", cm.ID, "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            var retg = await tgrepo.GetTag(tg.ID);
            Assert.IsFalse(retg.HasErrors);
            Tag rtg = retg.Value;
            Assert.IsNotNull(rtg);
            Assert.AreEqual(tg.ID, rtg.ID);
            Assert.AreEqual(tg.Name.ToString(), rtg.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtg.CommitID);
            Assert.AreEqual(tg.Tagger, rtg.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtg.DateTagged.ToString());
        }

        internal async Task GetTagByNameTest()
        {
            TreeNode tr = new TreeNode.Builder(new List<TreeTreeReference>(0), new List<TreeBlobReference>(0));
            await trrepo.PersistTree(tr.ID, new IVO.Definition.Containers.ImmutableContainer<TreeID, TreeNode>(trx => trx.ID, tr));
            Commit cm = new Commit.Builder(new List<CommitID>(0), tr.ID, "James S. Dunne", DateTimeOffset.Now, "Initial commit.");
            await cmrepo.PersistCommit(cm);
            Tag tg = new Tag.Builder((TagName)"v1.0", cm.ID, "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            var retg = await tgrepo.GetTagByName((TagName)"v1.0");
            Assert.IsFalse(retg.HasErrors);
            Tag rtg = retg.Value;
            Assert.IsNotNull(rtg);
            Assert.AreEqual(tg.ID, rtg.ID);
            Assert.AreEqual(tg.Name.ToString(), rtg.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtg.CommitID);
            Assert.AreEqual(tg.Tagger, rtg.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtg.DateTagged.ToString());
        }
    }
}
