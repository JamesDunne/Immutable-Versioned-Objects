using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using IVO.Definition.Errors;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A canonicalized absolute blob path is a canonical tree path guaranteed to not contain directory traversals with a blob name hanging off of it.
    /// </summary>
    [TypeConverter(typeof(CanonicalBlobPathTypeConverter))]
    public sealed class CanonicalBlobPath : PathObjectModel
    {
        private string _asString;
        /// <summary>
        /// Creates a canonical blob path from a canonical tree path and a blob name.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="name"></param>
        public CanonicalBlobPath(CanonicalTreePath tree, string name)
        {
            this.Tree = tree;
            this.Name = name;
            this._asString = String.Concat(Tree.ToString(), Name);
        }

        /// <summary>
        /// Gets the canonical tree path.
        /// </summary>
        public CanonicalTreePath Tree { get; private set; }
        /// <summary>
        /// Gets the blob name.
        /// </summary>
        public string Name { get; private set; }

        public static explicit operator CanonicalBlobPath(string path)
        {
            if (String.IsNullOrWhiteSpace(path)) throw new InvalidPathError("Path cannot be empty");
            if (path[0] != PathSeparatorChar) throw new InvalidPathError("Canonical blob path must begin with a '{0}'", PathSeparatorString);
            if (path[path.Length - 1] == PathSeparatorChar)
                throw new InvalidPathError("Canonical blob path cannot end in path separator character, '{0}'", PathSeparatorChar);

            string[] parts = SplitPath(path);
            validateCanonicalTreePath(parts);

            int treePartCount = parts.Length - 1;

            string[] treeParts = new string[treePartCount];
            Array.Copy(parts, treeParts, treePartCount);

            string blobName = parts[treePartCount];
            if (String.IsNullOrWhiteSpace(blobName))
                throw new InvalidPathError("Canonical blob path cannot end in path separator character, '{0}'", PathSeparatorChar);
            if (blobName == "." || blobName == "..")
                throw new InvalidPathError("Canonical blob path cannot end in '.' or '..' directory traversals");

            return new CanonicalBlobPath(new CanonicalTreePath(treeParts), blobName);
        }

        public static implicit operator AbsoluteBlobPath(CanonicalBlobPath path)
        {
            return new AbsoluteBlobPath(path.Tree, path.Name);
        }

        /// <summary>
        /// Renders the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._asString;
        }
    }

    public sealed class CanonicalBlobPathTypeConverter : TypeConverter
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
                return (CanonicalBlobPath)strValue;
            else
                return base.ConvertFrom(context, culture, value);
        }
    }
}
