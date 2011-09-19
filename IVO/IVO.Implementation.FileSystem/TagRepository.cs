using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using System.IO;
using System.Diagnostics;
using IVO.Definition.Exceptions;

namespace IVO.Implementation.FileSystem
{
    public sealed class TagRepository : ITagRepository
    {
        private FileSystem system;

        public TagRepository(FileSystem system)
        {
            this.system = system;
        }

        #region Private details

        private async Task persistTag(Tag tg)
        {
            FileInfo fiTracker = system.getTagPathByTagName(tg.Name);
            if (fiTracker.Exists)
            {
                TagID? tgID = await getTagIDByName(tg.Name).ConfigureAwait(continueOnCapturedContext: false);
                if (tgID.HasValue)
                {
                    var gtg = await getTag(tgID.Value).ConfigureAwait(continueOnCapturedContext: false);
                    if (gtg.Name != tg.Name)
                        throw new InvalidOperationException();

                    // FIXME: assuming rest of the contents are the same.
                }
                return;
            }

            FileInfo fi = system.getPathByID(tg.ID);
            if (fi.Exists)
            {
                return;
            }

            // Create directory if it doesn't exist:
            if (!fi.Directory.Exists)
            {
                Debug.WriteLine(String.Format("New DIR '{0}'", fi.Directory.FullName));
                fi.Directory.Create();
            }

            // Write the commit contents to the file:
            using (var fs = new FileStream(fi.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                Debug.WriteLine(String.Format("New TAG '{0}'", fi.FullName));
                tg.WriteTo(fs);
            }

            // Now keep track of the tag by its name:
            using (var fs = new FileStream(fiTracker.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                Debug.WriteLine(String.Format("New TAG '{0}'", fiTracker.FullName));
                byte[] rawID = Encoding.UTF8.GetBytes(tg.ID.ToString());
                fs.Write(rawID, 0, rawID.Length);
            }
        }

        private async Task<TagID?> getTagIDByName(TagName tagName)
        {
            FileInfo fiTracker = system.getTagPathByTagName(tagName);
            if (!fiTracker.Exists) return (TagID?)null;

            byte[] buf;
            int nr = 0;
            using (var fs = new FileStream(fiTracker.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384, true))
            {
                // TODO: implement an async buffered Stream:
                buf = new byte[16384];
                nr = await fs.ReadAsync(buf, 0, 16384).ConfigureAwait(continueOnCapturedContext: false);
                if (nr >= 16384)
                {
                    // My, what a large tag you have!
                    throw new NotSupportedException();
                }
            }

            // Parse the TagID:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line = sr.ReadLine();
                if (line == null) return (TagID?)null;

                return (TagID?) new TagID(line);
            }
        }

        private async Task<Tag> getTag(TagID id)
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
                    // My, what a large tag you have!
                    throw new NotSupportedException();
                }
            }

            Tag.Builder tb = new Tag.Builder();

            // Parse the Tag:
            using (var ms = new MemoryStream(buf, 0, nr, false))
            using (var sr = new StreamReader(ms, Encoding.UTF8))
            {
                string line = sr.ReadLine();

                // Set CommitID:
                if (line == null || !line.StartsWith("commit ")) throw new ObjectParseException("While parsing a tag, expected: 'commit'");
                tb.CommitID = new CommitID(line.Substring("commit ".Length));

                // Set Name:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("name ")) throw new ObjectParseException("While parsing a tag, expected: 'name'");
                tb.Name = (TagName) line.Substring("name ".Length);

                // Set Tagger:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("tagger ")) throw new ObjectParseException("While parsing a tag, expected: 'tagger'");
                tb.Tagger = line.Substring("tagger ".Length);

                // Set DateTagged:
                line = sr.ReadLine();
                if (line == null || !line.StartsWith("date ")) throw new ObjectParseException("While parsing a tag, expected: 'date'");

                // NOTE: date parsing will result in an inexact DateTimeOffset from what was created with, but it
                // is close enough because the SHA-1 hash is calculated using the DateTimeOffset.ToString(), so
                // only the ToString() representations of the DateTimeOffsets need to match.
                tb.DateTagged = DateTimeOffset.Parse(line.Substring("date ".Length));

                // Skip empty line:
                line = sr.ReadLine();
                if (line == null || line.Length != 0) throw new ObjectParseException("While parsing a commit, expected blank line");

                // Set Message:
                tb.Message = sr.ReadToEnd();
            }

            // Create the immutable Tag from the Builder:
            Tag tg = tb;
            // Validate the computed TagID:
            if (tg.ID != id) throw new TagIDMismatchException(tg.ID, id);

            return tg;
        }

        private void deleteTag(Tag tg)
        {
            FileInfo fi = system.getPathByID(tg.ID);
            if (fi.Exists) fi.Delete();
            FileInfo fiTracker = system.getTagPathByTagName(tg.Name);
            if (fiTracker.Exists) fiTracker.Delete();
        }

        #endregion

        public async Task<Tag> PersistTag(Tag tg)
        {
            await persistTag(tg).ConfigureAwait(continueOnCapturedContext: false);
            return tg;
        }

        public async Task<TagID?> DeleteTag(TagID id)
        {
            Tag tg = await getTag(id).ConfigureAwait(continueOnCapturedContext: false);
            if (tg == null) return (TagID?)null;

            deleteTag(tg);
            return tg.ID;
        }

        public async Task<TagID?> DeleteTagByName(TagName tagName)
        {
            TagID? id = await getTagIDByName(tagName).ConfigureAwait(continueOnCapturedContext: false);
            if (!id.HasValue) return null;

            Tag tg = await getTag(id.Value).ConfigureAwait(continueOnCapturedContext: false);
            deleteTag(tg);
            
            return id;
        }

        public Task<Tag> GetTag(TagID id)
        {
            return getTag(id);
        }

        public async Task<Tag> GetTagByName(TagName tagName)
        {
            TagID? id = await getTagIDByName(tagName).ConfigureAwait(continueOnCapturedContext: false);
            if (!id.HasValue) return null;

            var tg = await getTag(id.Value).ConfigureAwait(continueOnCapturedContext: false);
            if (tg.Name != tagName)
                throw new InvalidOperationException();
            return tg;
        }
    }
}
