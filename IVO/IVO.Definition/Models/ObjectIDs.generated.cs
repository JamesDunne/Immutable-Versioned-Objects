using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace IVO.Definition.Models
{
	[TypeConverter(typeof(CommitIDTypeConverter))]
    public struct CommitID : IEquatable<CommitID>
    {
        public const int ByteArrayLength = 20;
        public const int HexCharLength = ByteArrayLength * 2;

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

        public static Maybe<CommitID> Parse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return Maybe<CommitID>.Nothing;

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);
                if (v1 == -1) return Maybe<CommitID>.Nothing;
                if (v2 == -1) return Maybe<CommitID>.Nothing;
                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new CommitID(tmp);
        }

        public static Either<CommitID, Exception> TryParse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return new Exception(String.Format("CommitID must be {0} characters in length", HexCharLength));

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);

                if (v1 == -1) return new Exception(String.Format("CommitID character position {0} has invalid hex character '{1}'", i * 2 + 0, hexValue[i * 2 + 0]));
                if (v2 == -1) return new Exception(String.Format("CommitID character position {0} has invalid hex character '{1}'", i * 2 + 1, hexValue[i * 2 + 1]));

                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new CommitID(tmp);
        }

        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            return -1;
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
            if (_toString == null) return String.Empty;
            return _toString;
        }

        public string ToString(int firstLength = HexCharLength)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (firstLength > HexCharLength) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (_toString == null) return String.Empty;
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

    public sealed class CommitIDTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(string) == sourceType)
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string strValue = value as string;
            if (strValue != null)
				return CommitID.Parse(strValue).Value;

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((CommitID)value).ToString();
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }

	[TypeConverter(typeof(TreeIDTypeConverter))]
    public struct TreeID : IEquatable<TreeID>
    {
        public const int ByteArrayLength = 20;
        public const int HexCharLength = ByteArrayLength * 2;

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

        public static Maybe<TreeID> Parse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return Maybe<TreeID>.Nothing;

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);
                if (v1 == -1) return Maybe<TreeID>.Nothing;
                if (v2 == -1) return Maybe<TreeID>.Nothing;
                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new TreeID(tmp);
        }

        public static Either<TreeID, Exception> TryParse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return new Exception(String.Format("TreeID must be {0} characters in length", HexCharLength));

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);

                if (v1 == -1) return new Exception(String.Format("TreeID character position {0} has invalid hex character '{1}'", i * 2 + 0, hexValue[i * 2 + 0]));
                if (v2 == -1) return new Exception(String.Format("TreeID character position {0} has invalid hex character '{1}'", i * 2 + 1, hexValue[i * 2 + 1]));

                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new TreeID(tmp);
        }

        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            return -1;
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
            if (_toString == null) return String.Empty;
            return _toString;
        }

        public string ToString(int firstLength = HexCharLength)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (firstLength > HexCharLength) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (_toString == null) return String.Empty;
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

    public sealed class TreeIDTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(string) == sourceType)
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string strValue = value as string;
            if (strValue != null)
				return TreeID.Parse(strValue).Value;

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((TreeID)value).ToString();
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }

	[TypeConverter(typeof(BlobIDTypeConverter))]
    public struct BlobID : IEquatable<BlobID>
    {
        public const int ByteArrayLength = 20;
        public const int HexCharLength = ByteArrayLength * 2;

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

        public static Maybe<BlobID> Parse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return Maybe<BlobID>.Nothing;

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);
                if (v1 == -1) return Maybe<BlobID>.Nothing;
                if (v2 == -1) return Maybe<BlobID>.Nothing;
                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new BlobID(tmp);
        }

        public static Either<BlobID, Exception> TryParse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return new Exception(String.Format("BlobID must be {0} characters in length", HexCharLength));

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);

                if (v1 == -1) return new Exception(String.Format("BlobID character position {0} has invalid hex character '{1}'", i * 2 + 0, hexValue[i * 2 + 0]));
                if (v2 == -1) return new Exception(String.Format("BlobID character position {0} has invalid hex character '{1}'", i * 2 + 1, hexValue[i * 2 + 1]));

                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new BlobID(tmp);
        }

        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            return -1;
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
            if (_toString == null) return String.Empty;
            return _toString;
        }

        public string ToString(int firstLength = HexCharLength)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (firstLength > HexCharLength) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (_toString == null) return String.Empty;
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

    public sealed class BlobIDTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(string) == sourceType)
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string strValue = value as string;
            if (strValue != null)
				return BlobID.Parse(strValue).Value;

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((BlobID)value).ToString();
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }

	[TypeConverter(typeof(TagIDTypeConverter))]
    public struct TagID : IEquatable<TagID>
    {
        public const int ByteArrayLength = 20;
        public const int HexCharLength = ByteArrayLength * 2;

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

        public static Maybe<TagID> Parse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return Maybe<TagID>.Nothing;

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);
                if (v1 == -1) return Maybe<TagID>.Nothing;
                if (v2 == -1) return Maybe<TagID>.Nothing;
                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new TagID(tmp);
        }

        public static Either<TagID, Exception> TryParse(string hexValue)
        {
            // Sanity check first:
            if (hexValue.Length != HexCharLength) return new Exception(String.Format("TagID must be {0} characters in length", HexCharLength));

            byte[] tmp = new byte[ByteArrayLength];
            for (int i = 0; i < ByteArrayLength; ++i)
            {
                int v1 = deHex(hexValue[i * 2 + 0]);
                int v2 = deHex(hexValue[i * 2 + 1]);

                if (v1 == -1) return new Exception(String.Format("TagID character position {0} has invalid hex character '{1}'", i * 2 + 0, hexValue[i * 2 + 0]));
                if (v2 == -1) return new Exception(String.Format("TagID character position {0} has invalid hex character '{1}'", i * 2 + 1, hexValue[i * 2 + 1]));

                tmp[i] = (byte)((v1 << 4) | v2);
            }

            return new TagID(tmp);
        }

        private static int deHex(char c)
        {
            if (c >= 'A' && c <= 'F') return (int)(c - 'A' + 10);
            if (c >= 'a' && c <= 'f') return (int)(c - 'a' + 10);
            if (c >= '0' && c <= '9') return (int)(c - '0');
            return -1;
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
            if (_toString == null) return String.Empty;
            return _toString;
        }

        public string ToString(int firstLength = HexCharLength)
        {
            if (firstLength <= 0) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (firstLength > HexCharLength) throw new ArgumentOutOfRangeException("firstLength", String.Format("firstLength must be greater than 0 and less than or equal to {0}", HexCharLength));
            if (_toString == null) return String.Empty;
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

    public sealed class TagIDTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(string) == sourceType)
                return true;
            else
                return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string strValue = value as string;
            if (strValue != null)
				return TagID.Parse(strValue).Value;

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((TagID)value).ToString();
            else
                return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
