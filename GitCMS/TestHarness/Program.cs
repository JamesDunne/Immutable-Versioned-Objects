using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCMS.Data.Queries;
using GitCMS.Definition.Models;
using Asynq;
using System.Threading.Tasks;
using GitCMS.Data.Persists;
using GitCMS.Definition.Containers;
using GitCMS.Definition.Repositories;
using GitCMS.Implementation.SQL;

namespace TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            var pr = new Program();

            //pr.TestIDGeneration();
            //pr.TestTreeIDEquivalencies();
            //pr.TestAsynqQuery();
            //pr.TestPersistBlob();
            //pr.TestQueryBlobs();

            TreeID rootid = pr.TestPersistTree();
            pr.TestRetrieveTreeRecursively(rootid);

            pr.TestCreateCommit(rootid);

            Console.WriteLine("Press a key.");
            Console.ReadLine();
        }

        DataContext getDataContext()
        {
            return new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=GitCMS;Integrated Security=SSPI");
        }

        void TestIDGeneration()
        {
            // Create a Blob:
            Blob bl = new Blob.Builder(
                pContents: Encoding.UTF8.GetBytes("Sample README content.")
            );
            Console.WriteLine(bl.ID.ToString());

            // Create a Tree:
            Tree tr = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("README", bl.ID)
                }
            );
            Console.WriteLine(tr.ID.ToString());

            // Create a Commit:
            Commit cm = new Commit.Builder(
                pParents: new List<CommitID>(0),
                pTreeID: tr.ID,
                pCommitter: "James Dunne <james.jdunne@gmail.com>",
                pDateCommitted: DateTimeOffset.Now,
                pMessage: "A commit message here."
            );
            Console.WriteLine(cm.ID.ToString());
        }

        void TestTreeIDEquivalencies()
        {
            byte[] tidBuf = new byte[TreeID.ByteArrayLength];
            byte[] bidBuf = new byte[BlobID.ByteArrayLength];

            // Obviously never generate random fake SHA-1s. This is purely for testing.
            var rnd = new Random(1011);
            rnd.NextBytes(tidBuf);
            rnd.NextBytes(bidBuf);

            // Create several variants of the same Tree:
            Tree tr0 = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("src", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("Content", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("Images", new TreeID(tidBuf)),
                },
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("main", new BlobID(bidBuf)),
                    new TreeBlobReference.Builder("README", new BlobID(bidBuf)),
                }
            );
            Console.WriteLine(tr0.ID.ToString());

            Tree tr1 = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("Content", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("src", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("Images", new TreeID(tidBuf)),
                },
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("README", new BlobID(bidBuf)),
                    new TreeBlobReference.Builder("main", new BlobID(bidBuf)),
                }
            );
            Console.WriteLine(tr1.ID.ToString());

            Tree tr2 = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("Content", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("Images", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("src", new TreeID(tidBuf)),
                },
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("README", new BlobID(bidBuf)),
                    new TreeBlobReference.Builder("main", new BlobID(bidBuf)),
                }
            );
            Console.WriteLine(tr2.ID.ToString());

            // Create a different tree:
            Tree tr3 = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("src", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("Images", new TreeID(tidBuf)),
                    new TreeTreeReference.Builder("Content", new TreeID(tidBuf)),
                },
                new List<TreeBlobReference> {
                    // Capitalization of name makes a difference:
                    new TreeBlobReference.Builder("Readme", new BlobID(bidBuf)),
                    new TreeBlobReference.Builder("main", new BlobID(bidBuf)),
                }
            );
            Console.WriteLine(tr3.ID.ToString());
        }

        void TestAsynqQuery()
        {
            var db = getDataContext();

            var q = new QueryCommit(new CommitID(new byte[20]));

            const int max = 500;

            Task<Commit>[] results = new Task<Commit>[max];
            for (int i = 0; i < max; ++i)
            {
                // Asynchronously execute the query:
                results[i] = db.AsynqMulti(q);
            }

            for (int i = 0; i < max; ++i)
            {
                results[i].Wait();
            }
        }

        void TestPersistBlob()
        {
            // Create a Blob:
            Blob bl = new Blob.Builder(Encoding.UTF8.GetBytes("Sample README content."));
            Console.WriteLine(bl.ID.ToString());

            var db = getDataContext();

            // Check if the Blob exists by this ID:
            var getBlob = db.AsynqSingle(new QueryBlob(bl.ID));
            getBlob.Wait();

            if (getBlob.Result == null)
            {
                // It does not, persist it:
                Console.WriteLine("PERSIST {0}", bl.ID);

                var persistBlob = db.AsynqNonQuery(new PersistBlob(bl));
                persistBlob.Wait();

                Console.WriteLine("{0} rows affected", persistBlob.Result);
            }
            else
            {
                Console.WriteLine("Blob retrieved {0}", getBlob.Result.ID.ToString());

                // Destroy the blob:
                Console.WriteLine("DELETE {0}", getBlob.Result.ID);

                var destroyBlob = db.AsynqNonQuery(new DestroyBlob(getBlob.Result.ID));
                destroyBlob.Wait();

                Console.WriteLine("{0} rows affected", destroyBlob.Result);
            }
        }

        void TestQueryBlobs()
        {
            // Create some Blobs:
            Blob bl0 = new Blob.Builder(Encoding.UTF8.GetBytes("Sample README content."));
            Console.WriteLine(bl0.ID.ToString());

            Blob bl1 = new Blob.Builder(Encoding.UTF8.GetBytes("Sample content."));
            Console.WriteLine(bl1.ID.ToString());

            BlobContainer blobs = new BlobContainer(bl0, bl1);
            var db = getDataContext();

            // Check which blobs exist already:
            var qBlobs = db.AsynqMulti(new QueryBlobsExist(bl0.ID, bl1.ID), expectedCapacity: blobs.Count);
            qBlobs.Wait();

            // Find the blobs to persist:
            var blobIDsToPersist = blobs.Keys.Except(qBlobs.Result).ToArray();

            // Persist each blob asynchronously:
            Task<Blob>[] persists = new Task<Blob>[blobIDsToPersist.Length];
            for (int i = 0; i < blobIDsToPersist.Length; ++i)
            {
                BlobID id = blobIDsToPersist[i];

                Console.WriteLine("PERSIST {0}", id.ToString());
                persists[i] = db.AsynqNonQuery(new PersistBlob(blobs[id]));
            }

            Console.WriteLine("Waiting for persists...");
            Task.WaitAll(persists);
            Console.WriteLine("Complete.");
        }

        TreeID TestPersistTree()
        {
            // Create a Blob:
            Blob bl = new Blob.Builder(Encoding.UTF8.GetBytes("Sample README content."));
            BlobContainer blobs = new BlobContainer(bl);

            // Create a Tree:
            Tree trPersists = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("HelloWorld.cs", bl.ID),
                    new TreeBlobReference.Builder("PersistBlob.cs", bl.ID),
                    new TreeBlobReference.Builder("PersistTree.cs", bl.ID),
                }
            );

            Tree trSrc = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("Persists", trPersists.ID),
                },
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("blah", bl.ID),
                }
            );

            Tree trData = new Tree.Builder(
                new List<TreeTreeReference>(0),
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("myTest.xml", bl.ID),
                    new TreeBlobReference.Builder("myTest2.xml", bl.ID),
                    new TreeBlobReference.Builder("myTest3.xml", bl.ID),
                }
            );

            Tree trRoot = new Tree.Builder(
                new List<TreeTreeReference> {
                    new TreeTreeReference.Builder("CRAP", trSrc.ID),
                    new TreeTreeReference.Builder("data", trData.ID),
                },
                new List<TreeBlobReference> {
                    new TreeBlobReference.Builder("README", bl.ID),
                    new TreeBlobReference.Builder("main.xml", bl.ID),
                    new TreeBlobReference.Builder("test.xml", bl.ID),
                }
            );

            TreeContainer trees = new TreeContainer(trRoot, trSrc, trData, trPersists);

            var db = getDataContext();

            // Start queries to check what exists already:
            var existBlobs = db.AsynqMulti(new QueryBlobsExist(blobs.Keys), expectedCapacity: blobs.Count);
            var existTrees = db.AsynqMulti(new QueryTreesExist(trees.Keys), expectedCapacity: trees.Count);

            // First, persist blobs that don't exist:
            Console.WriteLine("Waiting for blob exists...");
            existBlobs.Wait();

            BlobID[] blobIDsToPersist = blobs.Keys.Except(existBlobs.Result).ToArray();

            // Blobs may be persisted in any order; there are no dependencies between blobs:
            Task<Blob>[] blobPersists = new Task<Blob>[blobIDsToPersist.Length];
            for (int i = 0; i < blobIDsToPersist.Length; ++i)
            {
                BlobID id = blobIDsToPersist[i];

                Console.WriteLine("PERSIST blob {0}", id.ToString());
                blobPersists[i] = db.AsynqNonQuery(new PersistBlob(blobs[id]));
            }

            if (blobPersists.Length > 0)
            {
                Console.WriteLine("Waiting for blob persists...");
                Task.WaitAll(blobPersists);
            }

            // Next, persist trees that don't exist:
            Console.WriteLine("Waiting for tree exists...");
            existTrees.Wait();

            // Trees must be created in dependency order!
            HashSet<TreeID> treeIDsToPersistSet = new HashSet<TreeID>(trees.Keys.Except(existTrees.Result));
            Stack<TreeID> treeIDsToPersist = new Stack<TreeID>(treeIDsToPersistSet.Count);

            // Run through trees from root to leaf:
            Queue<TreeID> treeQueue = new Queue<TreeID>();
            treeQueue.Enqueue(trRoot.ID);
            while (treeQueue.Count > 0)
            {
                TreeID trID = treeQueue.Dequeue();
                if (treeIDsToPersistSet.Contains(trID))
                    treeIDsToPersist.Push(trID);

                Tree tr = trees[trID];
                if (tr.Trees == null) continue;

                foreach (TreeTreeReference r in tr.Trees)
                {
                    treeQueue.Enqueue(r.TreeID);
                }
            }

            // Asynchronously persist the trees in dependency order:
            // FIXME: asynchronous fan-out per depth level
            Task<Tree> waiter = null, runner = null;
            while (treeIDsToPersist.Count > 0)
            {
                TreeID id = treeIDsToPersist.Pop();

                Console.WriteLine("PERSIST tree {0}", id.ToString());

                if (runner == null) waiter = runner = db.AsynqNonQuery(new PersistTree(trees[id]));
                else runner = runner.ContinueWith(r => db.AsynqNonQuery(new PersistTree(trees[id]))).Unwrap();
            }

            if (runner != null)
            {
                Console.WriteLine("Waiting for tree persists...");
                runner.Wait();
            }

            Console.WriteLine("Complete");

            Console.WriteLine("Root TreeID = {0}", trRoot.ID);

            return trRoot.ID;
        }

        private static Stack<T> newStack<T>(T initial, int initialCapacity = 10)
        {
            var stk = new Stack<T>();
            stk.Push(initial);
            return stk;
        }

        void TestRetrieveTreeRecursively(TreeID? id = null)
        {
            var db = getDataContext();

            TreeID rootid = id ?? new TreeID("a1fe342751e09fda968cfd0f1a1755e386f494f8");
            Console.WriteLine("Retrieving TreeID {0} recursively...", rootid);

            ITreeRepository repo = new TreeRepository(db);
            var treeTask = repo.RetrieveTreeRecursively(rootid);
            treeTask.Wait();

            // Recursively display trees:
            TreeContainer trees = treeTask.Result.Item2;

            RecursivePrint(trees, treeTask.Result.Item1, String.Empty);
        }

        void TestCreateCommit(TreeID? id)
        {
            TreeID rootid = id ?? new TreeID("a1fe342751e09fda968cfd0f1a1755e386f494f8");
            
            var db = getDataContext();

            ICommitRepository cmrepo = new CommitRepository(db);
            IRefRepository rfrepo = new RefRepository(db);

            var taskHead = cmrepo.GetCommitByRef("HEAD");
            taskHead.Wait();
            Commit parent = taskHead.Result;

            Commit cm = new Commit.Builder(
                pParents:       parent == null ? new List<CommitID>(0) : new List<CommitID> { parent.ID },
                pTreeID:        rootid,
                pCommitter:     "James Dunne <james.jdunne@gmail.com>",
                pDateCommitted: DateTimeOffset.Now,
                pMessage:       "Initial commit."
            );

            Console.WriteLine("CommitID {0}", cm.ID);

            // Persist the commit:
            Task<Commit> commitTask = cmrepo.PersistCommit(cm);
            // Then update the "HEAD" to point to the new commit:
            commitTask.ContinueWith(t => rfrepo.PersistRef(new Ref.Builder("HEAD", cm.ID))).Wait();
        }

        static void RecursivePrint(TreeContainer trees, TreeID treeID, string treeName, int depth = 1)
        {
            var tr = trees[treeID];
            if (depth == 1)
            {
                Console.WriteLine("tree {1}: {0}/", new string('_', (depth - 1) * 2), tr.ID.ToString().Substring(0, 12));
            }

            // Sort refs by name:
            var namedRefs = Tree.ComputeChildList(tr.Trees, tr.Blobs);

            foreach (var kv in namedRefs)
            {
                var nref = kv.Value;
                switch (nref.Which)
                {
                    case Either<TreeTreeReference,TreeBlobReference>.Selected.N1:
                        Console.WriteLine("tree {1}: {0}{2}/", new string('_', depth * 2), nref.N1.TreeID.ToString().Substring(0, 12), nref.N1.Name);
                        RecursivePrint(trees, nref.N1.TreeID, nref.N1.Name, depth + 1);
                        break;
                    case Either<TreeTreeReference,TreeBlobReference>.Selected.N2:
                        Console.WriteLine("blob {1}: {0}{2}", new string('_', depth * 2), nref.N2.BlobID.ToString().Substring(0, 12), nref.N2.Name);
                        break;
                }
            }
        }
    }
}