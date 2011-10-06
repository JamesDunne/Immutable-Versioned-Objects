using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using IVO.Definition.Errors;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A staging name.
    /// </summary>
    [TypeConverter(typeof(StageNameStringConverter))]
    public sealed class StageName : PathObjectModel, IComparable<StageName>, IComparable
    {
        internal StageName(IList<string> parts)
        {
            // parts must be already validated with validateCanonicalPath().
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        internal StageName(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        /// <summary>
        /// Gets the path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public static explicit operator StageName(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathError("Stage name cannot be empty");

            string[] parts = SplitPath(path);

            validateCanonicalTreePath(parts);

            return new StageName(parts);
        }

        /// <summary>
        /// Renders the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Parts.Count == 0) return PathSeparatorString;

            return String.Join(PathSeparatorString, Parts);
        }

        public static bool operator ==(StageName a, StageName b)
        {
            if (Object.ReferenceEquals(a, null) != Object.ReferenceEquals(b, null)) return false;
            if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null)) return true;
            return a.ToString() == b.ToString();
        }

        public static bool operator !=(StageName a, StageName b)
        {
            if (Object.ReferenceEquals(a, null) != Object.ReferenceEquals(b, null)) return true;
            if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null)) return false;
            return a.ToString() != b.ToString();
        }

        public override bool Equals(object obj)
        {
            return this.ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public int CompareTo(StageName other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo((StageName)obj);
        }
    }

    public sealed class StageNameStringConverter : TypeConverter
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
                return (StageName)strValue;

            return base.ConvertFrom(context, culture, value);
        }
    }
}
