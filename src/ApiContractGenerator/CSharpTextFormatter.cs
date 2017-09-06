using System.Collections.Generic;
using System.IO;
using System.Xml;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    public sealed class CSharpTextFormatter : IMetadataVisitor
    {
        private readonly IndentedTextWriter writer;

        public CSharpTextFormatter(TextWriter writer)
        {
            this.writer = new IndentedTextWriter(writer);
        }

        public void Visit(IMetadataNamespace metadataNamespace)
        {
            writer.Write("namespace ");
            writer.WriteLine(metadataNamespace.Name);
            writer.WriteLine('{');
            writer.Indent();

            metadataNamespace.Accept(this);

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

        public void Visit(IMetadataClass metadataClass)
        {
            WriteVisibility(metadataClass.Visibility);

            if (metadataClass.IsStatic)
                writer.Write("static ");
            else if (metadataClass.IsAbstract)
                writer.Write("abstract ");
            else if (metadataClass.IsSealed)
                writer.Write("sealed ");

            writer.Write("class ");
            writer.WriteLine(metadataClass.Name);
            writer.WriteLine('{');
            writer.Indent();

            metadataClass.Accept(this);

            writer.Unindent();
            writer.WriteLine('}');
        }

        public void Visit(IMetadataStruct metadataStruct)
        {
            WriteVisibility(metadataStruct.Visibility);
            writer.Write("struct ");
            writer.WriteLine(metadataStruct.Name);
            writer.WriteLine('{');
            writer.Indent();

            metadataStruct.Accept(this);

            writer.Unindent();
            writer.WriteLine('}');
        }

        public void Visit(IMetadataInterface metadataInterface)
        {
            WriteVisibility(metadataInterface.Visibility);
            writer.Write("interface ");
            writer.WriteLine(metadataInterface.Name);
            writer.WriteLine('{');
            writer.Indent();

            metadataInterface.Accept(this);

            writer.Unindent();
            writer.WriteLine('}');
        }

        public void Visit(IMetadataEnum metadataEnum)
        {
            WriteVisibility(metadataEnum.Visibility);
            writer.Write("enum ");
            writer.Write(metadataEnum.Name);
            writer.Write(" : ");
            Write(metadataEnum.UnderlyingType);
            writer.WriteLine();
            writer.WriteLine('{');
            writer.Indent();

            metadataEnum.Accept(this);

            writer.Unindent();
            writer.WriteLine('}');

        }

        public void Visit(IMetadataDelegate metadataDelegate)
        {
            WriteVisibility(metadataDelegate.Visibility);
            writer.Write("delegate ");
            Write(metadataDelegate.ReturnType);
            writer.Write(' ');
            writer.Write(metadataDelegate.Name);

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
