using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.AssemblyReferenceResolvers
{
    public interface IAssemblyReferenceResolver
    {
        bool TryGetAssemblyPath(MetadataAssemblyReference assemblyReference, out string path);
    }
}
