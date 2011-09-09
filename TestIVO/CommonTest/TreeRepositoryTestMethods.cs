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

        private Tree trTemplate = null, trRoot = null;

        private void createTrees()
        {
            PersistingBlob pbHeader;
            pblobs = new PersistingBlob[1] {
                pbHeader = new PersistingBlob(() => "<div>Header</div>".ToStream())
            };

            trTemplate = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("header", pbHeader.ComputedID)
                }
            );

            trRoot = new Tree.Builder(
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

        internal async Task GetTreeTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Retrieve a single tree node:
            var root = await trrepo.GetTree(rootId);
            Assert.AreEqual(trRoot.ID, root.ID);

            // Retrieve a single tree node:
            var tmpl = await trrepo.GetTree(trTemplate.ID);
            Assert.AreEqual(trTemplate.ID, tmpl.ID);
        }

        internal async Task GetTreeIDByPathTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Retrieve a single TreeID mapping:
            var rootPath = (CanonicalTreePath)"/";
            var rootMapping = await trrepo.GetTreeIDByPath(new TreeTreePath(rootId, rootPath));

            Assert.IsTrue(rootMapping != null);
            Assert.AreEqual(trRoot.ID, rootMapping.TreeID);
            Assert.AreEqual(rootPath.ToString(), rootMapping.Path.Path.ToString());
        }

        internal async Task GetTreeIDsByPathsTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Retrieve some TreeID mappings by Paths:
            var rootPath = (CanonicalTreePath)"/";
            var tmplPath = (CanonicalTreePath)"/template/";
            var rootMappings = await trrepo.GetTreeIDsByPaths(
                new TreeTreePath(rootId, rootPath),
                new TreeTreePath(rootId, tmplPath)
            );

            Assert.IsTrue(rootMappings != null);
            Assert.AreEqual(2, rootMappings.Length);

            Assert.IsTrue(rootMappings[0].TreeID.HasValue);
            Assert.AreEqual(trRoot.ID, rootMappings[0].TreeID.Value);
            Assert.AreEqual(rootPath.ToString(), rootMappings[0].Path.Path.ToString());

            Assert.IsTrue(rootMappings[1].TreeID.HasValue);
            Assert.AreEqual(trTemplate.ID, rootMappings[1].TreeID.Value);
            Assert.AreEqual(tmplPath.ToString(), rootMappings[1].Path.Path.ToString());
        }

        internal async Task GetTreeRecursivelyTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Retrieve the tree recursively:
            var rec = await trrepo.GetTreeRecursively(rootId);

            Assert.AreEqual(rootId, rec.Item1);
            Assert.AreEqual(trees.Count, rec.Item2.Count);
        }

        internal async Task GetTreeRecursivelyFromPathTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Retrieve a subtree recursively by path:
            var rec = await trrepo.GetTreeRecursivelyFromPath(new TreeTreePath(rootId, (CanonicalTreePath)"/template/"));

            Assert.AreEqual(trTemplate.ID, rec.Item1);
            Assert.AreEqual(trTemplate.Trees.Length + 1, rec.Item2.Count);
        }

        internal async Task DeleteTreeRecursivelyTest()
        {
            createTrees();

            await blrepo.PersistBlobs(pblobs);
            await trrepo.PersistTree(rootId, trees);

            // Retrieve a subtree recursively by path:
            var rec = await trrepo.DeleteTreeRecursively(rootId);
        }

    }
}
