using System.Reflection;

namespace ApiContractGenerator.AssemblyReferenceResolvers
{
    public interface IAssemblyReferenceResolver
    {
        bool TryGetAssemblyPath(AssemblyName assemblyName, out string path);
    }
}
