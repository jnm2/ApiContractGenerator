using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private void WriteTypeMembers(IMetadataType metadataType)
        {
            writer.WriteLine('{');
            writer.Indent();

            foreach (var nestedType in metadataType.NestedTypes.OrderBy(_ => _.Name))
                Write(nestedType);

            writer.Unindent();
            writer.WriteLine('}');
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


            public ImmutableNode<string> Visit(NamedTypeReference namedTypeReference)
            {
                return new ImmutableNode<string>(
                    new ImmutableNode<string>(null, namedTypeReference.Namespace, null),
                    ".",
                    new ImmutableNode<string>(null, namedTypeReference.Name, null));
            }

            public ImmutableNode<string> Accept(GenericParameterTypeReference genericParameterTypeReference)
            {
                return new ImmutableNode<string>(null, genericParameterTypeReference.Name, null);
            }
        }
    }
}
