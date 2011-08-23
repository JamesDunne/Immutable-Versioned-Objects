using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCMS.Definition.Models
{
    public struct CommitID : IEquatable<CommitID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;

        public CommitID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("CommitID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
        }

        public static explicit operator byte[](CommitID id)
        {
            return id._idValue;
        }

        public static explicit operator CommitID(byte[] hash)
        {
            return new CommitID(hash);
        }

        public bool Equals(CommitID other)
        {
            // Compare byte-by-byte:
            for (int i = 0; i < ByteArrayLength; ++i)
                if (this._idValue[i] != other._idValue[i]) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _quickHash;
        }

        public override string ToString()
        {
            return BitConverter.ToString(_idValue).ToLower();
        }
    }

    public struct TreeID : IEquatable<TreeID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;

        public TreeID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("TreeID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
        }

        public static explicit operator byte[](TreeID id)
        {
            return id._idValue;
        }

        public static explicit operator TreeID(byte[] hash)
        {
            return new TreeID(hash);
        }

        public bool Equals(TreeID other)
        {
            // Compare byte-by-byte:
            for (int i = 0; i < ByteArrayLength; ++i)
                if (this._idValue[i] != other._idValue[i]) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _quickHash;
        }

        public override string ToString()
        {
            return BitConverter.ToString(_idValue).ToLower();
        }
    }

    public struct BlobID : IEquatable<BlobID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;

        public BlobID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("BlobID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
        }

        public static explicit operator byte[](BlobID id)
        {
            return id._idValue;
        }

        public static explicit operator BlobID(byte[] hash)
        {
            return new BlobID(hash);
        }

        public bool Equals(BlobID other)
        {
            // Compare byte-by-byte:
            for (int i = 0; i < ByteArrayLength; ++i)
                if (this._idValue[i] != other._idValue[i]) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _quickHash;
        }

        public override string ToString()
        {
            return BitConverter.ToString(_idValue).ToLower();
        }
    }
}
