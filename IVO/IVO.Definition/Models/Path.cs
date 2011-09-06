using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IVO.Definition.Exceptions;

namespace IVO.Definition.Models
{
    public abstract class Path
    {
        public const char PathSeparatorChar = '/';
        public static readonly char[] PathSeparatorCharArray = new char[1] { PathSeparatorChar };
        public static readonly string PathSeparatorString = new string(PathSeparatorChar, 1);

        public static readonly char[] InvalidNameCharacters = new char[] { PathSeparatorChar, '\\', '*', ':', '|', '&', '?', '%' };
        internal static readonly HashSet<char> invalidCharSet = new HashSet<char>(InvalidNameCharacters);

        protected static void validateTreePath(IList<string> parts)
        {
            // Validate the parts:
            foreach (string part in parts)
            {
                if (String.IsNullOrWhiteSpace(part))
                    throw new InvalidPathException("One of the path components is empty or whitespace");

                // Make sure the part is a valid name:
                foreach (char ch in part)
                    if (invalidCharSet.Contains(ch))
                        throw new InvalidPathException("One of the path components, '{0}', contains an invalid character '{1}'", part, ch);
            }
        }

        protected static void validateCanonicalTreePath(IList<string> parts)
        {
            // Validate the parts:
            foreach (string part in parts)
            {
                if (String.IsNullOrWhiteSpace(part))
                    throw new InvalidPathException("One of the path components is empty or whitespace");

                // Make sure the part is a valid name:
                foreach (char ch in part)
                    if (invalidCharSet.Contains(ch))
                        throw new InvalidPathException("One of the path components, '{0}', contains an invalid character '{1}'", part, ch);

                // Make sure there are no directory traversals:
                if (part == "." || part == "..")
                    throw new InvalidPathException("Canonical path cannot contain directory traversals '.' or '..'"); 
            }
        }

        protected static string[] SplitPath(string path)
        {
            if (path.Length == 0) return new string[0];

            string[] parts;
            if (path[0] == PathSeparatorChar)
                parts = path.Substring(1).Split(PathSeparatorCharArray);
            else
                parts = path.Split(PathSeparatorCharArray);

            return parts;
        }

        public static Either<AbsoluteBlobPath, RelativeBlobPath> ParseBlobPath(string path)
        {
            if (path.Length == 0) throw new ArgumentException("path cannot be empty", "path");
            
            if (path[0] == PathSeparatorChar)
                return (AbsoluteBlobPath)path;
            else
                return (RelativeBlobPath)path;
        }

        public static Either<AbsoluteTreePath, RelativeTreePath> ParseTreePath(string path)
        {
            if (path.Length == 0) throw new ArgumentException("path cannot be empty", "path");

            if (path[0] == PathSeparatorChar)
                return (AbsoluteTreePath)path;
            else
                return (RelativeTreePath)path;
        }
    }
}
