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
    class TreeRepositoryTestMethods
    {
        private ITreeRepository trrepo;
        private IStreamedBlobRepository blrepo;

        internal TreeRepositoryTestMethods(IStreamedBlobRepository blrepo, ITreeRepository trrepo)
        {
            this.blrepo = blrepo;
            this.trrepo = trrepo;
        }

        private TreeID rootId;
        private ImmutableContainer<TreeID, Tree> trees;
        private PersistingBlob[] pblobs;

        private void createTrees()
        {
            PersistingBlob pbHeader;
            pblobs = new PersistingBlob[1] {
                pbHeader = new PersistingBlob(() => "<div>Header</div>".ToStream())
            };

            Tree trTemplate = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("header", pbHeader.ComputedID)
                }
            );

            Tree trRoot = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("template", trTemplate.ID)
                },
                new List<TreeBlobReference>(0)
            );

            rootId = trRoot.ID;
            trees = new ImmutableContainer<TreeID, Tree>(tr => tr.ID, trTemplate, trRoot);
        }

        internal async Task PersistTreeTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);
        }

        internal async Task GetTreesTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Now retrieve those trees:
            var treeIDs = trees.Keys.ToArray();
            var trGot = await trrepo.GetTrees(treeIDs);

            Assert.AreEqual(treeIDs[0], trGot[0].ID);
        }
    }
}
