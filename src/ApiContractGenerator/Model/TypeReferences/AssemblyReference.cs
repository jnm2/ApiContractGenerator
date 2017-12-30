using System;
using System.Reflection;

namespace ApiContractGenerator.Model.TypeReferences
{
    public abstract class MetadataAssemblyReference
    {
        public abstract string Name { get; }
        public abstract Version Version { get; }
        public abstract string CultureName { get; }
        public abstract byte[] PublicKeyToken { get; }
        public abstract string FullName { get; }

        public static MetadataAssemblyReference FromFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name must not be blank.", nameof(fullName));
            return new FullNameMetadataAssemblyReference(fullName);
        }

        private sealed class FullNameMetadataAssemblyReference : MetadataAssemblyReference
        {
            public override string FullName { get; }

            private AssemblyName parsed;

            public FullNameMetadataAssemblyReference(string fullName)
            {
                FullName = fullName;
            }

            private AssemblyName GetParsed() => parsed ?? (parsed = new AssemblyName(FullName));

            public override string Name => GetParsed().Name;

            public override Version Version => GetParsed().Version;

            public override string CultureName => GetParsed().CultureName;

            public override byte[] PublicKeyToken => GetParsed().GetPublicKeyToken();
        }
    }
}
