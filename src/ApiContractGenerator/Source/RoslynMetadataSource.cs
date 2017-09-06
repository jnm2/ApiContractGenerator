using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource : IMetadataSource, IDisposable
    {
        private readonly PEReader peReader;
        private readonly MetadataReader reader;

        public RoslynMetadataSource(Stream stream)
        {
            peReader = new PEReader(stream);
            reader = peReader.GetMetadataReader();
        }

        public void Dispose()
        {
            peReader.Dispose();
        }

        public void Accept(IMetadataVisitor visitor)
        {
            VisitNamespace(null, reader.GetNamespaceDefinitionRoot());
            void VisitNamespace(ReaderNamespace parent, NamespaceDefinition definition)
            {
                var externallyVisibleTypes = new List<TypeDefinition>();

                foreach (var handle in definition.TypeDefinitions)
                {
                    var typeDefinition = reader.GetTypeDefinition(handle);
                    if ((typeDefinition.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public)
                        externallyVisibleTypes.Add(typeDefinition);
                }

                var metadataNamespace = new ReaderNamespace(reader, parent, definition, externallyVisibleTypes);
                if (externallyVisibleTypes.Count != 0)
                    visitor.Visit(metadataNamespace);

                foreach (var handle in definition.NamespaceDefinitions)
                    VisitNamespace(metadataNamespace, reader.GetNamespaceDefinition(handle));
            }
        }


        private static void Dispatch(MetadataReader reader, TypeDefinition typeDefinition, GenericContext parentGenericContext, IMetadataVisitor visitor)
        {
            if ((typeDefinition.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
            {
                visitor.Visit(new ReaderInterface(reader, typeDefinition, parentGenericContext));
                return;
            }

            if (typeDefinition.BaseType.Kind == HandleKind.TypeReference)
            {
                var baseType = reader.GetTypeReference((TypeReferenceHandle)typeDefinition.BaseType);
                var baseTypeName = reader.GetString(baseType.Name);
                var baseTypeNamespace = reader.GetString(baseType.Namespace);

                if (baseTypeName == "Enum" && baseTypeNamespace == "System")
                {
                    visitor.Visit(new ReaderEnum(reader, typeDefinition, parentGenericContext));
                    return;
                }
                if (baseTypeName == "ValueType" && baseTypeNamespace == "System")
                {
                    visitor.Visit(new ReaderStruct(reader, typeDefinition, parentGenericContext));
                    return;
                }
                if (baseTypeName == "MulticastDelegate" && baseTypeNamespace == "System")
                {
                    visitor.Visit(new ReaderDelegate(reader, typeDefinition, parentGenericContext));
                    return;
                }
            }

            visitor.Visit(new ReaderClass(reader, typeDefinition, parentGenericContext));
        }
    }
}
