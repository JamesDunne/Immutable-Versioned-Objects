using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Containers;
using IVO.Definition.Repositories;
using System.IO;
using System.Diagnostics;
using IVO.Definition.Exceptions;

namespace IVO.Implementation.FileSystem
{
    public sealed class CommitRepository : ICommitRepository
    {
        private FileSystem system;

        public CommitRepository(FileSystem system)
        {
            this.system = system;
        }

        #region Private details

        private void persistCommit(Commit cm)
        {
            FileInfo fi = system.getPathByID(cm.ID);
            if (fi.Exists) return;

            // Create directory if it doesn't exist:
            if (!fi.Directory.Exists)
            {
                Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                fi.Directory.Create();
            }

            // Write the commit contents to the file:
            using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                Debug.WriteLine(String.Format("New COMMIT '{0}'", fi.FullName));
                cm.WriteTo(fs);
            }
        }

        private async Task<Commit> getCommit(CommitID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return null;

            byte[] buf;
            int nr = 0;
            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                nr = await fs.ReadAsync(buf, 0, 16384);
                if (nr >= 16384)
                {
                    // My, what a large commit you have!
                    throw new NotSupportedException();
                }
            }

            Commit.Builder cb = new Commit.Builder()
            {
                Parents = new List<CommitID>()
            };

            // Parse the Commit:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line;

                // Read the list of parent CommitIDs:
                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.StartsWith("parent ")) break;

                    string parent_commitid = line.Substring("parent ".Length);
                    cb.Parents.Add(new CommitID(parent_commitid));
                }

                // Set TreeID:
                if (line == null || !line.StartsWith("tree ")) throw new ObjectParseException("While parsing a commit, expected: 'tree'");
                cb.TreeID = new TreeID(line.Substring("tree ".Length));

                // Set Committer:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("committer ")) throw new ObjectParseException("While parsing a commit, expected: 'committer'");
                cb.Committer = line.Substring("committer ".Length);

                // Set DateCommitted:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("date ")) throw new ObjectParseException("While parsing a commit, expected: 'date'");
                cb.DateCommitted = DateTimeOffset.Parse(line.Substring("date ".Length));

                // Skip empty line:
                line = sr.ReadLine();
                if (line == null || line.Length != 0) throw new ObjectParseException("While parsing a commit, expected blank line");

                // Set Message:
                cb.Message = sr.ReadToEnd();
            }

            // Create the immutable Commit from the Builder:
            Commit cm = cb;
            // Validate the computed CommitID:
            if (cm.ID != id) throw new CommitIDMismatchException(cm.ID, id);

            return cm;
        }

        private void deleteCommit(CommitID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return;

            fi.Delete();
        }

        #endregion

        public async Task<Commit> PersistCommit(Commit cm)
        {
            await TaskEx.Run(() => persistCommit(cm));

            return cm;
        }

        public async Task<CommitID> DeleteCommit(CommitID id)
        {
            await TaskEx.Run(() => deleteCommit(id));

            return id;
        }

        public Task<Commit> GetCommit(CommitID id)
        {
            return getCommit(id);
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTag(TagID id)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<Tag, Commit>> GetCommitByTagName(string tagName)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<Ref, Commit>> GetCommitByRef(string refName)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<CommitID, ImmutableContainer<CommitID, ICommit>>> GetCommitTree(CommitID id, int depth = 10)
        {
            throw new NotImplementedException();
        }
    }
}
