using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Asynq;
using GitCMS.Data;
using GitCMS.Data.Persists;
using GitCMS.Data.Queries;
using GitCMS.Definition;
using GitCMS.Definition.Models;
using GitCMS.Definition.Containers;
using GitCMS.Definition.Repositories;

namespace GitCMS.Implementation.SQL
{
    public class TreeRepository : ITreeRepository
    {
        private DataContext db;

        public TreeRepository(DataContext db)
        {
            this.db = db;
        }

        public Task<Tree> PersistTree(TreeID rootid, TreeContainer trees)
        {
            return Task.Factory.ContinueWhenAll(null, t => (Tree)new Tree.Builder());
        }

        public Task<Tuple<TreeID, TreeContainer>> RetrieveTreeRecursively(TreeID rootid)
        {
            return db.AsynqMulti(new QueryTreeRecursively(rootid));
        }

        public Task<TreeID> DeleteTreeRecursively(TreeID rootid)
        {
            throw new NotImplementedException();
        }
    }
}
