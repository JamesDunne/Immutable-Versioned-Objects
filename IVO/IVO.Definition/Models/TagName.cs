﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using IVO.Definition.Errors;
using System.ComponentModel;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A tag name.
    /// </summary>
    [TypeConverter(typeof(TagNameStringConverter))]
    public sealed class TagName : PathObjectModel, IComparable<TagName>, IComparable
    {
        internal TagName(IList<string> parts)
        {
            // parts must be already validated with validateCanonicalPath().
            this.Parts = new ReadOnlyCollection<string>(parts);
        }

        internal TagName(IEnumerable<string> parts, int initialCapacity = 4)
            : this(parts.ToList(initialCapacity))
        {
        }

        /// <summary>
        /// Gets the path components.
        /// </summary>
        public ReadOnlyCollection<string> Parts { get; private set; }

        public static explicit operator TagName(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathError("Tag name cannot be empty");

            string[] parts = SplitPath(path);

            validateCanonicalTreePath(parts);

            return new TagName(parts);
        }

        public static explicit operator TagName(string[] parts)
        {
            if (parts == null) throw new ArgumentNullException("parts");

            validateCanonicalTreePath(parts);

            return new TagName(parts);
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

        public static bool operator ==(TagName a, TagName b)
        {
            if (Object.ReferenceEquals(a, null) != Object.ReferenceEquals(b, null)) return false;
            if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null)) return true;
            return a.ToString() == b.ToString();
        }

        public static bool operator !=(TagName a, TagName b)
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

        public int CompareTo(TagName other)
        {
            return this.ToString().CompareTo(other.ToString());
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo((TagName)obj);
        }
    }

    public sealed class TagNameStringConverter : TypeConverter
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
                return (TagName)strValue;

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else if (typeof(Errorable<TagName>) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((TagName)value).ToString();
            else if (typeof(Errorable<TagName>) == destinationType)
            {
                string strValue = value as string;
                if (strValue != null)
                {
                    try
                    {
                        TagName name = (TagName)strValue;
                        return (Errorable<TagName>)name;
                    }
                    catch (ErrorBase err)
                    {
                        return (Errorable<TagName>)err;
                    }
                    catch (Exception ex)
                    {
                        return (Errorable<TagName>)new InputError(ex.Message);
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
