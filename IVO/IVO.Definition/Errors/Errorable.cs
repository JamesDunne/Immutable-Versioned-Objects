using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace IVO.Definition.Errors
{
    [TypeConverter(typeof(ErrorableTypeConverter))]
    public sealed class Errorable<Tsuccess>
    {
        public Errorable(Tsuccess value)
        {
            this._value = value;
            this.HasErrors = false;
            this.Errors = null;
        }

        private Errorable(ErrorContainer errors)
        {
            this.HasErrors = true;
            this.Errors = errors;
        }

        public static implicit operator Errorable<Tsuccess>(Tsuccess value)
        {
            return new Errorable<Tsuccess>(value);
        }

        public static implicit operator Errorable<Tsuccess>(ErrorContainer errors)
        {
            return new Errorable<Tsuccess>(errors);
        }

        public static implicit operator Errorable<Tsuccess>(ErrorBase err)
        {
            return new Errorable<Tsuccess>((ErrorContainer)err);
        }

        private Tsuccess _value;
        public Tsuccess Value { get { if (this.HasErrors) throw new Exception(); return this._value; } }

        public bool HasErrors { get; private set; }
        public ErrorContainer Errors { get; private set; }
    }

    public sealed class ErrorableTypeConverter : TypeConverter
    {
        static ErrorableTypeConverter()
        {
            TypeDescriptor.AddAttributes(typeof(string), new TypeConverterAttribute(typeof(ErrorableTypeConverter)));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType.IsGenericType && destinationType.Name == "Errorable`1" && destinationType.Namespace == "IVO.Definition.Errors")
            {
                TypeConverter cvt = TypeDescriptor.GetConverter(destinationType.GetGenericArguments()[0]);
                return cvt.CanConvertTo(context, destinationType);
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType.IsGenericType && destinationType.Name == "Errorable`1" && destinationType.Namespace == "IVO.Definition.Errors")
            {
                TypeConverter cvt = TypeDescriptor.GetConverter(destinationType.GetGenericArguments()[0]);
                return cvt.ConvertTo(context, culture, value, destinationType);
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    
    public sealed class Errorable
    {
        private Errorable()
        {
            this.HasErrors = false;
            this.Errors = new ErrorContainer();
        }

        private Errorable(ErrorContainer errors)
        {
            this.HasErrors = true;
            this.Errors = errors;
        }

        public static implicit operator Errorable(ErrorContainer errors)
        {
            return new Errorable(errors);
        }

        public static implicit operator Errorable(ErrorBase err)
        {
            return new Errorable((ErrorContainer)err);
        }

        public static readonly Errorable NoErrors = new Errorable();

        public bool HasErrors { get; private set; }
        public ErrorContainer Errors { get; private set; }
    }
}
