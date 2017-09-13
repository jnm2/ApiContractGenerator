using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource : IMetadataSource, IDisposable
    {
        private readonly PEReader peReader;
        private readonly MetadataReader reader;

        public MetadataReaderSource(Stream stream)
        {
            peReader = new PEReader(stream);
            reader = peReader.GetMetadataReader();
        }

        public void Dispose()
        {
            peReader.Dispose();
        }

        private IReadOnlyList<IMetadataNamespace> namespaces;
        public IReadOnlyList<IMetadataNamespace> Namespaces
        {
            get
            {
                if (namespaces == null)
                {
                    var r = new List<IMetadataNamespace>();

                    VisitNamespace(null, reader.GetNamespaceDefinitionRoot());
                    void VisitNamespace(ReaderNamespace parent, NamespaceDefinition definition)
                    {
                        var externallyVisibleTypes = new List<ReaderClassBase>();

                        foreach (var handle in definition.TypeDefinitions)
                        {
                            var typeDefinition = reader.GetTypeDefinition(handle);
                            if ((typeDefinition.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public)
                                externallyVisibleTypes.Add(ReaderClassBase.Create(reader, typeDefinition));
                        }

                        var metadataNamespace = new ReaderNamespace(reader, parent, definition, externallyVisibleTypes);
                        if (externallyVisibleTypes.Count != 0)
                            r.Add(metadataNamespace);

                        foreach (var handle in definition.NamespaceDefinitions)
                            VisitNamespace(metadataNamespace, reader.GetNamespaceDefinition(handle));
                    }

                    namespaces = r;
                }
                return namespaces;
            }
        }


        private static MetadataTypeReference GetTypeFromEntityHandle(MetadataReader reader, GenericContext genericContext, EntityHandle handle)
        {
            switch (handle.Kind)
            {
                case HandleKind.TypeReference:
                    return GetTypeFromTypeReferenceHandle(reader, (TypeReferenceHandle)handle);
                case HandleKind.TypeDefinition:
                    return GetTypeFromTypeDefinitionHandle(reader, (TypeDefinitionHandle)handle);
                case HandleKind.TypeSpecification:
                    var baseTypeSpecification = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                    return baseTypeSpecification.DecodeSignature(TypeReferenceTypeProvider.Instance, genericContext);
                default:
                    throw new NotImplementedException();
            }
        }

        // See https://github.com/dotnet/corefx/issues/13295 and
        // https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/src/System/Reflection/Metadata/MetadataReader.netstandard.cs
        private static AssemblyName GetAssemblyNameFromAssemblyReference(MetadataReader reader, AssemblyReference reference)
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

        private static MetadataTypeReference GetTypeFromTypeReferenceHandle(MetadataReader reader, TypeReferenceHandle handle)
        {
            var nestedTypeNames = new List<StringHandle>();

            var type = reader.GetTypeReference(handle);

            while (type.ResolutionScope.Kind == HandleKind.TypeReference)
            {
                nestedTypeNames.Add(type.Name);
                type = reader.GetTypeReference((TypeReferenceHandle)type.ResolutionScope);
            }

            var assemblyReference = reader.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope);

            MetadataTypeReference current = new TopLevelTypeReference(
                GetAssemblyNameFromAssemblyReference(reader, assemblyReference),
                reader.GetString(type.Namespace),
                reader.GetString(type.Name));

            for (var i = nestedTypeNames.Count - 1; i >= 0; i--)
            {
                current = new NestedTypeReference(current, reader.GetString(nestedTypeNames[i]));
            }

            return current;
        }

        private static MetadataTypeReference GetTypeFromTypeDefinitionHandle(MetadataReader reader, TypeDefinitionHandle handle)
        {
            var baseTypeDefinition = reader.GetTypeDefinition(handle);

            var declaringType = baseTypeDefinition.GetDeclaringType();
            if (!declaringType.IsNil)
                return new NestedTypeReference(GetTypeFromTypeDefinitionHandle(reader, declaringType), reader.GetString(baseTypeDefinition.Name));

            return new TopLevelTypeReference(null, reader.GetString(baseTypeDefinition.Namespace), reader.GetString(baseTypeDefinition.Name));
        }

        private static IReadOnlyList<IMetadataAttribute> GetAttributes(MetadataReader reader, CustomAttributeHandleCollection handles, GenericContext genericContext)
        {
            var r = new List<IMetadataAttribute>(handles.Count);
            foreach (var handle in handles)
                r.Add(new ReaderAttribute(reader, handle, genericContext));
            return r;
        }
    }
}
