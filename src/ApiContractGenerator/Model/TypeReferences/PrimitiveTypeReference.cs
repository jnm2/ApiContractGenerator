using System.Reflection.Metadata;

namespace ApiContractGenerator.Model.TypeReferences
{
    public sealed class PrimitiveTypeReference : MetadataTypeReference
    {
        public PrimitiveTypeCode Code { get; }

        public PrimitiveTypeReference(PrimitiveTypeCode code)
        {
            Code = code;
        }

        public override T Accept<T>(IMetadataTypeReferenceVisitor<T> visitor) => visitor.Visit(this);
    }
}
