using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed class CSharpTextFormatter : IMetadataWriter
    {
        private readonly IndentedTextWriter writer;

        public CSharpTextFormatter(TextWriter writer)
        {
            this.writer = new IndentedTextWriter(writer);
        }

        public void Write(IMetadataSource metadataSource)
        {
            foreach (var metadataNamespace in metadataSource.Namespaces.OrderBy(_ => _.Name))
                Write(metadataNamespace);
        }

        public void Write(IMetadataNamespace metadataNamespace)
        {
            writer.Write("namespace ");
            writer.WriteLine(metadataNamespace.Name);
            writer.WriteLine('{');
            writer.Indent();

            foreach (var type in metadataNamespace.Types.OrderBy(_ => _.Name))
                Write(type);

            writer.Unindent();
            writer.WriteLine('}');
        }

        private void WriteVisibility(MetadataVisibility visibility)
        {
            switch (visibility)
            {
                case MetadataVisibility.Public:
                    writer.Write("public ");
                    break;
                case MetadataVisibility.Protected:
                    writer.Write("protected ");
                    break;
            }
        }

        private void WriteNameAndGenericSignature(IMetadataType type)
        {
            var genericParameters = type.GenericTypeParameters;
            if (genericParameters.Count == 0)
            {
                writer.Write(type.Name);
            }
            else
            {
                var genericSuffixIndex = type.Name.LastIndexOf('`');
                var buffer = new char[genericSuffixIndex];
                type.Name.CopyTo(0, buffer, 0, buffer.Length);
                writer.Write(buffer);
                writer.Write('<');

                for (var i = 0; i < genericParameters.Count; i++)
                {
                    if (i != 0) writer.Write(", ");
                    writer.Write(genericParameters[i].Name);
                }

                writer.Write('>');
            }
        }

        public void WriteStringLiteral(string literal)
        {
            writer.Write('"');
            foreach (var c in literal)
            {
                switch (c)
                {
                    case '\0':
                        writer.Write("\\0");
                        break;
                    case '\x1':
                        writer.Write("\\x1");
                        break;
                    case '\x2':
                        writer.Write("\\x2");
                        break;
                    case '\x3':
                        writer.Write("\\x3");
                        break;
                    case '\x4':
                        writer.Write("\\x4");
                        break;
                    case '\x5':
                        writer.Write("\\x5");
                        break;
                    case '\x6':
                        writer.Write("\\x6");
                        break;
                    case '\a':
                        writer.Write("\\a");
                        break;
                    case '\b':
                        writer.Write("\\b");
                        break;
                    case '\t':
                        writer.Write("\\t");
                        break;
                    case '\n':
                        writer.Write("\\n");
                        break;
                    case '\v':
                        writer.Write("\\v");
                        break;
                    case '\f':
                        writer.Write("\\f");
                        break;
                    case '\r':
                        writer.Write("\\r");
                        break;
                    case '\xE':
                        writer.Write("\\xE");
                        break;
                    case '\xF':
                        writer.Write("\\xF");
                        break;
                    case '\x10':
                        writer.Write("\\x10");
                        break;
                    case '\x11':
                        writer.Write("\\x11");
                        break;
                    case '\x12':
                        writer.Write("\\x12");
                        break;
                    case '\x13':
                        writer.Write("\\x13");
                        break;
                    case '\x14':
                        writer.Write("\\x14");
                        break;
                    case '\x15':
                        writer.Write("\\x15");
                        break;
                    case '\x16':
                        writer.Write("\\x16");
                        break;
                    case '\x17':
                        writer.Write("\\x17");
                        break;
                    case '\x18':
                        writer.Write("\\x18");
                        break;
                    case '\x19':
                        writer.Write("\\x19");
                        break;
                    case '\x1A':
                        writer.Write("\\x1A");
                        break;
                    case '\x1B':
                        writer.Write("\\x1B");
                        break;
                    case '\x1C':
                        writer.Write("\\x1C");
                        break;
                    case '\x1D':
                        writer.Write("\\x1D");
                        break;
                    case '\x1E':
                        writer.Write("\\x1E");
                        break;
                    case '\x1F':
                        writer.Write("\\x1F");
                        break;
                    case '"':
                        writer.Write("\\\"");
                        break;
                    case '\\':
                        writer.Write("\\\\");
                        break;
                    default:
                        writer.Write(c);
                        break;
                }
            }
            writer.Write('"');
        }

        public void WriteCharLiteral(char literal)
        {
            writer.Write('\'');
            switch (literal)
            {
                case '\0':
                    writer.Write("\\0");
                    break;
                case '\x1':
                    writer.Write("\\x1");
                    break;
                case '\x2':
                    writer.Write("\\x2");
                    break;
                case '\x3':
                    writer.Write("\\x3");
                    break;
                case '\x4':
                    writer.Write("\\x4");
                    break;
                case '\x5':
                    writer.Write("\\x5");
                    break;
                case '\x6':
                    writer.Write("\\x6");
                    break;
                case '\a':
                    writer.Write("\\a");
                    break;
                case '\b':
                    writer.Write("\\b");
                    break;
                case '\t':
                    writer.Write("\\t");
                    break;
                case '\n':
                    writer.Write("\\n");
                    break;
                case '\v':
                    writer.Write("\\v");
                    break;
                case '\f':
                    writer.Write("\\f");
                    break;
                case '\r':
                    writer.Write("\\r");
                    break;
                case '\xE':
                    writer.Write("\\xE");
                    break;
                case '\xF':
                    writer.Write("\\xF");
                    break;
                case '\x10':
                    writer.Write("\\x10");
                    break;
                case '\x11':
                    writer.Write("\\x11");
                    break;
                case '\x12':
                    writer.Write("\\x12");
                    break;
                case '\x13':
                    writer.Write("\\x13");
                    break;
                case '\x14':
                    writer.Write("\\x14");
                    break;
                case '\x15':
                    writer.Write("\\x15");
                    break;
                case '\x16':
                    writer.Write("\\x16");
                    break;
                case '\x17':
                    writer.Write("\\x17");
                    break;
                case '\x18':
                    writer.Write("\\x18");
                    break;
                case '\x19':
                    writer.Write("\\x19");
                    break;
                case '\x1A':
                    writer.Write("\\x1A");
                    break;
                case '\x1B':
                    writer.Write("\\x1B");
                    break;
                case '\x1C':
                    writer.Write("\\x1C");
                    break;
                case '\x1D':
                    writer.Write("\\x1D");
                    break;
                case '\x1E':
                    writer.Write("\\x1E");
                    break;
                case '\x1F':
                    writer.Write("\\x1F");
                    break;
                case '\'':
                    writer.Write("\'");
                    break;
                case '\\':
                    writer.Write("\\\\");
                    break;
                default:
                    writer.Write(literal);
                    break;
            }
            writer.Write('\'');
        }

        public void Write(IMetadataConstantValue metadataConstantValue)
        {
            switch (metadataConstantValue.TypeCode)
            {
                case ConstantTypeCode.Boolean:
                    WriteStringLiteral(metadataConstantValue.GetValueAsBoolean() ? "true" : "false");
                    break;
                case ConstantTypeCode.Char:
                    WriteCharLiteral(metadataConstantValue.GetValueAsChar());
                    break;
                case ConstantTypeCode.SByte:
                    writer.Write(metadataConstantValue.GetValueAsSByte());
                    break;
                case ConstantTypeCode.Byte:
                    writer.Write(metadataConstantValue.GetValueAsByte());
                    break;
                case ConstantTypeCode.Int16:
                    writer.Write(metadataConstantValue.GetValueAsInt16());
                    break;
                case ConstantTypeCode.UInt16:
                    writer.Write(metadataConstantValue.GetValueAsUInt16());
                    break;
                case ConstantTypeCode.Int32:
                    writer.Write(metadataConstantValue.GetValueAsInt32());
                    break;
                case ConstantTypeCode.UInt32:
                    writer.Write(metadataConstantValue.GetValueAsUInt32());
                    break;
                case ConstantTypeCode.Int64:
                    writer.Write(metadataConstantValue.GetValueAsInt64());
                    break;
                case ConstantTypeCode.UInt64:
                    writer.Write(metadataConstantValue.GetValueAsUInt64());
                    break;
                case ConstantTypeCode.Single:
                    writer.Write(metadataConstantValue.GetValueAsSingle());
                    break;
                case ConstantTypeCode.Double:
                    writer.Write(metadataConstantValue.GetValueAsDouble());
                    break;
                case ConstantTypeCode.String:
                    WriteStringLiteral(metadataConstantValue.GetValueAsString());
                    break;
                case ConstantTypeCode.NullReference:
                    writer.Write("null");
                    break;
            }
        }

        public void Write(IMetadataField metadataField)
        {
            WriteVisibility(metadataField.Visibility);

            if (metadataField.IsLiteral)
                writer.Write("const ");
            else if (metadataField.IsStatic)
                writer.Write("static ");
            if (metadataField.IsInitOnly)
                writer.Write("readonly ");

            Write(metadataField.FieldType);
            writer.Write(' ');
            writer.Write(metadataField.Name);

            if (metadataField.DefaultValue != null)
            {
                writer.Write(" = ");
                Write(metadataField.DefaultValue);
            }

            writer.WriteLine(';');

        }

        private void WriteEnumFields(IMetadataEnum metadataEnum)
        {
            switch (((PrimitiveTypeReference)metadataEnum.UnderlyingType).Code)
            {
                case PrimitiveTypeCode.Int32:
                {
                    var enumFields = new List<(string name, int value)>(metadataEnum.Fields.Count);
                    foreach (var field in metadataEnum.Fields)
                    {
                        if (!field.IsLiteral) continue;
                        enumFields.Add((field.Name, field.DefaultValue.GetValueAsInt32()));
                    }

                    enumFields.Sort((x, y) =>
                    {
                        var byValue = x.value.CompareTo(y.value);
                        return byValue != 0 ? byValue : string.Compare(x.name, y.name, StringComparison.Ordinal);
                    });

                    for (var i = 0; i < enumFields.Count; i++)
                    {
                        if (i != 0) writer.WriteLine(',');
                        var (name, value) = enumFields[i];
                        writer.Write(name);
                        writer.Write(" = ");
                        writer.Write(value);
                    }
                    writer.WriteLine();
                    break;
                }
            }
        }
        private void WriteTypeMembers(IMetadataType metadataType)
        {
            writer.WriteLine('{');
            writer.Indent();

            if (metadataType is IMetadataEnum metadataEnum)
            {
                WriteEnumFields(metadataEnum);
            }
            else
            {
                foreach (var field in metadataType.Fields
                    .OrderByDescending(_ => _.IsLiteral)
                    .ThenByDescending(_ => _.IsStatic)
                    .ThenByDescending(_ => _.IsInitOnly)
                    .ThenBy(_ => _.Name))
                {
                    Write(field);
                }
            }

            foreach (var nestedType in metadataType.NestedTypes.OrderBy(_ => _.Name))
                Write(nestedType);

            writer.Unindent();
            writer.WriteLine('}');
        }

        private void WriteBaseTypeAndInterfaces(IMetadataType metadataType)
        {
            var didWriteColon = false;

            if (metadataType.BaseType != null &&
                !(metadataType is IMetadataClass && metadataType.BaseType is NamespaceTypeReference named && named.Namespace == "System" && named.Name == "Object"))
            {
                writer.Write(" : ");
                didWriteColon = true;
                Write(metadataType.BaseType);
            }

            foreach (var interfaceImplementation in metadataType.InterfaceImplementations)
            {
                if (!didWriteColon)
                {
                    writer.Write(" : ");
                    didWriteColon = true;
                }
                else
                {
                    writer.Write(", ");
                }

                Write(interfaceImplementation);
            }
        }

        public void Write(IMetadataType metadataType)
        {
            switch (metadataType)
            {
                case IMetadataClass metadataClass:
                    Write(metadataClass);
                    break;
                case IMetadataStruct metadataStruct:
                    Write(metadataStruct);
                    break;
                case IMetadataEnum metadataEnum:
                    Write(metadataEnum);
                    break;
                case IMetadataDelegate metadataDelegate:
                    Write(metadataDelegate);
                    break;
                case IMetadataInterface metadataInterface:
                    Write(metadataInterface);
                    break;
            }
        }

        public void Write(IMetadataClass metadataClass)
        {
            WriteVisibility(metadataClass.Visibility);

            if (metadataClass.IsStatic)
                writer.Write("static ");
            else if (metadataClass.IsAbstract)
                writer.Write("abstract ");
            else if (metadataClass.IsSealed)
                writer.Write("sealed ");

            writer.Write("class ");
            WriteNameAndGenericSignature(metadataClass);
            WriteBaseTypeAndInterfaces(metadataClass);
            writer.WriteLine();
            WriteTypeMembers(metadataClass);
        }

        public void Write(IMetadataStruct metadataStruct)
        {
            WriteVisibility(metadataStruct.Visibility);
            writer.Write("struct ");
            WriteNameAndGenericSignature(metadataStruct);
            writer.WriteLine();
            WriteTypeMembers(metadataStruct);
        }

        public void Write(IMetadataInterface metadataInterface)
        {
            WriteVisibility(metadataInterface.Visibility);
            writer.Write("interface ");
            WriteNameAndGenericSignature(metadataInterface);
            WriteBaseTypeAndInterfaces(metadataInterface);
            writer.WriteLine();
            WriteTypeMembers(metadataInterface);
        }

        public void Write(IMetadataEnum metadataEnum)
        {
            WriteVisibility(metadataEnum.Visibility);
            writer.Write("enum ");
            WriteNameAndGenericSignature(metadataEnum);
            writer.Write(" : ");
            Write(metadataEnum.UnderlyingType);
            writer.WriteLine();
            WriteTypeMembers(metadataEnum);
        }

        public void Write(IMetadataDelegate metadataDelegate)
        {
            WriteVisibility(metadataDelegate.Visibility);
            writer.Write("delegate ");
            Write(metadataDelegate.ReturnType);
            writer.Write(' ');
            WriteNameAndGenericSignature(metadataDelegate);

            writer.WriteLine(';');
        }

        private void Write(MetadataTypeReference typeReference)
        {
            Write(typeReference.Accept(SignatureTypeProvider.Instance));
        }

        private void Write(ImmutableNode<string> parts)
        {
            for (; parts != null; parts = parts.Next)
            {
                Write(parts.Prev);
                writer.Write(parts.Value);
            }
        }

        private sealed class ImmutableNode<T>
        {
            public ImmutableNode(ImmutableNode<T> prev, T value, ImmutableNode<T> next)
            {
                Value = value;
                Prev = prev;
                Next = next;
            }

            public ImmutableNode<T> Prev { get; }
            public T Value { get; }
            public ImmutableNode<T> Next { get; }
        }

        private sealed class SignatureTypeProvider : IMetadataTypeReferenceVisitor<ImmutableNode<string>>
        {
            public static readonly SignatureTypeProvider Instance = new SignatureTypeProvider();
            private SignatureTypeProvider() { }

            private static ImmutableNode<string> Literal(string value) => new ImmutableNode<string>(null, value, null);

            private static readonly ImmutableNode<string>[] PrimitiveTypesByCode =
            {
                null,
                Literal("void"),
                Literal("bool"),
                Literal("char"),
                Literal("sbyte"),
                Literal("byte"),
                Literal("short"),
                Literal("ushort"),
                Literal("int"),
                Literal("uint"),
                Literal("long"),
                Literal("ulong"),
                Literal("float"),
                Literal("double"),
                Literal("string"),
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Literal("System.TypedReference"),
                null,
                Literal("System.IntPtr"),
                Literal("System.UIntPtr"),
                null,
                null,
                Literal("object")
            };

            public ImmutableNode<string> Visit(PrimitiveTypeReference primitiveTypeReference)
            {
                return PrimitiveTypesByCode[(int)primitiveTypeReference.Code];
            }


            private static readonly List<string> ArraySuffixesByDimension = new List<string> { null, "[]" };

            public ImmutableNode<string> Visit(ArrayTypeReference array)
            {
                while (ArraySuffixesByDimension.Count < array.Dimensions)
                {
                    var buffer = new char[array.Dimensions + 1];
                    buffer[0] = '[';
                    for (var i = 1; i < buffer.Length - 2; i++)
                        buffer[i] = ',';
                    buffer[buffer.Length - 1] = ']';
                    ArraySuffixesByDimension.Add(new string(buffer));
                }

                return new ImmutableNode<string>(array.ElementType.Accept(this), ArraySuffixesByDimension[array.Dimensions], null);
            }

            private static string TrimGenericArity(string typeName)
            {
                var index = typeName.LastIndexOf('`');
                return index == -1 ? typeName : typeName.Substring(0, index);
            }

            public ImmutableNode<string> Visit(NamespaceTypeReference namespaceTypeReference)
            {
                var nameNode = new ImmutableNode<string>(null, TrimGenericArity(namespaceTypeReference.Name), null);
                return string.IsNullOrEmpty(namespaceTypeReference.Namespace) ? nameNode :
                    new ImmutableNode<string>(new ImmutableNode<string>(null, namespaceTypeReference.Namespace, null), ".", nameNode);
            }

            public ImmutableNode<string> Accept(GenericParameterTypeReference genericParameterTypeReference)
            {
                return new ImmutableNode<string>(null, genericParameterTypeReference.Name, null);
            }

            public ImmutableNode<string> Visit(GenericInstantiationTypeReference genericInstantiationTypeReference)
            {
                var args = genericInstantiationTypeReference.GenericTypeArguments;
                var current = new ImmutableNode<string>(args[args.Count - 1].Accept(this), ">", null);

                for (var i = args.Count - 2; i >= 0; i--)
                {
                    current = new ImmutableNode<string>(args[i].Accept(this), ", ", current);
                }

                return new ImmutableNode<string>(genericInstantiationTypeReference.TypeDefinition.Accept(this), "<", current);
            }

            public ImmutableNode<string> Visit(NestedTypeReference nestedTypeReference)
            {
                return new ImmutableNode<string>(nestedTypeReference.DeclaringType.Accept(this), ".", new ImmutableNode<string>(null, nestedTypeReference.Name, null));
            }
        }
    }
}
