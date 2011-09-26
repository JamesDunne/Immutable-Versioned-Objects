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
using IVO.Definition.Errors;

namespace IVO.Implementation.FileSystem
{
    public sealed class CommitRepository : ICommitRepository
    {
        private FileSystem system;
        private TagRepository tgrepo;
        private RefRepository rfrepo;

        public CommitRepository(FileSystem system, TagRepository tgrepo = null, RefRepository rfrepo = null)
        {
            this.system = system;
            this.tgrepo = tgrepo ?? new TagRepository(system);
            this.rfrepo = rfrepo ?? new RefRepository(system);
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

        private async Task<Errorable<Commit>> getCommit(CommitID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return null;

            byte[] buf;
            int nr = 0;
            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                nr = await fs.ReadAsync(buf, 0, 16384).ConfigureAwait(continueOnCapturedContext: false);
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
                    cb.Parents.Add(CommitID.Parse(parent_commitid).Value);
                }

                // Set TreeID:
                if (line == null || !line.StartsWith("tree ")) return new CommitParseExpectedTreeError();
                cb.TreeID = TreeID.Parse(line.Substring("tree ".Length)).Value;

                // Set Committer:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("committer ")) return new CommitParseExpectedCommitterError();
                cb.Committer = line.Substring("committer ".Length);

                // Set DateCommitted:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("date ")) return new CommitParseExpectedDateError();

                // NOTE: date parsing will result in an inexact DateTimeOffset from what was created with, but it
                // is close enough because the SHA-1 hash is calculated using the DateTimeOffset.ToString(), so
                // only the ToString() representations of the DateTimeOffsets need to match.
                DateTimeOffset tmpDate;
                if (!DateTimeOffset.TryParse(line.Substring("date ".Length), out tmpDate))
                    return new CommitParseBadDateFormatError();
                cb.DateCommitted = tmpDate;

                // Skip empty line:
                line = sr.ReadLine();
                if (line == null || line.Length != 0) return new CommitParseExpectedBlankLineError();

                // Set Message:
                cb.Message = sr.ReadToEnd();
            }

            // Create the immutable Commit from the Builder:
            Commit cm = cb;
            // Validate the computed CommitID:
            if (cm.ID != id) return new ComputedCommitIDMismatchError();

            return cm;
        }

        private void deleteCommit(CommitID id)
        {
            FileInfo fi = system.getPathByID(id);
            if (!fi.Exists) return;

            fi.Delete();
        }

        #endregion

        public async Task<Errorable<Commit>> PersistCommit(Commit cm)
        {
            await TaskEx.Run(() => persistCommit(cm)).ConfigureAwait(continueOnCapturedContext: false);

            return cm;
        }

        public async Task<Errorable<CommitID>> DeleteCommit(CommitID id)
        {
            await TaskEx.Run(() => deleteCommit(id)).ConfigureAwait(continueOnCapturedContext: false);

            return id;
        }

        public Task<Errorable<Commit>> GetCommit(CommitID id)
        {
            return getCommit(id);
        }

        public async Task<Errorable<Tuple<Tag, Commit>>> GetCommitByTag(TagID id)
        {
            var etg = await tgrepo.GetTag(id).ConfigureAwait(continueOnCapturedContext: false);
            if (etg.HasErrors) return etg.Errors;
            
            Tag tg = etg.Value;

            var ecm = await getCommit(tg.CommitID).ConfigureAwait(continueOnCapturedContext: false);
            if (ecm.HasErrors) return ecm.Errors;

            Commit cm = ecm.Value;
            return new Tuple<Tag, Commit>(tg, cm);
        }

        public async Task<Errorable<Tuple<Tag, Commit>>> GetCommitByTagName(TagName tagName)
        {
            var etg = await tgrepo.GetTagByName(tagName).ConfigureAwait(continueOnCapturedContext: false);
            if (etg.HasErrors) return null;

            Tag tg = etg.Value;

            var ecm = await getCommit(tg.CommitID).ConfigureAwait(continueOnCapturedContext: false);
            if (ecm.HasErrors) return ecm.Errors;

            Commit cm = ecm.Value;
            return new Tuple<Tag, Commit>(tg, cm);
        }

        public async Task<Errorable<Tuple<Ref, Commit>>> GetCommitByRefName(RefName refName)
        {
            var erf = await rfrepo.GetRefByName(refName).ConfigureAwait(continueOnCapturedContext: false);
            if (erf.HasErrors) return erf.Errors;

            Ref rf = erf.Value;

            var ecm = await getCommit(rf.CommitID).ConfigureAwait(continueOnCapturedContext: false);
            if (ecm.HasErrors) return ecm.Errors;

            Commit cm = ecm.Value;
            return new Tuple<Ref, Commit>(rf, cm);
        }

        private async Task<Errorable<Commit[]>> getCommitsRecursively(CommitID id, int depthLevel, int depthMaximum)
        {
            // Get the current commit:
            var eroot = await getCommit(id).ConfigureAwait(continueOnCapturedContext: false);
            if (eroot.HasErrors) return eroot.Errors;
            
            var root = eroot.Value;
            var rootArr = new Commit[1] { root };

            // We have no parents:
            if (root.Parents.Length == 0)
                return rootArr;

            // This is the last depth level:
            if (depthLevel >= depthMaximum)
                return rootArr;

            // Recurse up the commit parents:
            Task<Errorable<Commit[]>>[] tasks = new Task<Errorable<Commit[]>>[root.Parents.Length];
            for (int i = 0; i < root.Parents.Length; ++i)
            {
                tasks[i] = getCommitsRecursively(root.Parents[i], depthLevel + 1, depthMaximum);
            }

            // Await all the tree retrievals:
            var allCommits = await TaskEx.WhenAll(tasks).ConfigureAwait(continueOnCapturedContext: false);

            // Roll up all the errors:
            ErrorContainer errors =
                (
                    from ecms in allCommits
                    where ecms.HasErrors
                    select ecms.Errors
                ).Aggregate(new ErrorContainer(), (acc, err) => acc + err);

            if (errors.HasAny) return errors;

            // Flatten out the tree arrays:
            var flattened =
                from ecms in allCommits
                from cm in ecms.Value
                select cm;

            // Return the final array:
            return rootArr.Concat(flattened).ToArray(allCommits.Sum(ca => ca.Value.Length) + 1);
        }

        public async Task<Errorable<CommitTree>> GetCommitTree(CommitID id, int depth = 10)
        {
            var eall = await getCommitsRecursively(id, 1, depth).ConfigureAwait(continueOnCapturedContext: false);
            if (eall.HasErrors) return eall.Errors;

            var all = eall.Value;

            // Return them (all[0] is the root):
            return new CommitTree(all[0].ID, new ImmutableContainer<CommitID, ICommit>(cm => cm.ID, all));
        }

        public async Task<Errorable<Tuple<Tag, CommitTree>>> GetCommitTreeByTagName(TagName tagName, int depth = 10)
        {
            var etg = await tgrepo.GetTagByName(tagName).ConfigureAwait(continueOnCapturedContext: false);
            if (etg.HasErrors) return null;

            Tag tg = etg.Value;
            var eall = await getCommitsRecursively(tg.CommitID, 1, depth).ConfigureAwait(continueOnCapturedContext: false);
            if (eall.HasErrors) return eall.Errors;

            var all = eall.Value;

            // Return them (all[0] is the root):
            return new Tuple<Tag, CommitTree>(
                tg,
                new CommitTree(all[0].ID, new ImmutableContainer<CommitID, ICommit>(cm => cm.ID, all))
            );
        }

        public async Task<Errorable<Tuple<Ref, CommitTree>>> GetCommitTreeByRefName(RefName refName, int depth = 10)
        {
            var erf = await rfrepo.GetRefByName(refName).ConfigureAwait(continueOnCapturedContext: false);
            if (erf.HasErrors) return erf.Errors;

            Ref rf = erf.Value;
            var eall = await getCommitsRecursively(rf.CommitID, 1, depth).ConfigureAwait(continueOnCapturedContext: false);
            if (eall.HasErrors) return eall.Errors;

            var all = eall.Value;

            // Return them (all[0] is the root):
            return new Tuple<Ref, CommitTree>(
                rf,
                new CommitTree(all[0].ID, new ImmutableContainer<CommitID, ICommit>(cm => cm.ID, all))
            );
        }
    }
}
