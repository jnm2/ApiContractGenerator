using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class RoslynMetadataSource
    {
        private abstract class ReaderClassBase : IMetadataType, IMetadataSource
        {
            protected readonly MetadataReader Reader;
            protected readonly TypeDefinition Definition;
            private readonly GenericContext parentGenericContext;

            protected ReaderClassBase(MetadataReader reader, TypeDefinition definition, GenericContext parentGenericContext)
            {
                Reader = reader;
                Definition = definition;
                this.parentGenericContext = parentGenericContext;
            }

            private GenericContext genericContext;
            protected GenericContext GenericContext
            {
                get
                {
                    if (genericContext == null)
                    {
                        var genericParameters = Definition.GetGenericParameters();
                        if (genericParameters.Count == 0)
                        {
                            genericContext = parentGenericContext;
                        }
                        else
                        {
                            var childParameters = new GenericParameterTypeReference[genericParameters.Count];
                            foreach (var handle in genericParameters)
                            {
                                var genericParameter = Reader.GetGenericParameter(handle);
                                childParameters[genericParameter.Index] = new GenericParameterTypeReference(Reader.GetString(genericParameter.Name));
                            }
                            genericContext = new GenericContext(parentGenericContext, childParameters);
                        }
                    }
                    return genericContext;
                }
            }

            private string name;
            public string Name => name ?? (name = Reader.GetString(Definition.Name));

            public MetadataVisibility Visibility
            {
                get
                {
                    switch (Definition.Attributes & TypeAttributes.VisibilityMask)
                    {
                        case TypeAttributes.Public:
                        case TypeAttributes.NestedPublic:
                            return MetadataVisibility.Public;
                        case TypeAttributes.NestedFamily:
                        case TypeAttributes.NestedFamORAssem:
                            return MetadataVisibility.Protected;
                        default:
                            return 0;
                    }
                }
            }

            public IReadOnlyList<GenericParameterTypeReference> GenericTypeParameters =>
                GenericContext == null
                    ? Array.Empty<GenericParameterTypeReference>()
                    : GenericContext.TypeParameters;

            public void Accept(IMetadataVisitor visitor)
            {
                foreach (var handle in Definition.GetNestedTypes())
                {
                    var definition = Reader.GetTypeDefinition(handle);
                    switch (definition.Attributes & TypeAttributes.VisibilityMask)
                    {
                        case TypeAttributes.NestedPublic:
                        case TypeAttributes.NestedFamily:
                        case TypeAttributes.NestedFamORAssem:
                            Dispatch(Reader, definition, GenericContext, visitor);
                            break;
                    }
                }
            }
        }
    }
}
