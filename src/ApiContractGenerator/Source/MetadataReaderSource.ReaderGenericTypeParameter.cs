using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderGenericTypeParameter : IMetadataGenericTypeParameter
        {
            private readonly MetadataReader reader;
            private readonly GenericParameter definition;
            private readonly GenericContext genericContext;

            public ReaderGenericTypeParameter(MetadataReader reader, GenericParameter definition, GenericContext genericContext)
            {
                this.reader = reader;
                this.definition = definition;
                this.genericContext = genericContext;
            }

            private string name;
            public string Name => name ?? (name = reader.GetString(definition.Name));

            private IReadOnlyList<IMetadataAttribute> attributes;
            public IReadOnlyList<IMetadataAttribute> Attributes => attributes ?? (attributes =
                GetAttributes(reader, definition.GetCustomAttributes(), genericContext));

            public bool IsCovariant => (definition.Attributes & GenericParameterAttributes.Covariant) != 0;
            public bool IsContravariant => (definition.Attributes & GenericParameterAttributes.Contravariant) != 0;
            public bool HasReferenceTypeConstraint => (definition.Attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            public bool HasNotNullableValueTypeConstraint => (definition.Attributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
            public bool HasDefaultConstructorConstraint => (definition.Attributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

            private MetadataTypeReference[] typeConstraints;
            public IReadOnlyList<MetadataTypeReference> TypeConstraints
            {
                get
                {
                    if (typeConstraints == null)
                    {
                        var handles = definition.GetConstraints();
                        typeConstraints = new MetadataTypeReference[handles.Count];

                        for (var i = 0; i < typeConstraints.Length; i++)
                        {
                            typeConstraints[i] = GetTypeFromEntityHandle(reader, genericContext, reader.GetGenericParameterConstraint(handles[i]).Type);
                        }
                    }
                    return typeConstraints;
                }
            }
        }
    }
}
