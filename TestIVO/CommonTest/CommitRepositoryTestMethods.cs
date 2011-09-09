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

        internal CommitRepositoryTestMethods(IStreamedBlobRepository blrepo, ITreeRepository trrepo, ICommitRepository cmrepo)
        {
            this.blrepo = blrepo;
            this.trrepo = trrepo;
            this.cmrepo = cmrepo;
        }

        Tree trRoot;
        Commit cm;

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

            cm = new Commit.Builder(
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

            await cmrepo.PersistCommit(cm);
        }

        internal async Task PersistCommitTest()
        {
            await createCommits();
        }

        internal async Task GetCommitTest()
        {
            await createCommits();

            // Get the commit back now:
            var rcm = await cmrepo.GetCommit(cm.ID);

            Assert.IsNotNull(rcm);
            Assert.AreEqual(cm.ID, rcm.ID);
            Assert.AreEqual(cm.DateCommitted.ToString(), rcm.DateCommitted.ToString());
            Assert.AreEqual(cm.Committer, rcm.Committer);
            Assert.AreEqual(cm.Message, rcm.Message);
        }
    }
}
