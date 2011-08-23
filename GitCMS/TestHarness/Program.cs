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
            pr.TestPersistBlob();

            Console.WriteLine("Press a key.");
            Console.ReadLine();
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
            var db = new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=GitCMS;Integrated Security=SSPI");

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

            var db = new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=GitCMS;Integrated Security=SSPI");

            // Check if the Blob exists by this ID:
            var getBlob = db.AsynqSingle(new QueryBlob(bl.ID));
            getBlob.Wait();

            if (getBlob.Result == null)
            {
                // It does not, persist it:
                var t = db.AsynqNonQuery(new PersistBlob(bl));
                t.Wait();

                Console.WriteLine("{0} rows affected", t.Result);
            }
            else
            {
                Console.WriteLine("Blob retrieved {0}", getBlob.Result.ID.ToString());
            }
        }
    }
}
