using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GitCMS.Definition.Models;

namespace GitCMS.Definition.Containers
{
    public sealed class BlobContainer
    {
        private Dictionary<BlobID, Blob> _container;

        public BlobContainer(IEnumerable<Blob> blobs)
        {
            _container = blobs.ToDictionary(bl => bl.ID);
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
    }
}
