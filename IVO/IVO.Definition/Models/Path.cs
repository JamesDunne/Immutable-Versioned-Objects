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

        public static readonly char[] InvalidNameCharacters = new char[] { PathSeparatorChar, ':', '$', '|', '&', '?', '%' };
        internal static readonly HashSet<char> invalidCharSet = new HashSet<char>(InvalidNameCharacters);

        protected static void validatePath(IList<string> parts)
        {
            // Validate the parts:
            foreach (string part in parts)
            {
                // Make sure the part is a valid name:
                foreach (char ch in part)
                    if (invalidCharSet.Contains(ch))
                        throw new InvalidPathException("One of the path components, '{0}', contains an invalid character '{1}'", part, ch);
            }
        }

        protected static string[] SplitPath(string path)
        {
            return path.Split(PathSeparatorCharArray, StringSplitOptions.RemoveEmptyEntries);
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
