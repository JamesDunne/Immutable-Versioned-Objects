using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IVO.Definition.Models;

namespace IVO.Definition.Containers
{
    public sealed class BlobContainer
    {
        private Dictionary<BlobID, Blob> _container;

        public BlobContainer(params Blob[] items)
        {
            _container = items.ToDictionary(e => e.ID);
        }

        public BlobContainer(IEnumerable<Blob> items)
        {
            _container = items.ToDictionary(e => e.ID);
        }

        public BlobContainer(IDictionary<BlobID, Blob> items)
        {
            _container = new Dictionary<BlobID, Blob>(items);
        }

        public Blob this[BlobID id]
        {
            get { return _container[id]; }
        }

        public bool ContainsKey(BlobID id)
        {
            return _container.ContainsKey(id);
        }

        public bool TryGetValue(BlobID id, out Blob value)
        {
            return _container.TryGetValue(id, out value);
        }

        public Maybe<Blob> MaybeGet(BlobID id)
        {
            Blob value;
            
            if (_container.TryGetValue(id, out value))
                return new Maybe<Blob>(value);

            return Maybe<Blob>.Nothing;
        }

        public int Count { get { return _container.Count; } }

        public IEnumerable<BlobID> Keys { get { return _container.Keys; } }

        public IEnumerable<Blob> Values { get { return _container.Values; } }
    }

    public sealed class TreeContainer
    {
        private Dictionary<TreeID, Tree> _container;

        public TreeContainer(params Tree[] items)
        {
            _container = items.ToDictionary(e => e.ID);
        }

        public TreeContainer(IEnumerable<Tree> items)
        {
            _container = items.ToDictionary(e => e.ID);
        }

        public TreeContainer(IDictionary<TreeID, Tree> items)
        {
            _container = new Dictionary<TreeID, Tree>(items);
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

    public sealed class ICommitContainer
    {
        private Dictionary<CommitID, ICommit> _container;

        public ICommitContainer(params ICommit[] items)
        {
            _container = items.ToDictionary(e => e.ID);
        }

        public ICommitContainer(IEnumerable<ICommit> items)
        {
            _container = items.ToDictionary(e => e.ID);
        }

        public ICommitContainer(IDictionary<CommitID, ICommit> items)
        {
            _container = new Dictionary<CommitID, ICommit>(items);
        }

        public ICommit this[CommitID id]
        {
            get { return _container[id]; }
        }

        public bool ContainsKey(CommitID id)
        {
            return _container.ContainsKey(id);
        }

        public bool TryGetValue(CommitID id, out ICommit value)
        {
            return _container.TryGetValue(id, out value);
        }

        public Maybe<ICommit> MaybeGet(CommitID id)
        {
            ICommit value;
            
            if (_container.TryGetValue(id, out value))
                return new Maybe<ICommit>(value);

            return Maybe<ICommit>.Nothing;
        }

        public int Count { get { return _container.Count; } }

        public IEnumerable<CommitID> Keys { get { return _container.Keys; } }

        public IEnumerable<ICommit> Values { get { return _container.Values; } }
    }
}
