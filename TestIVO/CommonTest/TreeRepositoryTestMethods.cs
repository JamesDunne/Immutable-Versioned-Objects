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
using IVO.Definition.Errors;

namespace TestIVO.CommonTest
{
    public sealed class TreeRepositoryTestMethods
    {
        private IStreamedBlobRepository blrepo;
        private ITreeRepository trrepo;

        internal TreeRepositoryTestMethods(IStreamedBlobRepository blrepo, ITreeRepository trrepo)
        {
            this.blrepo = blrepo;
            this.trrepo = trrepo;
        }

        private TreeID rootId;
        private ImmutableContainer<TreeID, TreeNode> trees;

        private TreeNode trTemplate = null, trContent = null, trPages = null, trRoot = null;
        private TreeNode trImages = null, trCSS = null, trJS = null;
        private TreeNode trSection1 = null, trSection2 = null, trSection3 = null;
        private Errorable<IStreamedBlob>[] sblobs;

        private T assertNoErrors<T>(Errorable<T> err)
        {
            if (err.HasErrors)
            {
                foreach (var e in err.Errors)
                {
                    Console.Error.WriteLine("ERROR: {0}", e.Message);
                }
                Assert.IsFalse(err.HasErrors);
            }
            return err.Value;
        }

        internal static void RecursivePrint(TreeID treeID, ImmutableContainer<TreeID, TreeNode> trees, int depth = 1)
        {
            TreeNode tr;
            if (!trees.TryGetValue(treeID, out tr))
                return;

            if (depth == 1)
            {
                Console.WriteLine("tree {1}: {0}/", new string('_', (depth - 1) * 2), tr.ID.ToString(firstLength: 7));
            }

            // Sort refs by name:
            var namedRefs = TreeNode.ComputeChildList(tr.Trees, tr.Blobs);

            foreach (var kv in namedRefs)
            {
                var nref = kv.Value;
                switch (nref.Which)
                {
                    case Either<TreeTreeReference, TreeBlobReference>.Selected.Left:
                        Console.WriteLine("tree {1}: {0}{2}/", new string('_', depth * 2), nref.Left.TreeID.ToString(firstLength: 7), nref.Left.Name);
                        RecursivePrint(nref.Left.TreeID, trees, depth + 1);
                        break;
                    case Either<TreeTreeReference, TreeBlobReference>.Selected.Right:
                        Console.WriteLine("blob {1}: {0}{2}", new string('_', depth * 2), nref.Right.BlobID.ToString(firstLength: 7), nref.Right.Name);
                        break;
                }
            }
        }

        private async Task createTrees()
        {
            PersistingBlob pbHeader = new PersistingBlob("<div>Header</div>".ToStream());

            sblobs = await blrepo.PersistBlobs(pbHeader);

            trSection1 = new TreeNode.Builder();
            trSection2 = new TreeNode.Builder();
            trSection3 = new TreeNode.Builder();
            trImages = new TreeNode.Builder();
            trCSS = new TreeNode.Builder();
            trJS = new TreeNode.Builder();

            trPages = new TreeNode.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("section1", trSection1.ID),
                    new TreeTreeReference.Builder("section2", trSection2.ID),
                    new TreeTreeReference.Builder("section3", trSection3.ID)
                },
                new List<TreeBlobReference>(0)
            );

            trContent = new TreeNode.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("images", trImages.ID),
                    new TreeTreeReference.Builder("css", trCSS.ID),
                    new TreeTreeReference.Builder("js", trJS.ID)
                },
                new List<TreeBlobReference>(0)
            );

            trTemplate = new TreeNode.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("header", sblobs[0].Value.ID)
                }
            );

            trRoot = new TreeNode.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("template", trTemplate.ID),
                    new TreeTreeReference.Builder("content", trContent.ID),
                    new TreeTreeReference.Builder("pages", trPages.ID)
                },
                new List<TreeBlobReference>(0)
            );

            rootId = trRoot.ID;
            trees = new ImmutableContainer<TreeID, TreeNode>(tr => tr.ID, trSection1, trSection2, trSection3, trPages, trImages, trCSS, trJS, trContent, trTemplate, trRoot);
            RecursivePrint(trRoot.ID, trees);
        }

        internal async Task PersistTreeTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);
        }

        internal async Task GetTreesTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);

            // Now retrieve those trees:
            var treeIDs = trees.Keys.ToArray();
            var etrGot = await trrepo.GetTrees(treeIDs);

            assertNoErrors(etrGot[0]);
            Assert.AreEqual(treeIDs[0], etrGot[0].Value.ID);
        }

        internal async Task GetTreeTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);

            // Retrieve a single tree node:
            var eroot = await trrepo.GetTree(rootId);
            var root = assertNoErrors(eroot);
            Assert.AreEqual(trRoot.ID, root.ID);

            // Retrieve a single tree node:
            var etmpl = await trrepo.GetTree(trTemplate.ID);
            var tmpl = assertNoErrors(etmpl);
            Assert.AreEqual(trTemplate.ID, tmpl.ID);
        }

        internal async Task GetTreeIDByPathTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);

            // Retrieve a single TreeID mapping:
            var rootPath = (CanonicalTreePath)"/";
            var erootMapping = await trrepo.GetTreeIDByPath(new TreeTreePath(rootId, rootPath));
            var rootMapping = assertNoErrors(erootMapping);

            Assert.IsTrue(rootMapping != null);
            Assert.AreEqual(trRoot.ID, rootMapping.TreeID);
            Assert.AreEqual(rootPath.ToString(), rootMapping.Path.Path.ToString());
        }

        internal async Task GetTreeIDsByPathsTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);

            // Retrieve some TreeID mappings by Paths:
            var rootPath = (CanonicalTreePath)"/";
            var tmplPath = (CanonicalTreePath)"/template/";
            var rootMappings = await trrepo.GetTreeIDsByPaths(
                new TreeTreePath(rootId, rootPath),
                new TreeTreePath(rootId, tmplPath)
            );

            Assert.IsTrue(rootMappings != null);
            Assert.AreEqual(2, rootMappings.Length);

            assertNoErrors(rootMappings[0]);
            Assert.IsTrue(rootMappings[0].Value.TreeID.HasValue);
            Assert.AreEqual(trRoot.ID, rootMappings[0].Value.TreeID.Value);
            Assert.AreEqual(rootPath.ToString(), rootMappings[0].Value.Path.Path.ToString());

            assertNoErrors(rootMappings[1]);
            Assert.IsTrue(rootMappings[1].Value.TreeID.HasValue);
            Assert.AreEqual(trTemplate.ID, rootMappings[1].Value.TreeID.Value);
            Assert.AreEqual(tmplPath.ToString(), rootMappings[1].Value.Path.Path.ToString());
        }

        internal async Task GetTreeRecursivelyTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);

            // Retrieve the tree recursively:
            var erec = await trrepo.GetTreeRecursively(rootId);
            var rec = assertNoErrors(erec);

            Assert.AreEqual(rootId, rec.RootID);
            Assert.AreEqual(trees.Count, rec.Trees.Count);
        }

        internal async Task GetTreeRecursivelyFromPathTest()
        {
            await createTrees();

            var ept = await trrepo.PersistTree(rootId, trees);
            assertNoErrors(ept);

            // Retrieve a subtree recursively by path:
            var erec = await trrepo.GetTreeRecursivelyFromPath(new TreeTreePath(rootId, (CanonicalTreePath)"/template/"));
            var rec = assertNoErrors(erec);

            Assert.AreEqual(trTemplate.ID, rec.RootID);
            Assert.AreEqual(trTemplate.Trees.Length + 1, rec.Trees.Count);
        }

        internal async Task DeleteTreeRecursivelyTest()
        {
            await createTrees();

            await trrepo.PersistTree(rootId, trees);

            // Retrieve a subtree recursively by path:
            var erec = await trrepo.DeleteTreeRecursively(rootId);
            assertNoErrors(erec);
        }

        internal async Task GetTreeNodesAlongPath()
        {
            await createTrees();

            await trrepo.PersistTree(rootId, trees);

            var etrnodes = await trrepo.GetTreeNodesAlongPath(new TreeTreePath(rootId, (CanonicalTreePath)"/content/images/"));
            assertNoErrors(etrnodes);
            
            var trnodes = etrnodes.Value;
            Assert.AreEqual(3, trnodes.Length);
            Assert.AreEqual(rootId, trnodes[0].ID);
            Assert.AreEqual(trContent.ID, trnodes[1].ID);
            Assert.AreEqual(trImages.ID, trnodes[2].ID);
        }
    }
}
