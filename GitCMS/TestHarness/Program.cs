using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCMS.Data.Queries;
using GitCMS.Definition.Models;
using Asynq;
using System.Threading.Tasks;
using GitCMS.Data.Persists;

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
            pr.TestPersistTree();

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
            var bl = (Blob) new Blob.Builder()
            {
                Contents = Encoding.UTF8.GetBytes("Sample README content.")
            };
            Console.WriteLine(bl.ID.ToString());

            // Create a Tree:
            var tr = (Tree) new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("README", bl.ID)
                },
                Trees = new TreeTreeReference[0]
            };
            Console.WriteLine(tr.ID.ToString());

            // Create a Commit:
            var cm = (Commit) new Commit.Builder()
            {
                Parents = new CommitID[0],
                TreeID = tr.ID,
                Committer = "James Dunne <james.jdunne@gmail.com>",
                Author = "James Dunne <james.jdunne@gmail.com>",
                DateCommitted = DateTimeOffset.UtcNow.Date,
                Message = "A commit message here."
            };
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
            var tr0 = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("main", new BlobID(bidBuf)),
                    new TreeBlobReference("README", new BlobID(bidBuf)),
                },
                Trees = new TreeTreeReference[] {
                    new TreeTreeReference("src", new TreeID(tidBuf)),
                    new TreeTreeReference("Content", new TreeID(tidBuf)),
                    new TreeTreeReference("Images", new TreeID(tidBuf)),
                },
            };
            Console.WriteLine(tr0.ID.ToString());

            var tr1 = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("README", new BlobID(bidBuf)),
                    new TreeBlobReference("main", new BlobID(bidBuf)),
                },
                Trees = new TreeTreeReference[] {
                    new TreeTreeReference("Content", new TreeID(tidBuf)),
                    new TreeTreeReference("src", new TreeID(tidBuf)),
                    new TreeTreeReference("Images", new TreeID(tidBuf)),
                },
            };
            Console.WriteLine(tr1.ID.ToString());

            var tr2 = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("README", new BlobID(bidBuf)),
                    new TreeBlobReference("main", new BlobID(bidBuf)),
                },
                Trees = new TreeTreeReference[] {
                    new TreeTreeReference("Content", new TreeID(tidBuf)),
                    new TreeTreeReference("Images", new TreeID(tidBuf)),
                    new TreeTreeReference("src", new TreeID(tidBuf)),
                },
            };
            Console.WriteLine(tr2.ID.ToString());

            // Create a different tree:
            var tr3 = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    // Capitalization of name makes a difference:
                    new TreeBlobReference("Readme", new BlobID(bidBuf)),
                    new TreeBlobReference("main", new BlobID(bidBuf)),
                },
                Trees = new TreeTreeReference[] {
                    new TreeTreeReference("src", new TreeID(tidBuf)),
                    new TreeTreeReference("Images", new TreeID(tidBuf)),
                    new TreeTreeReference("Content", new TreeID(tidBuf)),
                },
            };
            Console.WriteLine(tr3.ID.ToString());
        }

        void TestAsynqQuery()
        {
            var db = getDataContext();

            var q = new QueryCommit(new CommitID(new byte[20]));

            const int max = 500;

            Task<List<Commit>>[] results = new Task<List<Commit>>[max];
            for (int i = 0; i < max; ++i)
            {
                // Asynchronously execute the query:
                results[i] = db.AsynqMulti(q);
            }

            for (int i = 0; i < max; ++i)
            {
                results[i].Wait();

                Console.WriteLine("{0} rows", results[i].Result.Count);
            }
        }

        void TestPersistBlob()
        {
            // Create a Blob:
            var bl = (Blob) new Blob.Builder()
            {
                Contents = Encoding.UTF8.GetBytes("Sample README content.")
            };
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
            Dictionary<BlobID, Blob> blobs = new Dictionary<BlobID, Blob>();

            // Create some Blobs:
            var bl0 = (Blob)new Blob.Builder()
            {
                Contents = Encoding.UTF8.GetBytes("Sample README content.")
            };
            blobs.Add(bl0.ID, bl0);
            Console.WriteLine(bl0.ID.ToString());

            var bl1 = (Blob)new Blob.Builder()
            {
                Contents = Encoding.UTF8.GetBytes("Sample content.")
            };
            blobs.Add(bl1.ID, bl1);
            Console.WriteLine(bl1.ID.ToString());

            var db = getDataContext();

            // Check which blobs exist already:
            var qBlobs = db.AsynqMulti(new QueryBlobsExist(bl0.ID, bl1.ID), expectedCapacity: blobs.Count);
            qBlobs.Wait();

            // Find the blobs to persist:
            var blobIDsToPersist = blobs.Keys.Except(qBlobs.Result).ToArray();

            // Persist each blob asynchronously:
            Task<int>[] persists = new Task<int>[blobIDsToPersist.Length];
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

        void TestPersistTree()
        {
            Dictionary<BlobID, Blob> blobs = new Dictionary<BlobID, Blob>();
            Dictionary<TreeID, Tree> trees = new Dictionary<TreeID, Tree>();

            // Create a Blob:
            var bl = (Blob)new Blob.Builder()
            {
                Contents = Encoding.UTF8.GetBytes("Sample README content.")
            };
            blobs.Add(bl.ID, bl);

            // Create a Tree:
            var trPersists = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("HelloWorld.cs", bl.ID),
                    new TreeBlobReference("PersistBlob.cs", bl.ID),
                    new TreeBlobReference("PersistTree.cs", bl.ID),
                },
                Trees = new TreeTreeReference[0]
            };
            trees.Add(trPersists.ID, trPersists);

            var trSrc = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("blah", bl.ID),
                },
                Trees = new TreeTreeReference[] {
                    new TreeTreeReference("Persists", trPersists.ID),
                }
            };
            trees.Add(trSrc.ID, trSrc);

            var trData = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("myTest.xml", bl.ID),
                    new TreeBlobReference("myTest2.xml", bl.ID),
                    new TreeBlobReference("myTest3.xml", bl.ID),
                },
                Trees = new TreeTreeReference[0]
            };
            trees.Add(trData.ID, trData);

            var trRoot = (Tree)new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("README", bl.ID),
                    new TreeBlobReference("main.xml", bl.ID),
                    new TreeBlobReference("test.xml", bl.ID),
                },
                Trees = new TreeTreeReference[] {
                    new TreeTreeReference("src", trSrc.ID),
                    new TreeTreeReference("data", trData.ID),
                },
            };
            trees.Add(trRoot.ID, trRoot);

            var db = getDataContext();

            // Start queries to check what exists already:
            var existBlobs = db.AsynqMulti(new QueryBlobsExist(blobs.Keys), expectedCapacity: blobs.Count);
            var existTrees = db.AsynqMulti(new QueryTreesExist(trees.Keys), expectedCapacity: trees.Count);

            // First, persist blobs that don't exist:
            Console.WriteLine("Waiting for blob exists...");
            existBlobs.Wait();

            BlobID[] blobIDsToPersist = blobs.Keys.Except(existBlobs.Result).ToArray();

            // Blobs may be persisted in any order; there are no dependencies between blobs:
            Task<int>[] blobPersists = new Task<int>[blobIDsToPersist.Length];
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
            Task<int> waiter = null, runner = null;
            while (treeIDsToPersist.Count > 0)
            {
                TreeID id = treeIDsToPersist.Pop();

                Console.WriteLine("PERSIST tree {0}", id.ToString());

                if (runner == null) waiter = runner = db.AsynqNonQuery(new PersistTree(trees[id]));
                else runner = runner.ContinueWith(r => db.AsynqNonQuery(new PersistTree(trees[id]))).Unwrap();
            }

            if (waiter != null)
            {
                Console.WriteLine("Waiting for tree persists...");
                waiter.Wait();
            }

            Console.WriteLine("Complete");

            Console.WriteLine("Root TreeID = {0}", trRoot.ID);
        }
    }
}