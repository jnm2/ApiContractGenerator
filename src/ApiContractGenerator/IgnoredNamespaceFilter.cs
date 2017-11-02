using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ApiContractGenerator.Model;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed class IgnoredNamespaceFilter : IMetadataSource
    {
        private readonly IMetadataSource source;
        private readonly IEnumerable<string> ignoredNamespaces;

        public IgnoredNamespaceFilter(IMetadataSource source, IEnumerable<string> ignoredNamespaces)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.ignoredNamespaces = ignoredNamespaces ?? throw new ArgumentNullException(nameof(ignoredNamespaces));
        }

        private IReadOnlyList<IMetadataNamespace> namespaces;
        public IReadOnlyList<IMetadataNamespace> Namespaces
        {
            get => namespaces ?? (namespaces = CalculateNonignoredTransitiveClosure());
        }

        private IReadOnlyList<IMetadataNamespace> CalculateNonignoredTransitiveClosure()
        {
            var regexBuilder = (StringBuilder)null;
            foreach (var ignoredNamespace in ignoredNamespaces)
            {
                if (regexBuilder == null)
                    regexBuilder = new StringBuilder(@"\A(?:");
                else
                    regexBuilder.Append('|');

                regexBuilder.Append(Regex.Escape(ignoredNamespace));
            }

            if (regexBuilder == null) return source.Namespaces;

            var regex = new Regex(regexBuilder.Append(@")(\Z|\.)").ToString(), RegexOptions.IgnoreCase);

            return source.Namespaces.Where(ns => !regex.IsMatch(ns.Name)).ToList();
        }
    }
}
