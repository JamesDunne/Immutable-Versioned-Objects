using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using IVO.Definition.Containers;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.Implementation.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace TestIVO.FileSystemTest
{
    [TestClass()]
    public class BlobRepositoryTest
    {
        private FileSystem getFileSystem()
        {
            string tmpPath = System.IO.Path.GetTempPath();
            string tmpRoot = System.IO.Path.Combine(tmpPath, "ivo");

            // Delete our temporary 'ivo' folder:
            var tmpdi = new DirectoryInfo(tmpRoot);
            if (tmpdi.Exists)
                tmpdi.Delete(recursive: true);

            FileSystem system = new FileSystem(new DirectoryInfo(tmpRoot));
            return system;
        }

        private Blob[] createBlobs(int numBlobs)
        {
            Random rnd = new Random(8191);

            Blob[] blobArr = new Blob[numBlobs];
            for (int i = 0; i < numBlobs; ++i)
            {
                // Create a random-sized (multiple of 64KB) temp buffer:
                int multiplier = rnd.Next(0, 8) + 12;
                byte[] tmp = new byte[multiplier * 65536];
                rnd.NextBytes(tmp);

                blobArr[i] = new Blob.Builder(tmp);
                tmp = null;
            }

            return blobArr;
        }

        /// <summary>
        ///A test for PersistBlobs
        ///</summary>
        [TestMethod()]
        public void PersistBlobsTest()
        {
            FileSystem system = getFileSystem();
            IBlobRepository blrepo = new BlobRepository(system);

            const int numBlobs = 32;

            // Create an immutable container that points to the new blobs:
            Console.WriteLine("Creating {0} random blobs...", numBlobs);
            var blobs = new ImmutableContainer<BlobID, Blob>(bl => bl.ID, createBlobs(numBlobs));

            // Now persist those blobs to the filesystem:
            Console.WriteLine("Persisting {0} random blobs...", numBlobs);
            Stopwatch sw = Stopwatch.StartNew();
            blrepo.PersistBlobs(blobs).Wait();
            Console.WriteLine("Completed in {0} ms, {1} bytes/sec", sw.ElapsedMilliseconds, blobs.Values.Sum(b => b.Contents.Length) * 1000d / sw.ElapsedMilliseconds);

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod]
        public void DeleteBlobsTest()
        {
            FileSystem system = getFileSystem();
            IBlobRepository blrepo = new BlobRepository(system);

            const int numBlobs = 32;
            var blobs = new ImmutableContainer<BlobID, Blob>(bl => bl.ID, createBlobs(numBlobs));
            
            // Persist the blobs:
            blrepo.PersistBlobs(blobs).Wait();

            // Delete the newly persisted blobs:
            blrepo.DeleteBlobs(blobs.Keys.ToArray(numBlobs)).Wait();

            // Clean up:
            if (system.Root.Exists)
                system.Root.Delete(recursive: true);
        }

        [TestMethod]
        public void StreamedBlobTest()
        {
            BlobID constructedID = new BlobID(), retrievedID = new BlobID();

            TaskEx.RunEx(async () =>
            {
                FileSystem system = getFileSystem();
                IBlobRepository blrepo = new BlobRepository(system);

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
                // Clean up:
                if (system.Root.Exists)
                    system.Root.Delete(recursive: true);
            }).Wait();

            Assert.AreEqual(constructedID, retrievedID);
            Console.WriteLine("Test complete");
        }
    }
}
