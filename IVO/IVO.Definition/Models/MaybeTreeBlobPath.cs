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
    [TypeConverter(typeof(MaybeTreeBlobPathTypeConverter))]
    public sealed class MaybeTreeBlobPath
    {
        public MaybeTreeBlobPath(Maybe<TreeID> rootTreeID, CanonicalBlobPath path)
        {
            this.RootTreeID = rootTreeID;
            this.Path = path;
        }

        public Maybe<TreeID> RootTreeID { get; private set; }
        public CanonicalBlobPath Path { get; private set; }

        public override string ToString()
        {
            if (RootTreeID.HasValue)
                return String.Format("{0}:{1}", RootTreeID, Path.ToString());
            return Path.ToString();
        }
    }

    public sealed class MaybeTreeBlobPathTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return true;
            else if (typeof(Errorable<MaybeTreeBlobPath>) == destinationType)
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (typeof(string) == destinationType)
                return ((MaybeTreeBlobPath)value).ToString();
            else if (typeof(Errorable<MaybeTreeBlobPath>) == destinationType)
            {
                string strValue = value as string;
                if (strValue == null)
                    return (Errorable<MaybeTreeBlobPath>)null;

                try
                {
                    // {TreeID}:{CanonicalBlobPath}
                    CanonicalBlobPath path;
                    Maybe<TreeID> rootTreeID;

                    int colon = strValue.IndexOf(':');
                    if (colon >= 0)
                    {
                        var eroot = TreeID.TryParse(strValue.Substring(0, colon));
                        if (eroot.HasErrors) return (Errorable<MaybeTreeBlobPath>)eroot.Errors;
                        
                        rootTreeID = eroot.Value;
                        // TODO: TryParse on CanonicalBlobPath...
                        path = (CanonicalBlobPath)('/' + strValue.Substring(colon + 1));
                    }
                    else
                    {
                        rootTreeID = Maybe<TreeID>.Nothing;
                        // TODO: TryParse on CanonicalBlobPath...
                        path = (CanonicalBlobPath)('/' + strValue);
                    }

                    return (Errorable<MaybeTreeBlobPath>)new MaybeTreeBlobPath(rootTreeID, path);
                }
                catch (ErrorBase err)
                {
                    return (Errorable<MaybeTreeBlobPath>)err;
                }
                catch (Exception ex)
                {
                    return (Errorable<MaybeTreeBlobPath>)new InputError(ex.Message);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
