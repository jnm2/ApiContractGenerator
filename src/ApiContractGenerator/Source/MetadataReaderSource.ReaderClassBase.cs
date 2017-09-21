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
            protected readonly TypeReferenceTypeProvider TypeProvider;
            protected readonly TypeDefinition Definition;

            protected ReaderClassBase(MetadataReader reader, TypeReferenceTypeProvider typeProvider, TypeDefinition definition)
            {
                Reader = reader;
                TypeProvider = typeProvider;
                Definition = definition;
            }

            private GenericContext? genericContext;
            protected GenericContext GenericContext
            {
                get
                {
                    if (genericContext == null)
                        genericContext = GenericContext.FromType(Reader, TypeProvider, Definition);
                    return genericContext.Value;
                }
            }

            private string name;
            public string Name => name ?? (name = Reader.GetString(Definition.Name));

            private IReadOnlyList<IMetadataAttribute> attributes;
            public IReadOnlyList<IMetadataAttribute> Attributes => attributes ?? (attributes =
                GetAttributes(Reader, TypeProvider, Definition.GetCustomAttributes(), GenericContext));

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

            public IReadOnlyList<IMetadataGenericTypeParameter> GenericTypeParameters => GenericContext.TypeParameters;



            private MetadataTypeReference baseType;
            private bool isBaseTypeValid;
            public MetadataTypeReference BaseType
            {
                get
                {
                    if (!isBaseTypeValid)
                    {
                        if (!Definition.BaseType.IsNil)
                            baseType = GetTypeFromEntityHandle(Reader, TypeProvider, GenericContext, Definition.BaseType);
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
                            r.Add(GetTypeFromEntityHandle(Reader, TypeProvider, GenericContext, Reader.GetInterfaceImplementation(handle).Interface));
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
                                    r.Add(new ReaderField(Reader, TypeProvider, definition, GenericContext));
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

                            r.Add(new ReaderProperty(Reader,TypeProvider,  definition, GenericContext, visibleGetter, visibleSetter));
                        }

                        properties = r;
                    }
                    return properties;
                }
            }

            private IReadOnlyList<IMetadataEvent> events;
            public IReadOnlyList<IMetadataEvent> Events
            {
                get
                {
                    if (events == null)
                    {
                        var r = new List<IMetadataEvent>();

                        foreach (var handle in Definition.GetEvents())
                        {
                            var definition = Reader.GetEventDefinition(handle);
                            var accessors = definition.GetAccessors();
                            var visibleAdder = accessors.Adder.IsNil ? null : GetVisibleMethod(accessors.Adder);
                            var visibleRemover = accessors.Remover.IsNil ? null : GetVisibleMethod(accessors.Remover);
                            var visibleRaiser = accessors.Raiser.IsNil ? null : GetVisibleMethod(accessors.Raiser);
                            if (visibleAdder == null && visibleRemover == null && visibleRaiser == null) continue;

                            r.Add(new ReaderEvent(Reader, TypeProvider, definition, GenericContext, visibleAdder, visibleRemover, visibleRaiser));
                        }

                        events = r;
                    }
                    return events;
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
                                    r.Add(new ReaderMethod(Reader, TypeProvider, definition, GenericContext));
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
                                     r.Add(Create(Reader, TypeProvider, definition));
                                    break;
                            }
                        }

                        nestedTypes = r;
                    }
                    return nestedTypes;
                }
            }

            private static bool TryGetNonGenericTypeNamespaceAndName(MetadataReader reader, EntityHandle entity, out string @namespace, out string name)
            {
                switch (entity.Kind)
                {
                    case HandleKind.TypeReference:
                        var reference = reader.GetTypeReference((TypeReferenceHandle)entity);
                        @namespace = reader.GetString(reference.Namespace);
                        name = reader.GetString(reference.Name);
                        return true;
                    case HandleKind.TypeDefinition:
                        var definition = reader.GetTypeDefinition((TypeDefinitionHandle)entity);
                        @namespace = reader.GetString(definition.Namespace);
                        name = reader.GetString(definition.Name);
                        return true;
                    default:
                        @namespace = null;
                        name = null;
                        return false;
                }
            }

            public static ReaderClassBase Create(MetadataReader reader, TypeReferenceTypeProvider typeProvider, TypeDefinition typeDefinition)
            {
                if ((typeDefinition.Attributes & TypeAttributes.ClassSemanticsMask) == TypeAttributes.Interface)
                {
                    return new ReaderInterface(reader, typeProvider, typeDefinition);
                }

                if (!typeDefinition.BaseType.IsNil
                    && TryGetNonGenericTypeNamespaceAndName(reader, typeDefinition.BaseType, out var baseTypeNamespace, out var baseTypeName))
                {
                    if (baseTypeName == "Enum" && baseTypeNamespace == "System")
                    {
                        return new ReaderEnum(reader, typeProvider, typeDefinition);
                    }
                    if (baseTypeName == "ValueType" && baseTypeNamespace == "System")
                    {
                        return new ReaderStruct(reader, typeProvider, typeDefinition);
                    }
                    if (baseTypeName == "MulticastDelegate" && baseTypeNamespace == "System")
                    {
                        return new ReaderDelegate(reader, typeProvider, typeDefinition);
                    }
                }

                return new ReaderClass(reader, typeProvider, typeDefinition);
            }
        }
    }
}
