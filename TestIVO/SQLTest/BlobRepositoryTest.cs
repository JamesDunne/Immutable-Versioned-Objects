using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.SQL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Asynq;

namespace TestIVO.SQLTest
{
    [TestClass()]
    public class BlobRepositoryTest
    {
        private DataContext getDataContext()
        {
            return new DataContext(@"Data Source=.\SQLEXPRESS;Initial Catalog=IVO;Integrated Security=SSPI");
        }

        /// <summary>
        ///A test for PersistBlobs
        ///</summary>
        [TestMethod()]
        public void PersistBlobsTest()
        {
            DataContext db = getDataContext();
            IBlobRepository blrepo = new BlobRepository(db);

            const int numBlobs = 32;

            // Create an immutable container that points to the new blobs:
            Console.WriteLine("Creating {0} random blobs...", numBlobs);
            // TODO

            // Now persist those blobs to the filesystem:
            Console.WriteLine("Persisting {0} random blobs...", numBlobs);
            Stopwatch sw = Stopwatch.StartNew();
            //blrepo.PersistBlobs(new PersistingBlob()).Wait();
            Console.WriteLine("Completed in {0} ms, {1} bytes/sec", sw.ElapsedMilliseconds, blobs.Values.Sum(b => b.Contents.Length) * 1000d / sw.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DeleteBlobsTest()
        {
            var db = getDataContext();
            IBlobRepository blrepo = new BlobRepository(db);

            const int numBlobs = 32;
            var blobs = new ImmutableContainer<BlobID, Blob>(bl => bl.ID, createBlobs(numBlobs));
            
            // Persist the blobs:
            blrepo.PersistBlobs(blobs).Wait();

            // Delete the newly persisted blobs:
            blrepo.DeleteBlobs(blobs.Keys.ToArray(numBlobs)).Wait();
        }

        [TestMethod]
        public void StreamedBlobTest()
        {
            BlobID constructedID = new BlobID(), retrievedID = new BlobID();

            TaskEx.RunEx(async () =>
            {
                var db = getDataContext();
                IBlobRepository blrepo = new BlobRepository(db);

                const int numBlobs = 1;
                var blobs = new ImmutableContainer<BlobID, Blob>(bl => bl.ID, createBlobs(numBlobs));

                // Persist the blobs:
                blrepo.PersistBlobs(blobs).Wait();

                // Load a streamed blob:
                Console.WriteLine("Awaiting fetch of streamed blob.");
                constructedID = blobs.Values.First().ID;

                IStreamedBlob strbl = await blrepo.GetStreamedBlob(constructedID);

                Console.WriteLine("Awaiting ReadStream to complete...");
                await strbl.ReadStream(blsr =>
                {
                    Console.WriteLine("blob is {0} length", blsr.Length);

                    SHA1 sha1 = SHA1.Create();

                    const int bufsize = 8040 * 8 * 4;
                    byte[] dum = new byte[bufsize];
                    int count = bufsize;

                    try
                    {
                        while ((count = blsr.Read(dum, 0, bufsize)) > 0)
                        {
                            sha1.TransformBlock(dum, 0, count, null, 0);
                            //for (int i = 0; i < (count / 40) + ((count % 40) > 0 ? 1 : 0); ++i)
                            //{
                            //    Console.WriteLine(dum.ToHexString(i * 40, Math.Min(count - (i * 40), 40)));
                            //}
                        }
                        sha1.TransformFinalBlock(dum, 0, 0);

                        byte[] hash = sha1.Hash;

                        retrievedID = new BlobID(hash);
                        Console.WriteLine("SHA1 is {0}", hash.ToHexString(0, hash.Length));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                });

                Console.WriteLine("Cleaning up");
            }).Wait();

            Assert.AreEqual(constructedID, retrievedID);
            Console.WriteLine("Test complete");
        }
    }
}
