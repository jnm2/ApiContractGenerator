using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
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
            protected GenericContext GenericContext => genericContext ?? (genericContext =
                GenericContext.Create(Reader, parentGenericContext, Definition.GetGenericParameters()));

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



            private MetadataTypeReference baseType;
            private bool isBaseTypeValid;
            public MetadataTypeReference BaseType
            {
                get
                {
                    if (!isBaseTypeValid)
                    {
                        if (!Definition.BaseType.IsNil)
                            baseType = GetTypeFromEntityHandle(Reader, GenericContext, Definition.BaseType);
                        isBaseTypeValid = true;
                    }
                    return baseType;
                }
            }

            private IReadOnlyList<MetadataTypeReference> interfaceImplementations;
            public IReadOnlyList<MetadataTypeReference> InterfaceImplementations
            {
                get
                {
                    if (interfaceImplementations == null)
                    {
                        var handles = Definition.GetInterfaceImplementations();
                        var r = new List<MetadataTypeReference>(handles.Count);

                        foreach (var handle in handles)
                        {
                            r.Add(GetTypeFromEntityHandle(Reader, genericContext, Reader.GetInterfaceImplementation(handle).Interface));
                        }

                        interfaceImplementations = r;
                    }
                    return interfaceImplementations;
                }
            }

            private IReadOnlyList<IMetadataField> fields;
            public IReadOnlyList<IMetadataField> Fields
            {
                get
                {
                    if (fields == null)
                    {
                        var r = new List<IMetadataField>();

                        foreach (var handle in Definition.GetFields())
                        {
                            var definition = Reader.GetFieldDefinition(handle);
                            switch (definition.Attributes & FieldAttributes.FieldAccessMask)
                            {
                                case FieldAttributes.Public:
                                case FieldAttributes.Family:
                                case FieldAttributes.FamORAssem:
                                    r.Add(new ReaderField(Reader, definition, GenericContext));
                                    break;
                            }
                        }

                        fields = r;
                    }
                    return fields;
                }
            }

            private IReadOnlyList<IMetadataProperty> properties;
            public IReadOnlyList<IMetadataProperty> Properties
            {
                get
                {
                    if (properties == null)
                    {
                        var r = new List<IMetadataProperty>();

                        foreach (var handle in Definition.GetProperties())
                        {
                            var definition = Reader.GetPropertyDefinition(handle);
                            var accessors = definition.GetAccessors();
                            var visibleGetter = accessors.Getter.IsNil ? null : GetVisibleMethod(accessors.Getter);
                            var visibleSetter = accessors.Setter.IsNil ? null : GetVisibleMethod(accessors.Setter);
                            if (visibleGetter == null && visibleSetter == null) continue;

                            r.Add(new ReaderProperty(Reader, definition, GenericContext, visibleGetter, visibleSetter));
                        }

                        properties = r;
                    }
                    return properties;
                }
            }

            private IMetadataMethod GetVisibleMethod(MethodDefinitionHandle handle)
            {
                var methods = Methods;
                var index = visibleMethodHandles.IndexOf(handle);
                return index == -1 ? null : methods[index];
            }

            private List<MethodDefinitionHandle> visibleMethodHandles;
            private IReadOnlyList<IMetadataMethod> methods;
            public IReadOnlyList<IMetadataMethod> Methods
            {
                get
                {
                    if (methods == null)
                    {
                        var r = new List<IMetadataMethod>();
                        visibleMethodHandles = new List<MethodDefinitionHandle>();

                        foreach (var handle in Definition.GetMethods())
                        {
                            var definition = Reader.GetMethodDefinition(handle);
                            switch (definition.Attributes & MethodAttributes.MemberAccessMask)
                            {
                                case MethodAttributes.Public:
                                case MethodAttributes.Family:
                                case MethodAttributes.FamORAssem:
                                    r.Add(new ReaderMethod(Reader, definition, GenericContext));
                                    visibleMethodHandles.Add(handle);
                                    break;
                            }
                        }

                        methods = r;
                    }
                    return methods;
                }
            }

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
