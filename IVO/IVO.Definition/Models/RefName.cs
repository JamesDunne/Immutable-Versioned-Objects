using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A ref name.
    /// </summary>
    [TypeConverter(typeof(RefNameStringConverter))]
    public sealed class RefName : PathObjectModel
    {
        internal RefName(IList<string> parts)
        {
            // parts must be already validated with validateCanonicalPath().
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        internal RefName(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        /// <summary>
        /// Gets the path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public static explicit operator RefName(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathException("Ref name cannot be empty");

            string[] parts = SplitPath(path);

            validateCanonicalTreePath(parts);

            return new RefName(parts);
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

        public static bool operator ==(RefName a, RefName b)
        {
            if (Object.ReferenceEquals(a, null) && !Object.ReferenceEquals(b, null)) return false;
            if (!Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null)) return false;
            return a.ToString() == b.ToString();
        }

        public static bool operator !=(RefName a, RefName b)
        {
            if (Object.ReferenceEquals(a, null) && !Object.ReferenceEquals(b, null)) return true;
            if (!Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null)) return true;
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
    }

    public sealed class RefNameStringConverter : TypeConverter
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
                return (RefName)strValue;

            return base.ConvertFrom(context, culture, value);
        }
    }
}
