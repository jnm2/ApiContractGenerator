using System.Reflection;

namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class TopLevelTypeReference : MetadataTypeReference
    {
        public AssemblyName Assembly { get; }
        public string Namespace { get; }
        public string Name { get; }

        public TopLevelTypeReference(AssemblyName assembly, string @namespace, string name)
        {
            Assembly = assembly;
            Namespace = @namespace;
            Name = name;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
