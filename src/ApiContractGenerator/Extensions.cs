using System.Reflection;
using System.Reflection.Metadata;

namespace ApiContractGenerator
{
    internal static class Extensions
    {
        // See https://github.com/dotnet/corefx/issues/13295 and
        // https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/src/System/Reflection/Metadata/MetadataReader.netstandard.cs
        public static AssemblyName GetAssemblyName(this AssemblyReference reference, MetadataReader reader)
        {
            var flags = reference.Flags;
            var assemblyName = new AssemblyName(reader.GetString(reference.Name))
            {
                Version = reference.Version,
                CultureName = reference.Culture.IsNil ? null : reader.GetString(reference.Culture),
                Flags = (AssemblyNameFlags)(reference.Flags & (AssemblyFlags.PublicKey | AssemblyFlags.Retargetable | AssemblyFlags.EnableJitCompileTracking | AssemblyFlags.DisableJitCompileOptimizer)),
                ContentType = (AssemblyContentType)((int)(flags & AssemblyFlags.ContentTypeMask) >> 9)
            };

            var publicKeyOrToken = reference.PublicKeyOrToken.IsNil ? null : reader.GetBlobBytes(reference.PublicKeyOrToken);

            if ((flags & AssemblyFlags.PublicKey) != 0)
                assemblyName.SetPublicKey(publicKeyOrToken);
            else
                assemblyName.SetPublicKeyToken(publicKeyOrToken);

            return assemblyName;
        }
    }
}
