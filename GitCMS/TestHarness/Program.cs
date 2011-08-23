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

            //pr.TestAsynqQuery();
            pr.TestPersistCommit();

            Console.WriteLine("Press a key.");
            Console.ReadLine();
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

        void TestPersistCommit()
        {
            var db = new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=GitCMS;Integrated Security=SSPI");

            // Create a Blob:
            var bl = (Blob) new Blob.Builder()
            {
                Contents = Encoding.UTF8.GetBytes("Sample README content.")
            };
            // Asynchronously persist the blob to the database:
            db.AsynqNonQuery(new PersistBlob(bl));

            // Create a Tree:
            var tr = (Tree) new Tree.Builder()
            {
                Blobs = new TreeBlobReference[] {
                    new TreeBlobReference("README", bl.ID)
                },
                Trees = new TreeTreeReference[0]
            };

            var cm = (Commit) new Commit.Builder()
            {
                TreeID = tr.ID,
                Committer = "James Dunne <james.jdunne@gmail.com>",
                Author = "James Dunne <james.jdunne@gmail.com>",
                DateCommitted = DateTimeOffset.UtcNow,
                Message = "A commit message here."
            };



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
    }
}
