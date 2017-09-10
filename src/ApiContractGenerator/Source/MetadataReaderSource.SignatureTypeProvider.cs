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
                return GetTypeFromTypeDefinitionHandle(reader, handle);
            }

            public MetadataTypeReference GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            {
                return GetTypeFromTypeReferenceHandle(reader, handle);
            }

            public MetadataTypeReference GetSZArrayType(MetadataTypeReference elementType)
            {
                return new ArrayTypeReference(elementType, 1);
            }

            public MetadataTypeReference GetGenericInstantiation(MetadataTypeReference genericType, ImmutableArray<MetadataTypeReference> typeArguments)
            {
                return new GenericInstantiationTypeReference(genericType, typeArguments);
            }

            public MetadataTypeReference GetArrayType(MetadataTypeReference elementType, ArrayShape shape)
            {
                if (shape.LowerBounds.Any(_ => _ != 0)) throw new NotImplementedException("Non-zero lower bounds");
                if (shape.Sizes.Any(_ => _ != 0)) throw new NotImplementedException("Sizes");
                return new ArrayTypeReference(elementType, shape.Rank);
            }

            public MetadataTypeReference GetByReferenceType(MetadataTypeReference elementType)
            {
                return new ByRefTypeReference(elementType);
            }

            public MetadataTypeReference GetPointerType(MetadataTypeReference elementType)
            {
                return new PointerTypeReference(elementType);
            }

            public MetadataTypeReference GetFunctionPointerType(MethodSignature<MetadataTypeReference> signature)
            {
                throw new NotImplementedException();
            }

            public MetadataTypeReference GetGenericMethodParameter(GenericContext genericContext, int index)
            {
                return new GenericParameterTypeReference(genericContext.MethodParameters[index].Name);
            }

            public MetadataTypeReference GetGenericTypeParameter(GenericContext genericContext, int index)
            {
                return new GenericParameterTypeReference(genericContext.TypeParameters[index].Name);
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
