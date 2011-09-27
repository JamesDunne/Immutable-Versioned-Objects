using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace IVO.Definition.Errors
{
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
