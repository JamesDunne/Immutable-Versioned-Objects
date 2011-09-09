using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using IVO.TestSupport;

namespace TestIVO.CommonTest
{
    class StreamedBlobRepositoryTestMethods
    {
        private IStreamedBlobRepository blrepo;

        internal StreamedBlobRepositoryTestMethods(IStreamedBlobRepository blrepo)
        {
            this.blrepo = blrepo;
        }

        private PersistingBlob[] createBlobs(int numBlobs)
        {
            PersistingBlob[] blobs = new PersistingBlob[numBlobs];
            for (int i = 0; i < numBlobs; ++i)
            {
                string tmpFileName = System.IO.Path.GetTempFileName();
                using (var fs = new FileStream(tmpFileName, FileMode.Open, FileAccess.Write, FileShare.Read, 1048576, true))
                {
                    new RandomDataStream(1048576 * 2).CopyTo(fs);

                    blobs[i] = new PersistingBlob(() => new FileStream(tmpFileName, FileMode.Open, FileAccess.Read, FileShare.Read));
                }
            }
            return blobs;
        }

        internal async Task PersistBlobsTest()
        {
            const int numBlobs = 16;

            // Create an immutable container that points to the new blobs:
            Console.WriteLine("Creating {0} random blobs...", numBlobs);
            PersistingBlob[] blobs = createBlobs(numBlobs);

            // Now persist those blobs to the filesystem:
            Console.WriteLine("Persisting {0} random blobs...", numBlobs);
            Stopwatch sw = Stopwatch.StartNew();
            var streamedBlobs = await blrepo.PersistBlobs(blobs);

            Console.WriteLine("Completed in {0} ms, {1} bytes/sec", sw.ElapsedMilliseconds, streamedBlobs.Sum(b => b.Length) * 1000d / sw.ElapsedMilliseconds);
        }

        internal async Task DeleteBlobsTest()
        {
            const int numBlobs = 16;

            // Persist the blobs:
            PersistingBlob[] blobs = createBlobs(numBlobs);
            var streamedBlobs = await blrepo.PersistBlobs(blobs);

            // Delete the newly persisted blobs:
            await blrepo.DeleteBlobs(streamedBlobs.Select(bl => bl.ID).ToArray(numBlobs));
        }

        internal async Task GetBlobsTest()
        {
            const int numBlobs = 16;

            // Create an immutable container that points to the new blobs:
            Console.WriteLine("Creating {0} random blobs...", numBlobs);
            PersistingBlob[] blobs = createBlobs(numBlobs);

            // Now persist those blobs to the filesystem:
            Console.WriteLine("Persisting {0} random blobs...", numBlobs);
            Stopwatch sw = Stopwatch.StartNew();
            var streamedBlobs = await blrepo.PersistBlobs(blobs);

            Console.WriteLine("Completed in {0} ms, {1} bytes/sec", sw.ElapsedMilliseconds, streamedBlobs.Sum(b => b.Length) * 1000d / sw.ElapsedMilliseconds);

            // Get the blobs:
            var getBlobs = await blrepo.GetBlobs(blobs.Select(bl => bl.ComputedID).ToArray(blobs.Length));
            for (int i = 0; i < getBlobs.Length; ++i)
            {
                Console.WriteLine("{0}", getBlobs[i].ID);
            }
        }

        internal async Task StreamedBlobTest()
        {
            const int numBlobs = 1;
            PersistingBlob[] blobs = createBlobs(numBlobs);

            // Persist the blobs:
            var streamedBlobs = await blrepo.PersistBlobs(blobs);

            // Load a streamed blob:
            Console.WriteLine("Awaiting fetch of streamed blob.");

            Console.WriteLine("Awaiting ReadStream to complete...");
            await streamedBlobs[0].ReadStream(blsr =>
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

                    BlobID retrievedID = new BlobID(hash);
                    Console.WriteLine("SHA1 is {0}", hash.ToHexString(0, hash.Length));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
        }
    }
}
