using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    /// <summary>
    /// A relative blob path is a path to a blob relative to some absolute tree path.
    /// </summary>
    public sealed class RelativeBlobPath : PathObjectModel
    {
        /// <summary>
        /// Creates a relative blob path from a relative tree path and a blob name.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="name"></param>
        public RelativeBlobPath(RelativeTreePath tree, string name)
        {
            this.Tree = tree;
            this.Name = name;
        }

        /// <summary>
        /// Gets the relative tree path.
        /// </summary>
        public RelativeTreePath Tree { get; private set; }
        /// <summary>
        /// Gets the blob name.
        /// </summary>
        public string Name { get; private set; }

        public static explicit operator RelativeBlobPath(string path)
        {
            if (path.Length == 0) throw new InvalidPathException("relative path cannot be empty");

            string[] parts = SplitPath(path);
            int treePartCount = parts.Length - 1;

            string[] treeParts = new string[treePartCount];
            Array.Copy(parts, treeParts, treePartCount);

            return new RelativeBlobPath((RelativeTreePath)treeParts, parts[treePartCount]);
        }

        /// <summary>
        /// Renders the path as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Concat(Tree.ToString(), Name);
        }
    }
}
