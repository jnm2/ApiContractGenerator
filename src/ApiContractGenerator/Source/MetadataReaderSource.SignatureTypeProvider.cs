using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class SignatureTypeProvider : ISignatureTypeProvider<MetadataTypeReference, GenericContext>
        {
            public static readonly SignatureTypeProvider Instance = new SignatureTypeProvider();
            private SignatureTypeProvider() { }

            public MetadataTypeReference GetPrimitiveType(PrimitiveTypeCode typeCode)
            {
                return new PrimitiveTypeReference(typeCode);
            }

            public MetadataTypeReference GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            {
                var definition = reader.GetTypeDefinition(handle);
                return new NamespaceTypeReference(reader.GetString(definition.Namespace), reader.GetString(definition.Name));
            }

            public MetadataTypeReference GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            {
                var reference = reader.GetTypeReference(handle);
                return new NamespaceTypeReference(reader.GetString(reference.Namespace), reader.GetString(reference.Name));
            }

            public MetadataTypeReference GetSZArrayType(MetadataTypeReference elementType)
            {
                return new ArrayTypeReference(elementType, 1);
            }

            public MetadataTypeReference GetGenericInstantiation(MetadataTypeReference genericType, ImmutableArray<MetadataTypeReference> typeArguments)
            {
                return new GenericInstantiationTypeReference((NamespaceTypeReference)genericType, typeArguments);
            }

            public MetadataTypeReference GetArrayType(MetadataTypeReference elementType, ArrayShape shape)
            {
                if (shape.LowerBounds.Any(_ => _ != 0)) throw new NotImplementedException("Non-zero lower bounds");
                if (shape.Sizes.Any(_ => _ != 0)) throw new NotImplementedException("Sizes");
                return new ArrayTypeReference(elementType, shape.Rank);
            }

            public MetadataTypeReference GetByReferenceType(MetadataTypeReference elementType)
            {
                throw new NotImplementedException();
            }

            public MetadataTypeReference GetPointerType(MetadataTypeReference elementType)
            {
                throw new NotImplementedException();
            }

            public MetadataTypeReference GetFunctionPointerType(MethodSignature<MetadataTypeReference> signature)
            {
                throw new NotImplementedException();
            }

            public MetadataTypeReference GetGenericMethodParameter(GenericContext genericContext, int index)
            {
                return genericContext.TypeParameters[index];
            }

            public MetadataTypeReference GetGenericTypeParameter(GenericContext genericContext, int index)
            {
                return genericContext.TypeParameters[index];
            }

            public MetadataTypeReference GetModifiedType(MetadataTypeReference modifier, MetadataTypeReference unmodifiedType, bool isRequired)
            {
                throw new NotImplementedException();
            }

            public MetadataTypeReference GetPinnedType(MetadataTypeReference elementType)
            {
                throw new NotImplementedException();
            }

            public MetadataTypeReference GetTypeFromSpecification(MetadataReader reader, GenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
            {
                throw new NotImplementedException();
            }
        }
    }
}
