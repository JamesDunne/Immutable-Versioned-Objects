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
using IVO.Definition.Containers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO.CommonTest
{
    class CommitRepositoryTestMethods
    {
        private IStreamedBlobRepository blrepo;
        private ITreeRepository trrepo;
        private ICommitRepository cmrepo;
        private ITagRepository tgrepo;
        private IRefRepository rfrepo;

        internal CommitRepositoryTestMethods(IStreamedBlobRepository blrepo, ITreeRepository trrepo, ICommitRepository cmrepo, ITagRepository tgrepo, IRefRepository rfrepo)
        {
            this.blrepo = blrepo;
            this.trrepo = trrepo;
            this.cmrepo = cmrepo;
            this.tgrepo = tgrepo;
            this.rfrepo = rfrepo;
        }

        internal static void RecursivePrint(CommitID cmID, ImmutableContainer<CommitID, ICommit> commits, int depth = 1)
        {
            ICommit cm;
            if (!commits.TryGetValue(cmID, out cm))
                return;

            if (cm.IsComplete)
            {
                Console.WriteLine("{0}c {1}:  ({2})", new string(' ', (depth - 1) * 2), cm.ID.ToString(firstLength: 7), String.Join(",", cm.Parents.Select(id => id.ToString(firstLength: 7))));
                foreach (CommitID parentID in cm.Parents)
                {
                    RecursivePrint(parentID, commits, depth + 1);
                }
            }
            else
            {
                Console.WriteLine("{0}p  {1}:  ?", new string(' ', (depth - 1) * 2), cm.ID.ToString(firstLength: 7));
            }
        }

        Tree trRoot;
        Commit cmRoot;

        private async Task createCommits()
        {
            PersistingBlob pblReadme;

            trRoot = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference>
                {
                    new TreeBlobReference.Builder(
                        "README",
                        (pblReadme = new PersistingBlob(() => "Readme file.".ToStream())).ComputedID
                    )
                }
            );

            cmRoot = new Commit.Builder(
                new List<CommitID>(0),
                trRoot.ID,
                "James S. Dunne",
                DateTimeOffset.Now,
                "Hello world."
            );

            PersistingBlob[] pblobs = new PersistingBlob[] { pblReadme };
            await blrepo.PersistBlobs(pblobs);

            var trees = new ImmutableContainer<TreeID, Tree>(tr => tr.ID, trRoot);
            await trrepo.PersistTree(trRoot.ID, trees);

            TreeRepositoryTestMethods.RecursivePrint(trRoot.ID, trees);

            await cmrepo.PersistCommit(cmRoot);
        }

        Commit cmHead;

        private async Task createCommitTree()
        {
            cmHead = cmRoot;

            for (int i = 0; i < 20; ++i)
            {
                cmHead = new Commit.Builder(
                    new List<CommitID>(1) { cmHead.ID },
                    trRoot.ID,
                    "James S. Dunne",
                    DateTimeOffset.Now,
                    "Commit #" + (i + 1).ToString()
                );

                await cmrepo.PersistCommit(cmHead);
            }
        }

        internal async Task PersistCommitTest()
        {
            await createCommits();
        }

        internal async Task GetCommitTest()
        {
            await createCommits();

            // Get the commit back now:
            var rcm = await cmrepo.GetCommit(cmRoot.ID);

            Assert.IsNotNull(rcm);
            Assert.AreEqual(cmRoot.ID, rcm.ID);
            Assert.AreEqual(cmRoot.DateCommitted.ToString(), rcm.DateCommitted.ToString());
            Assert.AreEqual(cmRoot.Committer, rcm.Committer);
            Assert.AreEqual(cmRoot.Message, rcm.Message);
        }

        internal async Task GetCommitTreeTest()
        {
            await createCommits();
            await createCommitTree();

            // Get the commit tree:
            var rcmHeadTree = await cmrepo.GetCommitTree(cmHead.ID, depth: 10);

            Assert.AreEqual(cmHead.ID, rcmHeadTree.Item1);

            RecursivePrint(rcmHeadTree.Item1, rcmHeadTree.Item2);
        }

        internal async Task GetCommitTreeTest2()
        {
            await createCommits();
            await createCommitTree();

            // Get the commit tree:
            var rcmHeadTree = await cmrepo.GetCommitTree(cmHead.ID, depth: 20);

            Assert.AreEqual(cmHead.ID, rcmHeadTree.Item1);

            RecursivePrint(rcmHeadTree.Item1, rcmHeadTree.Item2);
        }

        internal async Task GetCommitByTagTest()
        {
            await createCommits();

            Tag tg = new Tag.Builder((TagName)"v1.0", cmRoot.ID, "James S. Dunne", DateTimeOffset.Now, "Tagged!");
            await tgrepo.PersistTag(tg);

            var tgcm = await cmrepo.GetCommitByTag(tg.ID);
            Assert.IsNotNull(tgcm);
            Assert.IsNotNull(tgcm.Item1);
            Assert.IsNotNull(tgcm.Item2);
            Assert.AreEqual(cmRoot.ID, tgcm.Item2.ID);
            Assert.AreEqual(cmRoot.DateCommitted.ToString(), tgcm.Item2.DateCommitted.ToString());
            Assert.AreEqual(cmRoot.Committer, tgcm.Item2.Committer);
            Assert.AreEqual(cmRoot.Message, tgcm.Item2.Message);
        }

        internal async Task GetCommitByTagNameTest()
        {
            await createCommits();

            Tag tg = new Tag.Builder((TagName)"v1.0", cmRoot.ID, "James S. Dunne", DateTimeOffset.Now, "Tagged!");
            await tgrepo.PersistTag(tg);

            var tgcm = await cmrepo.GetCommitByTagName(tg.Name);
            Assert.IsNotNull(tgcm);
            Assert.IsNotNull(tgcm.Item1);
            Assert.IsNotNull(tgcm.Item2);
            Assert.AreEqual(cmRoot.ID, tgcm.Item2.ID);
            Assert.AreEqual(cmRoot.DateCommitted.ToString(), tgcm.Item2.DateCommitted.ToString());
            Assert.AreEqual(cmRoot.Committer, tgcm.Item2.Committer);
            Assert.AreEqual(cmRoot.Message, tgcm.Item2.Message);
        }

        internal async Task GetCommitByRefNameTest()
        {
            await createCommits();

            Ref rf = new Ref.Builder((RefName)"v1.0", cmRoot.ID);
            await rfrepo.PersistRef(rf);

            var rfcm = await cmrepo.GetCommitByRefName(rf.Name);
            Assert.IsNotNull(rfcm);
            Assert.IsNotNull(rfcm.Item1);
            Assert.IsNotNull(rfcm.Item2);
            Assert.AreEqual(cmRoot.ID, rfcm.Item2.ID);
            Assert.AreEqual(cmRoot.DateCommitted.ToString(), rfcm.Item2.DateCommitted.ToString());
            Assert.AreEqual(cmRoot.Committer, rfcm.Item2.Committer);
            Assert.AreEqual(cmRoot.Message, rfcm.Item2.Message);
        }
    }
}
