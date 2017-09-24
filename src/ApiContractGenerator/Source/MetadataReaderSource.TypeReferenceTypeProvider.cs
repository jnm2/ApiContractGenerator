using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class TypeReferenceTypeProvider : ICustomAttributeTypeProvider<MetadataTypeReference>, ISignatureTypeProvider<MetadataTypeReference, GenericContext>
        {
            private readonly IMetadataReferenceResolver metadataReferenceResolver;

            public TypeReferenceTypeProvider(IMetadataReferenceResolver metadataReferenceResolver)
            {
                this.metadataReferenceResolver = metadataReferenceResolver;
            }

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

            private static readonly TopLevelTypeReference SystemType = new TopLevelTypeReference(null, "System", "Type");
            public MetadataTypeReference GetSystemType() => SystemType;

            public bool IsSystemType(MetadataTypeReference type)
            {
                return type is TopLevelTypeReference topLevel
                    && topLevel.Name == "Type"
                    && topLevel.Namespace == "System";
            }

            public MetadataTypeReference GetTypeFromSerializedName(string name)
            {
                var assemblyNameStartSplit = name.IndexOf(',');
                var assemblyName = assemblyNameStartSplit != -1 ? new AssemblyName(name.Substring(assemblyNameStartSplit + 1)) : null;

                var typeFullName = assemblyNameStartSplit == -1 ? name : name.Slice(0, assemblyNameStartSplit);

                var nestedNameSplit = typeFullName.IndexOf('+');
                var topLevelName = nestedNameSplit == -1 ? typeFullName : typeFullName.Slice(0, nestedNameSplit);

                var namespaceEndSplit = topLevelName.LastIndexOf('.');

                var topLevelNamespace = namespaceEndSplit == -1 ? null : topLevelName.Slice(0, namespaceEndSplit).ToString();
                var topLevelTypeName = topLevelName.Slice(namespaceEndSplit + 1).ToString();

                MetadataTypeReference current = new TopLevelTypeReference(assemblyName, topLevelNamespace, topLevelTypeName);

                if (nestedNameSplit == -1) return current;

                var remainingNestedName = typeFullName.Slice(nestedNameSplit + 1);

                for (;;)
                {
                    nestedNameSplit = remainingNestedName.IndexOf('+');
                    if (nestedNameSplit == -1) return new NestedTypeReference(current, remainingNestedName.ToString());
                    current = new NestedTypeReference(current, remainingNestedName.Slice(0, nestedNameSplit).ToString());
                    remainingNestedName = remainingNestedName.Slice(nestedNameSplit + 1);
                }
            }

            public PrimitiveTypeCode GetUnderlyingEnumType(MetadataTypeReference type)
            {
                return metadataReferenceResolver.TryGetEnumInfo(type, out var info)
                    ? info.UnderlyingType
                    : PrimitiveTypeCode.Int32;
            }
        }
    }
}
