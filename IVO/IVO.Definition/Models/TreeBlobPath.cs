using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using IVO.Definition.Errors;

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

        public override string ToString()
        {
            return String.Format("{0}{1}", RootTreeID, Path);
        }
    }

    public sealed class TreeBlobPathTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else if (typeof(Errorable<TreeBlobPath>) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((TreeBlobPath)value).ToString();
            else if (typeof(Errorable<TreeBlobPath>) == destinationType)
            {
                string strValue = value as string;
                if (strValue == null)
                    return (Errorable<TreeBlobPath>)null;

                try
                {
                    // {TreeID}{:CanonicalBlobPath}
                    int firstSlash = strValue.IndexOf(':');
                    if (firstSlash < 0) return (Errorable<TreeBlobPath>)new TreeID.ParseError("Could not find first ':' char of TreeBlobPath");

                    var eroot = TreeID.TryParse(strValue.Substring(0, firstSlash));
                    if (eroot.HasErrors) return (Errorable<TreeBlobPath>)eroot.Errors;

                    // TODO: TryParse on CanonicalBlobPath...
                    CanonicalBlobPath path = (CanonicalBlobPath)strValue.Substring(firstSlash);

                    return (Errorable<TreeBlobPath>)new TreeBlobPath(eroot.Value, path);
                }
                catch (ErrorBase err)
                {
                    return (Errorable<TreeBlobPath>)err;
                }
                catch (Exception ex)
                {
                    return (Errorable<TreeBlobPath>)new InputError(ex.Message);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
