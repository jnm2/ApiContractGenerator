using System;
using System.Collections.Generic;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    public sealed partial class CSharpTextFormatter
    {
        private sealed class SignatureTypeProvider : IMetadataTypeReferenceVisitor<ImmutableNode<string>>
        {
            private readonly string currentNamespace;

            public SignatureTypeProvider(string currentNamespace)
            {
                this.currentNamespace = currentNamespace;
            }

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

            private static readonly ImmutableNode<string> GenericParameterListEnd = new ImmutableNode<string>(null, ">", null);

            private static ImmutableNode<string> BuildNameWithArity(string rawName)
            {
                var (name, arity) = ParseGenericArity(rawName);

                var arityBuilder = (ImmutableNode<string>)null;
                if (arity != 0)
                {
                    arityBuilder = GenericParameterListEnd;
                    for (var i = 1; i < arity; i++)
                        arityBuilder = new ImmutableNode<string>(null, ",", arityBuilder);
                    arityBuilder = new ImmutableNode<string>(null, "<", arityBuilder);
                }

                return new ImmutableNode<string>(null, name, arityBuilder);
            }

            public ImmutableNode<string> Visit(TopLevelTypeReference topLevelTypeReference)
            {
                return AddNamespace(topLevelTypeReference, BuildNameWithArity(topLevelTypeReference.Name));
            }

            private ImmutableNode<string> AddNamespace(TopLevelTypeReference topLevelTypeReference, ImmutableNode<string> name)
            {
                return currentNamespace == topLevelTypeReference.Namespace || string.IsNullOrEmpty(topLevelTypeReference.Namespace) ? name :
                    new ImmutableNode<string>(new ImmutableNode<string>(null, topLevelTypeReference.Namespace, null), ".", name);
            }

            public ImmutableNode<string> Accept(GenericParameterTypeReference genericParameterTypeReference)
            {
                return new ImmutableNode<string>(null, genericParameterTypeReference.TypeParameter.Name, null);
            }

            public ImmutableNode<string> Visit(GenericInstantiationTypeReference genericInstantiationTypeReference)
            {
                var args = genericInstantiationTypeReference.GenericTypeArguments;

                if (args.Count == 1
                    && genericInstantiationTypeReference.TypeDefinition is TopLevelTypeReference topLevel
                    && topLevel.Name == "Nullable`1"
                    && topLevel.Namespace == "System")
                {
                    return new ImmutableNode<string>(args[0].Accept(this), "?", null);
                }
                if (IsValueTuple(genericInstantiationTypeReference, minArgs: 2) && TryUseTupleSyntax(args, out var tupleSyntax))
                {
                    return tupleSyntax;
                }

                var builder = (ImmutableNode<string>)null;
                var argumentsLeft = genericInstantiationTypeReference.GenericTypeArguments.Count;

                void BuildNext(string typeName)
                {
                    var (name, arity) = ParseGenericArity(typeName);
                    if (arity != 0)
                    {
                        if (argumentsLeft < arity) throw new InvalidOperationException("Number of generic arguments provided does not match combined type arity.");

                        builder = new ImmutableNode<string>(null, ">", builder);
                        for (var i = 0; i < arity; i++)
                        {
                            argumentsLeft--;
                            builder = new ImmutableNode<string>(
                                genericInstantiationTypeReference.GenericTypeArguments[argumentsLeft].Accept(this),
                                i == 0 ? null : ", ",
                                builder);
                        }
                        builder = new ImmutableNode<string>(null, "<", builder);
                    }
                    builder = new ImmutableNode<string>(null, name, builder);
                }


                var currentType = genericInstantiationTypeReference.TypeDefinition;
                for (; currentType is NestedTypeReference nested; currentType = nested.DeclaringType)
                {
                    BuildNext(nested.Name);
                    builder = new ImmutableNode<string>(null, ".", builder);
                }

                topLevel = currentType as TopLevelTypeReference ??
                    throw new InvalidOperationException("Nested types must be declared by either a top-level type or another nested type.");

                BuildNext(topLevel.Name);

                if (argumentsLeft != 0) throw new InvalidOperationException("Number of generic arguments provided does not match combined type arity.");
                return AddNamespace(topLevel, builder);
            }

            private static readonly string[] ValueTupleNamesByArity =
            {
                null, "ValueTuple`1", "ValueTuple`2", "ValueTuple`3", "ValueTuple`4", "ValueTuple`5", "ValueTuple`6", "ValueTuple`7", "ValueTuple`8"
            };

            private static bool IsValueTuple(GenericInstantiationTypeReference genericInstantiationTypeReference, int minArgs)
            {
                var args = genericInstantiationTypeReference.GenericTypeArguments;
                return args.Count >= minArgs
                    && args.Count <= 8
                    && genericInstantiationTypeReference.TypeDefinition is TopLevelTypeReference topLevel
                    && topLevel.Name == ValueTupleNamesByArity[args.Count]
                    && topLevel.Namespace == "System";
            }

            private bool TryUseTupleSyntax(IReadOnlyList<MetadataTypeReference> args, out ImmutableNode<string> tupleSyntax)
            {
                var tupleElements = new List<MetadataTypeReference>(args.Count);

                while (args.Count == 8)
                {
                    if (!(args[7] is GenericInstantiationTypeReference genericRest && IsValueTuple(genericRest, minArgs: 1)))
                    {
                        tupleSyntax = null;
                        return false;
                    }

                    for (var i = 0; i < 7; i++)
                        tupleElements.Add(args[i]);

                    args = genericRest.GenericTypeArguments;
                }

                for (var i = 0; i < args.Count; i++)
                    tupleElements.Add(args[i]);

                var current = new ImmutableNode<string>(tupleElements[tupleElements.Count - 1].Accept(this), ")", null);

                for (var i = tupleElements.Count - 2; i >= 0; i--)
                    current = new ImmutableNode<string>(tupleElements[i].Accept(this), ", ", current);

                tupleSyntax = new ImmutableNode<string>(null, "(", current);
                return true;
            }

            public ImmutableNode<string> Visit(ByRefTypeReference byRefTypeReference)
            {
                return new ImmutableNode<string>(null, "ref ", byRefTypeReference.ElementType.Accept(this));
            }

            public ImmutableNode<string> Visit(NestedTypeReference nestedTypeReference)
            {
                return new ImmutableNode<string>(nestedTypeReference.DeclaringType.Accept(this), ".", BuildNameWithArity(nestedTypeReference.Name));
            }

            public ImmutableNode<string> Visit(PointerTypeReference pointerTypeReference)
            {
                return new ImmutableNode<string>(pointerTypeReference.ElementType.Accept(this), "*", null);
            }
        }
    }
}
