using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace IVO.Definition.Models
{
    /// <summary>
    /// Represents a canonical path to a blob relative to a root TreeID.
    /// </summary>
    [TypeConverter(typeof(TreeBlobPathTypeConverter))]
    public sealed class TreeBlobPath
    {
        public TreeBlobPath(TreeID rootTreeID, CanonicalBlobPath path)
        {
            this.RootTreeID = rootTreeID;
            this.Path = path;
        }

        public TreeID RootTreeID { get; private set; }
        public CanonicalBlobPath Path { get; private set; }
    }

    public sealed class TreeBlobPathTypeConverter : TypeConverter
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
            {
                // {TreeID}{/CanonicalBlobPath}
                int firstSlash = strValue.IndexOf('/');
                if (firstSlash < 0) goto fail;
                
                TreeID root = TreeID.Parse(strValue.Substring(0, firstSlash)).Value;
                CanonicalBlobPath path = (CanonicalBlobPath)strValue.Substring(firstSlash);
                
                return new TreeBlobPath(root, path);
            }

        fail:
            return base.ConvertFrom(context, culture, value);
        }
    }
}
