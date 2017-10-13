using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ApiContractGenerator.Model;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed class ApiContractGenerator
    {
        public ISet<string> IgnoredNamespaces { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Generate(IMetadataSource source, IMetadataWriter writer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.Write(new Filter(source, IgnoredNamespaces));
        }

        private sealed class Filter : IMetadataSource
        {
            private readonly IMetadataSource source;
            private readonly Regex ignoredNamespaces;

            public Filter(IMetadataSource source, IEnumerable<string> ignoredNamespaces)
            {
                this.source = source;

                var regexBuilder = new StringBuilder(@"\A(?:");
                var isFirst = true;
                foreach (var ignoredNamespace in ignoredNamespaces)
                {
                    if (isFirst) isFirst = false; else regexBuilder.Append('|');
                    regexBuilder.Append(Regex.Escape(ignoredNamespace));
                }

                this.ignoredNamespaces = isFirst ? null : new Regex(regexBuilder.Append(@")(\Z|\.)").ToString(), RegexOptions.IgnoreCase);
            }

            public IReadOnlyList<IMetadataNamespace> Namespaces
            {
                get => ignoredNamespaces == null ? source.Namespaces :
                    source.Namespaces.Where(ns => !ignoredNamespaces.IsMatch(ns.Name)).ToList();
            }
        }
    }
}
