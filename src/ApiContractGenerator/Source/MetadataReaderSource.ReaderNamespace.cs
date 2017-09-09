using System.Collections.Generic;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderNamespace : IMetadataNamespace
        {
            private readonly MetadataReader reader;
            private readonly ReaderNamespace parent;
            private readonly NamespaceDefinition definition;

            public IReadOnlyList<IMetadataType> Types { get; }

            private string name;
            public string Name
            {
                get
                {
                    if (name == null)
                    {
                        name = reader.GetString(definition.Name);
                        if (parent != null)
                        {
                            var parentName = parent.Name;
                            if (parentName.Length != 0) name = parent.Name + '.' + name;
                        }
                    }
                    return name;
                }
            }

            public ReaderNamespace(MetadataReader reader, ReaderNamespace parent, NamespaceDefinition definition, IReadOnlyList<IMetadataType> types)
            {
                this.reader = reader;
                this.parent = parent;
                this.definition = definition;
                Types = types;
            }
        }
    }
}
