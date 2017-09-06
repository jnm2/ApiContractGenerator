using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ApiContractGenerator.Model;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed class ApiContractGenerator
    {
        public ISet<string> IgnoredNamespaces { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Generate(IMetadataSource source, IMetadataVisitor formatter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            source.Accept(new Filter(formatter, IgnoredNamespaces));
        }

        private sealed class Filter : IMetadataVisitor
        {
            private readonly IMetadataVisitor formatter;
            private readonly Regex ignoredNamespaces;

            public Filter(IMetadataVisitor formatter, IEnumerable<string> ignoredNamespaces)
            {
                this.formatter = formatter;

                var regexBuilder = new StringBuilder(@"\A(?:");
                var isFirst = true;
                foreach (var ignoredNamespace in ignoredNamespaces)
                {
                    if (isFirst) isFirst = false; else regexBuilder.Append('|');
                    regexBuilder.Append(Regex.Escape(ignoredNamespace));
                }
                this.ignoredNamespaces = new Regex(regexBuilder.Append(@")(\Z|\.)").ToString(), RegexOptions.IgnoreCase);
            }

            public void Visit(IMetadataNamespace metadataNamespace)
            {
                if (ignoredNamespaces.IsMatch(metadataNamespace.Name)) return;
                formatter.Visit(metadataNamespace);
            }

            public void Visit(IMetadataClass metadataClass)
            {
                formatter.Visit(metadataClass);
            }

            public void Visit(IMetadataStruct metadataStruct)
            {
                formatter.Visit(metadataStruct);
            }

            public void Visit(IMetadataInterface metadataInterface)
            {
                formatter.Visit(metadataInterface);
            }

            public void Visit(IMetadataEnum metadataEnum)
            {
                formatter.Visit(metadataEnum);
            }

            public void Visit(IMetadataDelegate metadataDelegate)
            {
                formatter.Visit(metadataDelegate);
            }
        }
    }
}
