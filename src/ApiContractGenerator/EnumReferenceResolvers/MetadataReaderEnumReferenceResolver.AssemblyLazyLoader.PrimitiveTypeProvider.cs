using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    partial class MetadataReaderEnumReferenceResolver
    {
        partial class AssemblyLazyLoader
        {
            private sealed class PrimitiveTypeProvider : ISignatureTypeProvider<PrimitiveTypeCode?, object>
            {
                public static readonly PrimitiveTypeProvider Instance = new PrimitiveTypeProvider();
                private PrimitiveTypeProvider() { }

                public PrimitiveTypeCode? GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode;

                public PrimitiveTypeCode? GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) => null;

                public PrimitiveTypeCode? GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) => null;

                public PrimitiveTypeCode? GetSZArrayType(PrimitiveTypeCode? elementType) => null;

                public PrimitiveTypeCode? GetGenericInstantiation(PrimitiveTypeCode? genericType, ImmutableArray<PrimitiveTypeCode?> typeArguments) => null;

                public PrimitiveTypeCode? GetArrayType(PrimitiveTypeCode? elementType, ArrayShape shape) => null;

                public PrimitiveTypeCode? GetByReferenceType(PrimitiveTypeCode? elementType) => null;

                public PrimitiveTypeCode? GetPointerType(PrimitiveTypeCode? elementType) => null;

                public PrimitiveTypeCode? GetFunctionPointerType(MethodSignature<PrimitiveTypeCode?> signature) => null;

                public PrimitiveTypeCode? GetGenericMethodParameter(object genericContext, int index) => null;

                public PrimitiveTypeCode? GetGenericTypeParameter(object genericContext, int index) => null;

                public PrimitiveTypeCode? GetModifiedType(PrimitiveTypeCode? modifier, PrimitiveTypeCode? unmodifiedType, bool isRequired) => null;

                public PrimitiveTypeCode? GetPinnedType(PrimitiveTypeCode? elementType) => null;

                public PrimitiveTypeCode? GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => null;
            }
        }
    }
}
