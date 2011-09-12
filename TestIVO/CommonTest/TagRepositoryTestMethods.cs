using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IVO.Definition;
using IVO.Definition.Models;
using IVO.Definition.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestIVO.CommonTest
{
    public sealed class TagRepositoryTestMethods
    {
        private ITagRepository tgrepo;

        internal TagRepositoryTestMethods(ITagRepository tgrepo)
        {
            this.tgrepo = tgrepo;
        }

        // FIXME!!!
        const string cmID = "0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a0a";

        internal async Task PersistTagTest()
        {
            Tag tg = new Tag.Builder((TagName)"v1.0", new CommitID(cmID), "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);
        }

        internal async Task DeleteTagTest()
        {
            Tag tg = new Tag.Builder((TagName)"v1.0", new CommitID(cmID), "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            Tag rtgPre = await tgrepo.GetTag(tg.ID);
            Assert.IsNotNull(rtgPre);
            Assert.AreEqual(tg.ID, rtgPre.ID);
            Assert.AreEqual(tg.Name.ToString(), rtgPre.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtgPre.CommitID);
            Assert.AreEqual(tg.Tagger, rtgPre.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtgPre.DateTagged.ToString());

            await tgrepo.DeleteTag(tg.ID);

            Tag rtgPost = await tgrepo.GetTag(tg.ID);
            Assert.IsNull(rtgPost);
        }

        internal async Task DeleteTagByNameTest()
        {
            Tag tg = new Tag.Builder((TagName)"v1.0", new CommitID(cmID), "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            Tag rtgPre = await tgrepo.GetTag(tg.ID);
            Assert.IsNotNull(rtgPre);
            Assert.AreEqual(tg.ID, rtgPre.ID);
            Assert.AreEqual(tg.Name.ToString(), rtgPre.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtgPre.CommitID);
            Assert.AreEqual(tg.Tagger, rtgPre.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtgPre.DateTagged.ToString());

            await tgrepo.DeleteTagByName((TagName)"v1.0");

            Tag rtgPost = await tgrepo.GetTag(tg.ID);
            Assert.IsNull(rtgPost);
        }

        internal async Task GetTagTest()
        {
            Tag tg = new Tag.Builder((TagName)"v1.0", new CommitID(cmID), "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            Tag rtg = await tgrepo.GetTag(tg.ID);
            Assert.IsNotNull(rtg);
            Assert.AreEqual(tg.ID, rtg.ID);
            Assert.AreEqual(tg.Name.ToString(), rtg.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtg.CommitID);
            Assert.AreEqual(tg.Tagger, rtg.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtg.DateTagged.ToString());
        }

        internal async Task GetTagByNameTest()
        {
            Tag tg = new Tag.Builder((TagName)"v1.0", new CommitID(cmID), "James S. Dunne", DateTimeOffset.Now, "Testing tags");
            await tgrepo.PersistTag(tg);

            Tag rtg = await tgrepo.GetTagByName((TagName)"v1.0");
            Assert.IsNotNull(rtg);
            Assert.AreEqual(tg.ID, rtg.ID);
            Assert.AreEqual(tg.Name.ToString(), rtg.Name.ToString());
            Assert.AreEqual(tg.CommitID, rtg.CommitID);
            Assert.AreEqual(tg.Tagger, rtg.Tagger);
            Assert.AreEqual(tg.DateTagged.ToString(), rtg.DateTagged.ToString());
        }
    }
}
