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
        private abstract class ReaderClassBase : IMetadataType
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

            private IReadOnlyList<IMetadataType> nestedTypes;
            public IReadOnlyList<IMetadataType> NestedTypes
            {
                get
                {
                    if (nestedTypes == null)
                    {
                        var r = new List<IMetadataType>();

                        foreach (var handle in Definition.GetNestedTypes())
                        {
                            var definition = Reader.GetTypeDefinition(handle);
                            switch (definition.Attributes & TypeAttributes.VisibilityMask)
                            {
                                case TypeAttributes.NestedPublic:
                                case TypeAttributes.NestedFamily:
                                case TypeAttributes.NestedFamORAssem:
                                     r.Add(Create(Reader, definition, GenericContext));
                                    break;
                            }
                        }

                        nestedTypes = r;
                    }
                    return nestedTypes;
                }
            }

            public static ReaderClassBase Create(MetadataReader reader, TypeDefinition typeDefinition, GenericContext parentGenericContext)
            {
                if ((typeDefinition.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
                {
                    return new ReaderInterface(reader, typeDefinition, parentGenericContext);
                }

                if (typeDefinition.BaseType.Kind == HandleKind.TypeReference)
                {
                    var baseType = reader.GetTypeReference((TypeReferenceHandle)typeDefinition.BaseType);
                    var baseTypeName = reader.GetString(baseType.Name);
                    var baseTypeNamespace = reader.GetString(baseType.Namespace);

                    if (baseTypeName == "Enum" && baseTypeNamespace == "System")
                    {
                        return new ReaderEnum(reader, typeDefinition, parentGenericContext);
                    }
                    if (baseTypeName == "ValueType" && baseTypeNamespace == "System")
                    {
                        return new ReaderStruct(reader, typeDefinition, parentGenericContext);
                    }
                    if (baseTypeName == "MulticastDelegate" && baseTypeNamespace == "System")
                    {
                        return new ReaderDelegate(reader, typeDefinition, parentGenericContext);
                    }
                }

                return new ReaderClass(reader, typeDefinition, parentGenericContext);
            }
        }
    }
}
