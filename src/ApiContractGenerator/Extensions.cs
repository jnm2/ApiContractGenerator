using System;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    // Not in Internals namespace because it would introduce using directives.
    // This is fine so long as the class itself is internal.
    internal static class Extensions
    {
        public static MetadataAssemblyReference GetAssemblyName(this AssemblyReference reference, MetadataReader reader)
        {
            return new ReaderMetadataAssemblyReference(
                reader.GetString(reference.Name),
                reference.Version,
                reference.Culture.IsNil ? null : reader.GetString(reference.Culture),
                reference.PublicKeyOrToken.IsNil ? null : reader.GetBlobBytes(reference.PublicKeyOrToken),
                reference.Flags);
        }

        private sealed class ReaderMetadataAssemblyReference : MetadataAssemblyReference
        {
            public override string Name { get; }
            public override Version Version { get; }
            public override string CultureName { get; }

            private readonly byte[] publicKeyOrToken;
            private readonly AssemblyFlags flags;

            private AssemblyName parsed;

            public ReaderMetadataAssemblyReference(string name, Version version, string cultureName, byte[] publicKeyOrToken, AssemblyFlags flags)
            {
                Name = name;
                Version = version;
                CultureName = cultureName;
                this.publicKeyOrToken = publicKeyOrToken;
                this.flags = flags;
            }

            private AssemblyName GetParsed()
            {
                if (parsed == null)
                {
                    parsed = new AssemblyName
                    {
                        Name = Name,
                        Version = Version,
                        Flags = (AssemblyNameFlags)(flags & (AssemblyFlags.PublicKey | AssemblyFlags.Retargetable | AssemblyFlags.EnableJitCompileTracking | AssemblyFlags.DisableJitCompileOptimizer)),
                        ContentType = (AssemblyContentType)((int)(flags & AssemblyFlags.ContentTypeMask) >> 9)
                    };

                    if (CultureName != null) parsed.CultureName = CultureName;

                    if (IsPublicKey)
                        parsed.SetPublicKey(publicKeyOrToken);
                    else
                        parsed.SetPublicKeyToken(publicKeyOrToken);
                }
                return parsed;
            }

            private bool IsPublicKey => (flags & AssemblyFlags.PublicKey) != 0;

            public override byte[] PublicKeyToken => IsPublicKey ? GetParsed().GetPublicKeyToken() : publicKeyOrToken;

            public override string FullName => GetParsed().FullName;
        }
    }
}
