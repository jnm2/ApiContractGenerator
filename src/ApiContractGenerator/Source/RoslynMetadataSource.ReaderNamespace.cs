using System.Collections.Generic;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private sealed class ReaderNamespace : IMetadataNamespace
        {
            private readonly MetadataReader reader;
            private readonly ReaderNamespace parent;
            private readonly NamespaceDefinition definition;
            private readonly IReadOnlyList<TypeDefinition> externallyVisibleTypes;

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

            public ReaderNamespace(MetadataReader reader, ReaderNamespace parent, NamespaceDefinition definition, IReadOnlyList<TypeDefinition> externallyVisibleTypes)
            {
                this.reader = reader;
                this.parent = parent;
                this.definition = definition;
                this.externallyVisibleTypes = externallyVisibleTypes;
            }

            public void Accept(IMetadataVisitor visitor)
            {
                foreach (var type in externallyVisibleTypes)
                    Dispatch(reader, type, default(GenericContext), visitor);
            }
        }
    }
}
