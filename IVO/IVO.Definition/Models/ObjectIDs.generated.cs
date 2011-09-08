using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
    public struct CommitID : IEquatable<CommitID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;
        private string _toString;

        public CommitID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("CommitID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        public CommitID(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != ByteArrayLength * 2) throw new ArgumentOutOfRangeException("hexValue", String.Format("TreeID hex string must be {0} characters in length", ByteArrayLength * 2));

            _idValue = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                _idValue[i] = (byte)((deHex(hexValue[i * 2 + 0]) << 4) | deHex(hexValue[i * 2 + 1]));
            }

            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            throw new ArgumentOutOfRangeException("c", "Not a hexadecimal character!");
        }

        public static explicit operator byte[](CommitID id)
        {
            return id._idValue;
        }

        public static explicit operator CommitID(byte[] hash)
        {
            return new CommitID(hash);
        }
        
        public static bool operator ==(CommitID a, CommitID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CommitID a, CommitID b)
        {
            return !a.Equals(b);
        }
        
        public override bool Equals(object obj)
        {
            return this.Equals((CommitID)obj);
        }

        public bool Equals(CommitID other)
        {
            if ((this._idValue == null) != (other._idValue == null)) return false;

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
            return _toString;
        }

        public string ToString(int firstLength = ByteArrayLength * 2)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            if (firstLength > ByteArrayLength * 2) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            return _toString.Substring(0, firstLength);
        }

        public class Comparer : IComparer<CommitID>
        {
            public int Compare(CommitID x, CommitID y)
            {
                for (int i = 0; i < ByteArrayLength; ++i)
                {
                    if (x._idValue[i] == y._idValue[i]) continue;
                    return x._idValue[i].CompareTo(y._idValue[i]);
                }
                return 0;
            }
        }
    }

    public struct TreeID : IEquatable<TreeID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;
        private string _toString;

        public TreeID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("TreeID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        public TreeID(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != ByteArrayLength * 2) throw new ArgumentOutOfRangeException("hexValue", String.Format("TreeID hex string must be {0} characters in length", ByteArrayLength * 2));

            _idValue = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                _idValue[i] = (byte)((deHex(hexValue[i * 2 + 0]) << 4) | deHex(hexValue[i * 2 + 1]));
            }

            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            throw new ArgumentOutOfRangeException("c", "Not a hexadecimal character!");
        }

        public static explicit operator byte[](TreeID id)
        {
            return id._idValue;
        }

        public static explicit operator TreeID(byte[] hash)
        {
            return new TreeID(hash);
        }
        
        public static bool operator ==(TreeID a, TreeID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TreeID a, TreeID b)
        {
            return !a.Equals(b);
        }
        
        public override bool Equals(object obj)
        {
            return this.Equals((TreeID)obj);
        }

        public bool Equals(TreeID other)
        {
            if ((this._idValue == null) != (other._idValue == null)) return false;

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
            return _toString;
        }

        public string ToString(int firstLength = ByteArrayLength * 2)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            if (firstLength > ByteArrayLength * 2) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            return _toString.Substring(0, firstLength);
        }

        public class Comparer : IComparer<TreeID>
        {
            public int Compare(TreeID x, TreeID y)
            {
                for (int i = 0; i < ByteArrayLength; ++i)
                {
                    if (x._idValue[i] == y._idValue[i]) continue;
                    return x._idValue[i].CompareTo(y._idValue[i]);
                }
                return 0;
            }
        }
    }

    public struct BlobID : IEquatable<BlobID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;
        private string _toString;

        public BlobID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("BlobID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        public BlobID(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != ByteArrayLength * 2) throw new ArgumentOutOfRangeException("hexValue", String.Format("TreeID hex string must be {0} characters in length", ByteArrayLength * 2));

            _idValue = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                _idValue[i] = (byte)((deHex(hexValue[i * 2 + 0]) << 4) | deHex(hexValue[i * 2 + 1]));
            }

            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            throw new ArgumentOutOfRangeException("c", "Not a hexadecimal character!");
        }

        public static explicit operator byte[](BlobID id)
        {
            return id._idValue;
        }

        public static explicit operator BlobID(byte[] hash)
        {
            return new BlobID(hash);
        }
        
        public static bool operator ==(BlobID a, BlobID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(BlobID a, BlobID b)
        {
            return !a.Equals(b);
        }
        
        public override bool Equals(object obj)
        {
            return this.Equals((BlobID)obj);
        }

        public bool Equals(BlobID other)
        {
            if ((this._idValue == null) != (other._idValue == null)) return false;

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
            return _toString;
        }

        public string ToString(int firstLength = ByteArrayLength * 2)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            if (firstLength > ByteArrayLength * 2) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            return _toString.Substring(0, firstLength);
        }

        public class Comparer : IComparer<BlobID>
        {
            public int Compare(BlobID x, BlobID y)
            {
                for (int i = 0; i < ByteArrayLength; ++i)
                {
                    if (x._idValue[i] == y._idValue[i]) continue;
                    return x._idValue[i].CompareTo(y._idValue[i]);
                }
                return 0;
            }
        }
    }

    public struct TagID : IEquatable<TagID>
    {
        public const int ByteArrayLength = 20;

        private readonly byte[] _idValue;
        private int _quickHash;
        private string _toString;

        public TagID(byte[] value)
        {
            // Sanity check first:
            if (value.Length != ByteArrayLength) throw new ArgumentOutOfRangeException("value", String.Format("TagID value must be {0} bytes in length", ByteArrayLength));
            
            _idValue = value;
            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        public TagID(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != ByteArrayLength * 2) throw new ArgumentOutOfRangeException("hexValue", String.Format("TreeID hex string must be {0} characters in length", ByteArrayLength * 2));

            _idValue = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                _idValue[i] = (byte)((deHex(hexValue[i * 2 + 0]) << 4) | deHex(hexValue[i * 2 + 1]));
            }

            _quickHash = BitConverter.ToInt32(_idValue, 0);
            _toString = _idValue.ToHexString(0, 20);
        }
        
        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            throw new ArgumentOutOfRangeException("c", "Not a hexadecimal character!");
        }

        public static explicit operator byte[](TagID id)
        {
            return id._idValue;
        }

        public static explicit operator TagID(byte[] hash)
        {
            return new TagID(hash);
        }
        
        public static bool operator ==(TagID a, TagID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TagID a, TagID b)
        {
            return !a.Equals(b);
        }
        
        public override bool Equals(object obj)
        {
            return this.Equals((TagID)obj);
        }

        public bool Equals(TagID other)
        {
            if ((this._idValue == null) != (other._idValue == null)) return false;

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
            return _toString;
        }

        public string ToString(int firstLength = ByteArrayLength * 2)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            if (firstLength > ByteArrayLength * 2) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", ByteArrayLength * 2));
            return _toString.Substring(0, firstLength);
        }

        public class Comparer : IComparer<TagID>
        {
            public int Compare(TagID x, TagID y)
            {
                for (int i = 0; i < ByteArrayLength; ++i)
                {
                    if (x._idValue[i] == y._idValue[i]) continue;
                    return x._idValue[i].CompareTo(y._idValue[i]);
                }
                return 0;
            }
        }
    }
}
