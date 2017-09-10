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
                    var baseTypeReference = reader.GetTypeReference((TypeReferenceHandle)handle);
                    return new NamespaceTypeReference(reader.GetString(baseTypeReference.Namespace), reader.GetString(baseTypeReference.Name));
                case HandleKind.TypeDefinition:
                    return GetTypeFromTypeDefinitionHandle(reader, (TypeDefinitionHandle)handle);
                case HandleKind.TypeSpecification:
                    var baseTypeSpecification = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                    return baseTypeSpecification.DecodeSignature(SignatureTypeProvider.Instance, genericContext);
                default:
                    throw new NotImplementedException();
            }
        }

        private static MetadataTypeReference GetTypeFromTypeDefinitionHandle(MetadataReader reader, TypeDefinitionHandle handle)
        {
            var baseTypeDefinition = reader.GetTypeDefinition(handle);

            var declaringType = baseTypeDefinition.GetDeclaringType();
            if (!declaringType.IsNil)
                return new NestedTypeReference(GetTypeFromTypeDefinitionHandle(reader, declaringType), reader.GetString(baseTypeDefinition.Name));

            return new NamespaceTypeReference(reader.GetString(baseTypeDefinition.Namespace), reader.GetString(baseTypeDefinition.Name));
        }
    }
}
