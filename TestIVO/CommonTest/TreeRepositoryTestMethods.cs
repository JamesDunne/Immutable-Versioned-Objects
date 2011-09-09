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

        internal TreeRepositoryTestMethods(ITreeRepository trrepo)
        {
            this.trrepo = trrepo;
        }

        private Tuple<TreeID, ImmutableContainer<TreeID, Tree>> createTree()
        {
            Tree trTemplate = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("header", new BlobID("0000000000000000000011111111111111111111"))
                }
            );

            Tree trRoot = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("template", trTemplate.ID)
                },
                new List<TreeBlobReference>(0)
            );

            return new Tuple<TreeID, ImmutableContainer<TreeID, Tree>>(trRoot.ID, new ImmutableContainer<TreeID, Tree>(tr => tr.ID, trTemplate, trRoot));
        }

        internal async Task PersistTreeTest()
        {
            var tr = createTree();
            await trrepo.PersistTree(tr.Item1, tr.Item2);
        }

        internal async Task GetTreesTest()
        {
            var tr = createTree();

            // Persist the sample trees:
            await trrepo.PersistTree(tr.Item1, tr.Item2);

            // Now retrieve those trees:
            var treeIDs = tr.Item2.Keys.ToArray();
            var trGot = await trrepo.GetTrees(treeIDs);

            Assert.AreEqual(treeIDs[0], trGot[0].ID);
        }
    }
}
