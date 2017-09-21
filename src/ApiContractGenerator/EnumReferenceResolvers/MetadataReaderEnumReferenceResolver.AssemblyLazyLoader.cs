using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using ApiContractGenerator.Model;

namespace ApiContractGenerator.EnumReferenceResolvers
{
    public sealed partial class MetadataReaderEnumReferenceResolver
    {
        private sealed partial class AssemblyLazyLoader : IDisposable
        {
            private readonly PEReader peReader;
            private readonly MetadataReader reader;
            private TypeDefinitionHandleCollection.Enumerator enumerator;
            private readonly Dictionary<NameSpec, EnumInfo> cache = new Dictionary<NameSpec, EnumInfo>();
            private bool entirelyLoaded;

            public AssemblyLazyLoader(string path)
            {
                peReader = new PEReader(File.OpenRead(path));
                reader = peReader.GetMetadataReader();
                enumerator = reader.TypeDefinitions.GetEnumerator();
            }

            public void Dispose()
            {
                peReader.Dispose();
            }

            public bool TryGetEnumInfo(NameSpec name, out EnumInfo info)
            {
                if (cache.TryGetValue(name, out info))
                    return true;

                if (entirelyLoaded) return false;

                ref var en = ref enumerator;
                while (en.MoveNext())
                {
                    var definition = reader.GetTypeDefinition(en.Current);
                    if (!TryGetEnumInfo(definition, out var currentInfo)) continue;

                    var currentName = GetNameSpec(definition);
                    cache.Add(currentName, currentInfo);

                    if (name == currentName)
                    {
                        info = currentInfo;
                        return true;
                    }
                }

                peReader.Dispose();
                entirelyLoaded = true;
                return false;
            }

            private bool TryGetEnumInfo(TypeDefinition definition, out EnumInfo info)
            {
                // Enums are sealed, not abstract, not interfaces
                if ((definition.Attributes & (TypeAttributes.Sealed | TypeAttributes.Abstract | TypeAttributes.Interface)) != TypeAttributes.Sealed)
                {
                    info = default(EnumInfo);
                    return false;
                }

                // Internals may be visible
                if ((definition.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.NestedPrivate)
                {
                    info = default(EnumInfo);
                    return false;
                }

                if (!IsSimpleNamedType(definition.BaseType, "System", "Enum"))
                {
                    info = default(EnumInfo);
                    return false;
                }

                var fields = new List<EnumFieldInfo>();
                var underlyingType = (PrimitiveTypeCode?)null;

                foreach (var fieldHandle in definition.GetFields())
                {
                    var field = reader.GetFieldDefinition(fieldHandle);
                    if ((field.Attributes & FieldAttributes.Literal) != 0)
                    {
                        switch (field.Attributes & FieldAttributes.FieldAccessMask)
                        {
                            case FieldAttributes.Private:
                            case FieldAttributes.PrivateScope:
                                continue;
                        }

                        fields.Add(new EnumFieldInfo(
                            reader.GetString(field.Name),
                            ReadConstant(field.GetDefaultValue())));
                    }
                    else if ((field.Attributes & FieldAttributes.Static) == 0 && underlyingType == null)
                    {
                        underlyingType = field.DecodeSignature(PrimitiveTypeProvider.Instance, null);
                    }
                }

                if (underlyingType == null)
                {
                    info = default(EnumInfo);
                    return false;
                }

                info = new EnumInfo(HasFlagsAttribute(definition), underlyingType.Value, fields);
                return true;
            }

            private bool IsSimpleNamedType(EntityHandle type, string @namespace, string name)
            {
                if (type.IsNil) return false;

                switch (type.Kind)
                {
                    case HandleKind.TypeReference:
                        var typeReference = reader.GetTypeReference((TypeReferenceHandle)type);
                        return reader.GetString(typeReference.Name) == name && reader.GetString(typeReference.Namespace) == @namespace;
                    case HandleKind.TypeDefinition:
                        return IsNamedType((TypeDefinitionHandle)type, @namespace, name);
                    case HandleKind.TypeSpecification:
                        return false;
                    default:
                        throw new NotImplementedException();
                }
            }

            private bool IsNamedType(TypeDefinitionHandle type, string @namespace, string name)
            {
                var typeDefinition = reader.GetTypeDefinition(type);
                return reader.GetString(typeDefinition.Name) == name && reader.GetString(typeDefinition.Namespace) == @namespace;
            }

            private bool HasFlagsAttribute(TypeDefinition definition)
            {
                foreach (var handle in definition.GetCustomAttributes())
                {
                    var constructorHandle = reader.GetCustomAttribute(handle).Constructor;

                    switch (constructorHandle.Kind)
                    {
                        case HandleKind.MemberReference:
                            var memberReference = reader.GetMemberReference((MemberReferenceHandle)constructorHandle);
                            if (IsSimpleNamedType(memberReference.Parent, "System", "FlagsAttribute")) return true;
                            break;
                        case HandleKind.MethodDefinition:
                            var constructorDefinition = reader.GetMethodDefinition((MethodDefinitionHandle)constructorHandle);
                            if (IsNamedType(constructorDefinition.GetDeclaringType(), "System", "FlagsAttribute")) return true;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                return false;
            }

            private IMetadataConstantValue ReadConstant(ConstantHandle handle)
            {
                var constant = reader.GetConstant(handle);

                var br = reader.GetBlobReader(constant.Value);

                switch (constant.TypeCode)
                {
                    case ConstantTypeCode.Boolean:
                        return MetadataConstantValue.FromObject(br.ReadBoolean());
                    case ConstantTypeCode.Char:
                        return MetadataConstantValue.FromObject(br.ReadChar());
                    case ConstantTypeCode.SByte:
                        return MetadataConstantValue.FromObject(br.ReadSByte());
                    case ConstantTypeCode.Byte:
                        return MetadataConstantValue.FromObject(br.ReadByte());
                    case ConstantTypeCode.Int16:
                        return MetadataConstantValue.FromObject(br.ReadInt16());
                    case ConstantTypeCode.UInt16:
                        return MetadataConstantValue.FromObject(br.ReadUInt16());
                    case ConstantTypeCode.Int32:
                        return MetadataConstantValue.FromObject(br.ReadInt32());
                    case ConstantTypeCode.UInt32:
                        return MetadataConstantValue.FromObject(br.ReadUInt32());
                    case ConstantTypeCode.Int64:
                        return MetadataConstantValue.FromObject(br.ReadInt64());
                    case ConstantTypeCode.UInt64:
                        return MetadataConstantValue.FromObject(br.ReadUInt64());
                    default:
                        throw new NotImplementedException();
                }
            }

            private NameSpec GetNameSpec(TypeDefinition type)
            {
                var nestedNameHandles = new List<StringHandle>();

                for (; ; )
                {
                    var declaringTypeHandle = type.GetDeclaringType();
                    if (declaringTypeHandle.IsNil) break;

                    nestedNameHandles.Add(type.Name);
                    type = reader.GetTypeDefinition(declaringTypeHandle);
                }

                string[] nestedNames;
                if (nestedNameHandles.Count == 0)
                {
                    nestedNames = null;
                }
                else
                {
                    nestedNames = new string[nestedNameHandles.Count];

                    for (var i = 0; i < nestedNames.Length; i++)
                        nestedNames[i] = reader.GetString(nestedNameHandles[nestedNames.Length - 1 - i]);
                }

                return new NameSpec(reader.GetString(type.Namespace), reader.GetString(type.Name), nestedNames);
            }
        }
    }
}
