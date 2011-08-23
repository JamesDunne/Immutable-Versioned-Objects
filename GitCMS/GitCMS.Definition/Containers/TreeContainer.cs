using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GitCMS.Definition.Models;

namespace GitCMS.Definition.Containers
{
    public sealed class TreeContainer
    {
        private Dictionary<TreeID, Tree> _container;

        public TreeContainer(params Tree[] trees)
        {
            _container = trees.ToDictionary(tr => tr.ID);
        }

        public TreeContainer(IEnumerable<Tree> trees)
        {
            _container = trees.ToDictionary(tr => tr.ID);
        }

        public Tree this[TreeID id]
        {
            get { return _container[id]; }
        }

        public bool ContainsKey(TreeID id)
        {
            return _container.ContainsKey(id);
        }

        public bool TryGetValue(TreeID id, out Tree value)
        {
            return _container.TryGetValue(id, out value);
        }

        public Maybe<Tree> MaybeGet(TreeID id)
        {
            Tree value;
            
            if (_container.TryGetValue(id, out value))
                return new Maybe<Tree>(value);

            return Maybe<Tree>.Nothing;
        }

        public int Count { get { return _container.Count; } }

        public IEnumerable<TreeID> Keys { get { return _container.Keys; } }

        public IEnumerable<Tree> Values { get { return _container.Values; } }
    }
}
