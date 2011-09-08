using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Containers;
using IVO.Definition.Exceptions;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.Diagnostics;

namespace IVO.Implementation.FileSystem
{
    public sealed class BlobRepository : IBlobRepository
    {
        private FileSystem system;

        internal FileSystem FileSystem { get { return system; } }

        /// <summary>
        /// TODO: tune this parameter for best async write buffer size.
        /// </summary>
        private const int largeBufferSize = 1048576;

        public BlobRepository(FileSystem system)
        {
            this.system = system;
        }

        #region IBlobRepository Members

        private DirectoryInfo CreateObjectsDirectory()
        {
            // Create the 'objects' subdirectory if it doesn't exist:
            DirectoryInfo objDir = new DirectoryInfo(System.IO.Path.Combine(system.Root.FullName, "objects"));
            if (!objDir.Exists)
                objDir.Create();
            return objDir;
        }

#if false
        private class WriteBlobAsyncState
        {
            public Blob Blob;
            public int Offset;
            public int BufferSize;
            public int BytesWritten;
            public FileStream OutputStream;
        }
#endif

        private async Task persistBlob(PersistingBlob blob, DirectoryInfo objDir)
        {
            BlobID blid;

            if (blob.ID.HasValue)
                blid = blob.ID.Value;
            else
            {
                // Asynchronously compute the BlobID from a stream of the contents:
                blid = await TaskEx.Run((Func<BlobID>)blob.ComputeID);
            }

            // Get the hex string of the BlobID:
            string id = blid.ToString();

            Debug.WriteLine(String.Format("Starting persistence of blob {0}", id));

            // Create the blob's subdirectory under 'objects':
            DirectoryInfo subdir = new DirectoryInfo(System.IO.Path.Combine(objDir.FullName, id.Substring(0, 2)));
            if (!subdir.Exists)
            {
                Debug.WriteLine(String.Format("Create directory '{0}'", subdir.FullName));
                subdir.Create();
            }

            // Create the blob's file path:
            string path = System.IO.Path.Combine(subdir.FullName, id.Substring(2));

            // Don't recreate an existing blob:
            if (File.Exists(path))
            {
                Debug.WriteLine(String.Format("Blob already exists at path '{0}'", path));
                goto verifyContents;
            }

            // Open a new stream to the source blob contents:
            using (var sr = blob.GetNewStream())
            {
                // Create a new file and set its length so we can asynchronously write to it:
                using (var tmpFi = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    tmpFi.SetLength(sr.Length);
                    tmpFi.Close();
                }

                // Determine the best buffer size to use for writing contents:
                int bufSize = Math.Min((int)sr.Length, largeBufferSize);

                // Open a new FileStream to asynchronously write the blob contents:
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Read, bufSize, useAsync: true))
                {
                    // Copy the contents asynchronously:
                    await sr.CopyToAsync(fs, bufSize);
#if false
                    var state = new WriteBlobAsyncState()
                    {
                        OutputStream = fs,
                        Blob = blob,
                        BufferSize = bufSize,
                        Offset = 0,
                        BytesWritten = Math.Min(bufSize, (int)sr.Length)
                    };

                    // Asynchronously write the blob contents to the file:
                    do
                    {
                        Debug.WriteLine(String.Format("Awaiting  write to '{0}', offs = {1,8}, count = {2,8}", state.OutputStream.Name, state.Offset, state.BytesWritten));
                        try
                        {
                            await Task.Factory.FromAsync(
                                state.OutputStream.BeginWrite,
                                state.OutputStream.EndWrite,
                                arg1: state.Blob.Contents,      // byte[] buffer
                                arg2: state.Offset,             // int offset
                                arg3: state.BytesWritten,       // int count
                                state: state
                            );
                            Debug.WriteLine(String.Format("Completed write to '{0}', offs = {1,8}, count = {2,8}", state.OutputStream.Name, state.Offset, state.BytesWritten));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(String.Format("EXPECTION while writing to '{0}', offs = {1,8}, count = {2,8}:" + Environment.NewLine + ex.ToString(), state.OutputStream.Name, state.Offset, state.BytesWritten));
                        }

                        // Move the offset up by however much was written:
                        state.Offset += state.BytesWritten;
                        // Now calculate the new amount to write:
                        state.BytesWritten = Math.Min(state.BufferSize, state.Blob.Contents.Length - state.Offset);
                    } while (state.BytesWritten > 0);

                    Debug.WriteLine(String.Format("Done persisting to '{0}'", state.OutputStream.Name));

                    fs.Close();
#endif
                }
            }

        verifyContents:
#if VerifyContents
                // This kills performance, naturally.
                // Only used for debugging.
                Debug.WriteLine(String.Format("Verifying contents of '{0}'", path));

                byte[] readContents = File.ReadAllBytes(path);
                    
                if (readContents.Length != blob.Contents.Length)
                    throw new Exception("Written length != blob length");

                for (int x = 0; x < readContents.Length; ++x)
                    if (readContents[x] != blob.Contents[x])
                        throw new Exception(String.Format("Written byte at position {0} != blob byte", x));
#else
            return;
#endif
        }

        public async Task<IStreamedBlob[]> PersistBlobs(params PersistingBlob[] blobs)
        {
            if (blobs == null) throw new ArgumentNullException("blobs");
            if (blobs.Length == 0) return new IStreamedBlob[0];

            // TODO: implement a filesystem lock?

            DirectoryInfo objDir = CreateObjectsDirectory();

            // Persist each blob to the 'objects' folder asynchronously:
            Task[] persistTasks = new Task[blobs.Length];
            IStreamedBlob[] streamedBlobs = new IStreamedBlob[blobs.Length];
            for (int i = 0; i < blobs.Length; ++i)
            {
                var blob = blobs[i];
                // Start a new task to contain each asynchronous task so that they can start up in parallel with one another:
                persistTasks[i] = TaskEx.RunEx(() => persistBlob(blob, objDir));
                streamedBlobs[i] = new StreamedBlob(this, blob.ComputedID);
            }

            Debug.WriteLine("Awaiting all persistence tasks...");
            await TaskEx.WhenAll(persistTasks);

            Debug.WriteLine("All completed.");

            // Return the final immutable container:
            return streamedBlobs;
        }

        private void deleteBlob(BlobID id, DirectoryInfo objDir)
        {
            string idStr = id.ToString();
            string path = System.IO.Path.Combine(objDir.FullName, idStr.Substring(0, 2), idStr.Substring(2));

            if (File.Exists(path))
                File.Delete(path);

            return;
        }

        public async Task<BlobID[]> DeleteBlobs(params BlobID[] ids)
        {
            if (ids == null) throw new ArgumentNullException("ids");
            if (ids.Length == 0) return ids;

            DirectoryInfo objDir = CreateObjectsDirectory();

            // Delete each blob asynchronously:
            Task[] deleteTasks = new Task[ids.Length];
            for (int i = 0; i < ids.Length; ++i)
            {
                BlobID id = ids[i];
                deleteTasks[i] = TaskEx.Run(() => deleteBlob(id, objDir));
            }

            // Wait for all blobs to be deleted:
            await TaskEx.WhenAll(deleteTasks);

            // TODO: Run through all the 'objects' directories and prune empty ones.
            // Too eager? Could cause conflicts with other threads.

            return ids;
        }

        public Task<IStreamedBlob[]> GetBlobs(params BlobID[] ids)
        {
            throw new NotImplementedException();
        }

        public Task<TreePathStreamedBlob[]> GetBlobsByTreePaths(TreePath[] treePath)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
