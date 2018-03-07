using System;
using System.Collections.Generic;

namespace ApiContractGenerator
{
    internal sealed class GenericAwareNameComparer : IComparer<string>
    {
        public static GenericAwareNameComparer Instance { get; } = new GenericAwareNameComparer();
        private GenericAwareNameComparer() { }

        public int Compare(string x, string y)
        {
            var xParsed = NameUtils.ParseGenericArity(x);
            var yParsed = NameUtils.ParseGenericArity(y);

            var result = string.Compare(xParsed.name, yParsed.name, StringComparison.Ordinal);
            return result != 0 ? result : xParsed.arity.CompareTo(yParsed.arity);
        }
    }
}
