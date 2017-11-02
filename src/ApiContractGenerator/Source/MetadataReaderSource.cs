using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource : IMetadataSource, IDisposable
    {
        private readonly PEReader peReader;
        private readonly MetadataReader reader;
        private readonly TypeReferenceTypeProvider typeProvider;

        public MetadataReaderSource(Stream stream, IMetadataReferenceResolver metadataReferenceResolver)
        {
            peReader = new PEReader(stream);
            reader = peReader.GetMetadataReader();
            typeProvider = new TypeReferenceTypeProvider(metadataReferenceResolver);
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
                                externallyVisibleTypes.Add(ReaderClassBase.Create(reader, typeProvider, typeDefinition));
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


        private static MetadataTypeReference GetTypeFromEntityHandle(MetadataReader reader, TypeReferenceTypeProvider typeProvider, GenericContext genericContext, EntityHandle handle)
        {
            switch (handle.Kind)
            {
                case HandleKind.TypeReference:
                    return GetTypeFromTypeReferenceHandle(reader, (TypeReferenceHandle)handle);
                case HandleKind.TypeDefinition:
                    return GetTypeFromTypeDefinitionHandle(reader, (TypeDefinitionHandle)handle);
                case HandleKind.TypeSpecification:
                    var baseTypeSpecification = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                    return baseTypeSpecification.DecodeSignature(typeProvider, genericContext);
                default:
                    throw new NotImplementedException();
            }
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

            AssemblyName assemblyName;
            switch (type.ResolutionScope.Kind)
            {
                case HandleKind.ModuleReference:
                    assemblyName = null;
                    break;
                case HandleKind.AssemblyReference:
                    assemblyName = reader.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope)
                        .GetAssemblyName(reader);
                    break;
                default:
                    throw new NotImplementedException();
            }

            MetadataTypeReference current = new TopLevelTypeReference(
                assemblyName,
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

        private static IReadOnlyList<IMetadataAttribute> GetAttributes(MetadataReader reader, TypeReferenceTypeProvider typeProvider, CustomAttributeHandleCollection handles, GenericContext genericContext)
        {
            var r = new List<IMetadataAttribute>(handles.Count);
            foreach (var handle in handles)
                r.Add(new ReaderAttribute(reader, typeProvider, handle, genericContext));
            return r;
        }
    }
}
