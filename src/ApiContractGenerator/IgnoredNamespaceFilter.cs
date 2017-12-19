using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed partial class IgnoredNamespaceFilter : IMetadataSource
    {
        private readonly IMetadataSource source;
        private readonly IEnumerable<string> ignoredNamespaces;
        private readonly IMetadataReferenceResolver metadataReferenceResolver;

        public IgnoredNamespaceFilter(IMetadataSource source, IEnumerable<string> ignoredNamespaces, IMetadataReferenceResolver metadataReferenceResolver)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            this.ignoredNamespaces = ignoredNamespaces ?? throw new ArgumentNullException(nameof(ignoredNamespaces));
            this.metadataReferenceResolver = metadataReferenceResolver ?? throw new ArgumentNullException(nameof(metadataReferenceResolver));
        }

        private IReadOnlyList<IMetadataNamespace> namespaces;
        public IReadOnlyList<IMetadataNamespace> Namespaces
        {
            get => namespaces ?? (namespaces =
                ReferencedIgnoredMetadataVisitor.CalculateNonignoredTransitiveClosure(source, ignoredNamespaces, metadataReferenceResolver));
        }

        public string AssemblyName => source.AssemblyName;

        public byte[] PublicKey => source.PublicKey;

        private sealed class ReferencedIgnoredMetadataVisitor
        {
            private readonly IReadOnlyDictionary<string, PartiallyIgnoredNamespaceBuilder> ignoredNamespaces;
            private readonly IMetadataReferenceResolver metadataReferenceResolver;

            private ReferencedIgnoredMetadataVisitor(IReadOnlyDictionary<string, PartiallyIgnoredNamespaceBuilder> ignoredNamespaces, IMetadataReferenceResolver metadataReferenceResolver)
            {
                this.ignoredNamespaces = ignoredNamespaces;
                this.metadataReferenceResolver = metadataReferenceResolver;
            }

            public static IReadOnlyList<IMetadataNamespace> CalculateNonignoredTransitiveClosure(IMetadataSource source, IEnumerable<string> ignoredNamespaces, IMetadataReferenceResolver metadataReferenceResolver)
            {
                var regexBuilder = (StringBuilder)null;
                foreach (var ignoredNamespace in ignoredNamespaces)
                {
                    if (regexBuilder == null)
                        regexBuilder = new StringBuilder(@"\A(?:");
                    else
                        regexBuilder.Append('|');

                    regexBuilder.Append(Regex.Escape(ignoredNamespace));
                }

                if (regexBuilder == null) return source.Namespaces;

                var namespaceIgnoreRegex = new Regex(regexBuilder.Append(@")(\Z|\.)").ToString(), RegexOptions.IgnoreCase);

                var ignoredNamespaceLookup = new Dictionary<string, PartiallyIgnoredNamespaceBuilder>();

                var includedNamespaces = new List<IMetadataNamespace>(source.Namespaces.Count);
                foreach (var ns in source.Namespaces)
                {
                    if (namespaceIgnoreRegex.IsMatch(ns.Name))
                        ignoredNamespaceLookup.Add(ns.Name, new PartiallyIgnoredNamespaceBuilder(ns));
                    else
                        includedNamespaces.Add(ns);
                }

                if (ignoredNamespaceLookup.Count != 0)
                {
                    var visitor = new ReferencedIgnoredMetadataVisitor(ignoredNamespaceLookup, metadataReferenceResolver);

                    foreach (var ns in includedNamespaces)
                    {
                        var prefix = string.IsNullOrEmpty(ns.Name) ? null : TextNode.Root(ns.Name) + ".";
                        foreach (var type in ns.Types)
                            visitor.VisitNonignoredType(prefix + type.Name, type);
                    }

                    foreach (var ns in visitor.ignoredNamespaces.Values)
                        if (ns.Types.Count != 0)
                            includedNamespaces.Add(ns);
                }

                return includedNamespaces;
            }

            private bool ParameterIsNamedType(MetadataTypeReference type)
            {
                switch (type)
                {
                    case TopLevelTypeReference _:
                    case NestedTypeReference _:
                        return true;
                    case GenericInstantiationTypeReference instantiation:
                        return ParameterIsNamedType(instantiation.TypeDefinition);
                    default:
                        return false;
                }
            }

            private void VisitNonignoredType(TextNode typeName, IMetadataType type)
            {
                // Types should not be brought in via interface implementations;
                // if interface implementations are the only thing bringing it in, that requires naming the ignored type.

                // Generic type and method constraints don't need to be considered;
                // they'll be picked up in an output position already by VisitTypeReference.

                foreach (var field in type.Fields)
                    VisitTypeReference((typeName + ".") + field.Name, field.FieldType);

                var @delegate = type as IMetadataDelegate;
                if (@delegate != null)
                {
                    // Delegate parameters don't require naming the ignored type in order to receive values from them.
                    // when passing the delegate as a parameter.
                    foreach (var parameter in @delegate.Parameters)
                        VisitTypeReference((typeName + " parameter ") + parameter.Name, parameter.ParameterType);

                    // Delegate returns don't require naming the ignored type in order to receive values from them
                    // when invoking them.
                    VisitTypeReference(typeName + " return type", @delegate.ReturnType);
                }

                foreach (var method in type.Methods) // Covers constructors, operators, properties and events
                {
                    if (@delegate != null && (
                        method.Name == "Invoke"
                        || method.Name == "BeginInvoke"
                        || method.Name == "EndInvoke"))
                    {
                        continue;
                    }

                    var methodName = (typeName + ".") + method.Name;

                    foreach (var parameter in method.Parameters)
                    {
                        if (parameter.IsOut // Out var
                            || (ParameterIsNamedType(parameter.ParameterType)
                                && (!metadataReferenceResolver.TryGetIsDelegateType(parameter.ParameterType, out var isDelegate) // Better safe than sorry
                                    || isDelegate))) // Delegate parameter types can be syntactically inferred, so ignored types could be used without naming them
                        {
                            // For delegates we could be even smarter and skip if it has no byval parameters. Future enhancement.
                            VisitTypeReference((methodName + " parameter ") + parameter.Name, parameter.ParameterType);
                        }

                        // Otherwise, types should not be brought in via method parameters;
                        // if method parameters are the only thing bringing it in, that requires naming the ignored type.
                    }

                    VisitTypeReference(methodName + " return type", method.ReturnType);
                }

                if (type.BaseType != null) VisitTypeReference(typeName + " base type", type.BaseType);

                foreach (var nestedType in type.NestedTypes)
                    VisitNonignoredType((typeName + ".") + nestedType.Name, nestedType);
            }

            private void VisitTypeReference(TextNode referencePath, MetadataTypeReference type)
            {
                for (;;)
                {
                    switch (type)
                    {
                        case TopLevelTypeReference topLevel:
                            VisitTypeName(referencePath, topLevel.Namespace, topLevel.Name, Array.Empty<string>());
                            return;

                        case NestedTypeReference nested:
                            for (var nestedNames = new List<string>(); ;)
                            {
                                nestedNames.Add(nested.Name);
                                if (nested.DeclaringType is NestedTypeReference next)
                                {
                                    nested = next;
                                }
                                else
                                {
                                    var topLevel = (TopLevelTypeReference)nested.DeclaringType;
                                    nestedNames.Reverse();
                                    VisitTypeName(referencePath, topLevel.Namespace, topLevel.Name, nestedNames);
                                    return;
                                }
                            }

                        case GenericInstantiationTypeReference genericInstantiation:
                            foreach (var argument in genericInstantiation.GenericTypeArguments)
                                VisitTypeReference(referencePath, argument);
                            type = genericInstantiation.TypeDefinition;
                            continue;

                        case ArrayTypeReference array:
                            type = array.ElementType;
                            continue;

                        case ByRefTypeReference byref:
                            type = byref.ElementType;
                            continue;

                        case PointerTypeReference pointer:
                            type = pointer.ElementType;
                            continue;

                        case GenericParameterTypeReference genericParameter:
                            foreach (var constraint in genericParameter.TypeParameter.TypeConstraints)
                                VisitTypeReference(referencePath + " generic constraint", constraint);
                            return;

                        case PrimitiveTypeReference _:
                            return;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            private void VisitTypeName(TextNode referencePath, string @namespace, string topLevelName, IReadOnlyList<string> nestedNames)
            {
                if (ignoredNamespaces.TryGetValue(@namespace, out var builder)
                    && builder.TryUnignore(referencePath, topLevelName, nestedNames, out var unignoredType))
                {
                    var typeName = !string.IsNullOrEmpty(@namespace)
                        ? (TextNode.Root(@namespace) + ".") + topLevelName
                        : TextNode.Root(topLevelName);

                    foreach (var name in nestedNames)
                        typeName = (typeName + ".") + name;

                    VisitNonignoredType(typeName, unignoredType);
                }
            }

            private sealed class PartiallyIgnoredNamespaceBuilder : IMetadataNamespace
            {
                private readonly IMetadataNamespace originalNamespace;
                private readonly List<PartiallyIgnoredTypeBuilder> typeBuilders = new List<PartiallyIgnoredTypeBuilder>();

                public PartiallyIgnoredNamespaceBuilder(IMetadataNamespace originalNamespace)
                {
                    this.originalNamespace = originalNamespace;
                }

                public string Name => originalNamespace.Name;

                public IReadOnlyList<IMetadataType> Types => typeBuilders;

                /// <summary>
                /// Returns <see langword="false"/> if the type is not found or if it has already been unignored.
                /// </summary>
                public bool TryUnignore(TextNode referencePath, string topLevelName, IReadOnlyList<string> nestedNames, out IMetadataType unignoredType)
                {
                    var current = originalNamespace.Types.FirstOrDefault(_ => _.Name == topLevelName);
                    if (current != null)
                    {
                        if (nestedNames.Count == 0)
                        {
                            var builder = typeBuilders.FirstOrDefault(_ => _.Name == topLevelName);
                            if (builder == null)
                                typeBuilders.Add(builder = PartiallyIgnoredTypeBuilder.Create(current));

                            if (!builder.TryUnignore(referencePath))
                            {
                                unignoredType = null;
                                return false;
                            }

                            unignoredType = current;
                            return true;
                        }
                        else
                        {
                            var parentTypes = new List<IMetadataType>(nestedNames.Count);
                            foreach (var name in nestedNames)
                            {
                                parentTypes.Add(current);
                                current = current.NestedTypes.FirstOrDefault(_ => _.Name == name);
                                if (current == null)
                                {
                                    unignoredType = null;
                                    return false;
                                }
                            }

                            var builder = typeBuilders.FirstOrDefault(_ => _.Name == topLevelName);
                            if (builder == null)
                                typeBuilders.Add(builder = PartiallyIgnoredTypeBuilder.Create(parentTypes[0]));

                            if (builder.TryUnignoreNested(referencePath, parentTypes, 0, current))
                            {
                                unignoredType = current;
                                return true;
                            }
                        }
                    }

                    unignoredType = null;
                    return false;
                }
            }

            private abstract class PartiallyIgnoredTypeBuilder : IMetadataType
            {
                private readonly IMetadataType original;
                private readonly List<PartiallyIgnoredTypeBuilder> nestedTypeBuilders = new List<PartiallyIgnoredTypeBuilder>();
                private readonly List<TextNode> referencePaths = new List<TextNode>();
                private bool asDeclaringNameOnly = true;

                private PartiallyIgnoredTypeBuilder(IMetadataType original)
                {
                    this.original = original;
                }

                public static PartiallyIgnoredTypeBuilder Create(IMetadataType original)
                {
                    switch (original)
                    {
                        case IMetadataClass @class:
                            return new PartiallyIgnoredClassBuilder(@class);
                        case IMetadataStruct @struct:
                            return new PartiallyIgnoredStructBuilder(@struct);
                        case IMetadataInterface @interface:
                            return new PartiallyIgnoredInterfaceBuilder(@interface);
                        case IMetadataEnum @enum:
                            return new PartiallyIgnoredEnumBuilder(@enum);
                        case IMetadataDelegate @delegate:
                            return new PartiallyIgnoredDelegateBuilder(@delegate);
                        default:
                            throw new NotImplementedException();
                    }
                }

                private sealed class PartiallyIgnoredClassBuilder : PartiallyIgnoredTypeBuilder, IMetadataClass
                {
                    public PartiallyIgnoredClassBuilder(IMetadataClass original) : base(original)
                    {
                    }

                    public bool IsStatic => ((IMetadataClass)original).IsStatic;
                    public bool IsAbstract => ((IMetadataClass)original).IsAbstract;
                    public bool IsSealed => ((IMetadataClass)original).IsSealed;
                }

                private sealed class PartiallyIgnoredStructBuilder : PartiallyIgnoredTypeBuilder, IMetadataStruct
                {
                    public PartiallyIgnoredStructBuilder(IMetadataStruct original) : base(original)
                    {
                    }
                }

                private sealed class PartiallyIgnoredInterfaceBuilder : PartiallyIgnoredTypeBuilder, IMetadataInterface
                {
                    public PartiallyIgnoredInterfaceBuilder(IMetadataInterface original) : base(original)
                    {
                    }
                }

                private sealed class PartiallyIgnoredEnumBuilder : PartiallyIgnoredTypeBuilder, IMetadataEnum
                {
                    public PartiallyIgnoredEnumBuilder(IMetadataEnum original) : base(original)
                    {
                    }

                    public MetadataTypeReference UnderlyingType => ((IMetadataEnum)original).UnderlyingType;
                }

                private sealed class PartiallyIgnoredDelegateBuilder : PartiallyIgnoredTypeBuilder, IMetadataDelegate
                {
                    public PartiallyIgnoredDelegateBuilder(IMetadataDelegate original) : base(original)
                    {
                    }

                    public MetadataTypeReference ReturnType => ((IMetadataDelegate)original).ReturnType;
                    public IReadOnlyList<IMetadataAttribute> ReturnValueAttributes => ((IMetadataDelegate)original).ReturnValueAttributes;
                    public IReadOnlyList<IMetadataParameter> Parameters => ((IMetadataDelegate)original).Parameters;
                }



                public string Comment
                {
                    get
                    {
                        if (asDeclaringNameOnly) return null;

                        var comment = new StringBuilder();
                        if (!string.IsNullOrWhiteSpace(original.Comment)) comment.AppendLine(original.Comment);

                        comment.Append("Warning; type cannot be ignored because it is referenced by:");
                        foreach (var path in referencePaths)
                        {
                            comment.AppendLine();
                            comment.Append(" - ");
                            foreach (var segment in TextNode.ToArray(path))
                                comment.Append(segment);
                        }

                        return comment.ToString();
                    }
                }

                public bool TryUnignore(TextNode referencePath)
                {
                    referencePaths.Add(referencePath ?? throw new ArgumentNullException(nameof(referencePath)));

                    if (!asDeclaringNameOnly) return false;
                    asDeclaringNameOnly = false;
                    return true;
                }

                public bool TryUnignoreNested(TextNode referencePath, IReadOnlyList<IMetadataType> parentTypes, int parentIndex, IMetadataType typeToUnignore)
                {
                    var nextParentIndex = parentIndex + 1;
                    if (nextParentIndex != parentTypes.Count)
                    {
                        var nextParent = parentTypes[nextParentIndex];

                        var builder = nestedTypeBuilders.FirstOrDefault(_ => _.Name == nextParent.Name);
                        if (builder == null)
                            nestedTypeBuilders.Add(builder = Create(nextParent));

                        return builder.TryUnignoreNested(referencePath, parentTypes, nextParentIndex, typeToUnignore);
                    }
                    else
                    {
                        var builder = nestedTypeBuilders.FirstOrDefault(_ => _.Name == typeToUnignore.Name);
                        if (builder == null)
                            nestedTypeBuilders.Add(builder = Create(typeToUnignore));

                        return builder.TryUnignore(referencePath);
                    }
                }

                public IReadOnlyList<IMetadataType> NestedTypes => nestedTypeBuilders;

                public string Name => original.Name;

                public MetadataVisibility Visibility => original.Visibility;

                public IReadOnlyList<IMetadataGenericTypeParameter> GenericTypeParameters => original.GenericTypeParameters;

                public IReadOnlyList<IMetadataAttribute> Attributes
                {
                    get => asDeclaringNameOnly
                        ? Array.Empty<IMetadataAttribute>()
                        : original.Attributes;
                }

                public MetadataTypeReference BaseType
                {
                    get => asDeclaringNameOnly
                        ? null
                        : original.BaseType;
                }

                public IReadOnlyList<MetadataTypeReference> InterfaceImplementations
                {
                    get => asDeclaringNameOnly
                        ? Array.Empty<MetadataTypeReference>()
                        : original.InterfaceImplementations;
                }

                private bool CouldBeInheritedByNonignoredType => original is IMetadataClass @class && !@class.IsStatic && !@class.IsSealed;

                public IReadOnlyList<IMetadataField> Fields
                {
                    get => asDeclaringNameOnly ? Array.Empty<IMetadataField>() :
                        CouldBeInheritedByNonignoredType ? original.Fields :
                        original.Fields.Where(_ => _.IsStatic == false).ToList();
                }

                public IReadOnlyList<IMetadataProperty> Properties
                {
                    get => asDeclaringNameOnly ? Array.Empty<IMetadataProperty>() :
                        CouldBeInheritedByNonignoredType ? original.Properties :
                        original.Properties.Where(_ => _.GetAccessor?.IsStatic == false || _.SetAccessor?.IsStatic == false).ToList();
                }

                public IReadOnlyList<IMetadataEvent> Events
                {
                    get => asDeclaringNameOnly ? Array.Empty<IMetadataEvent>() :
                        CouldBeInheritedByNonignoredType ? original.Events :
                        original.Events.Where(_ => _.AddAccessor?.IsStatic == false || _.RemoveAccessor?.IsStatic == false || _.RaiseAccessor?.IsStatic == false).ToList();
                }

                public IReadOnlyList<IMetadataMethod> Methods
                {
                    get => asDeclaringNameOnly ? (IReadOnlyList<IMetadataMethod>)Array.Empty<IMetadataMethod>() :
                        CouldBeInheritedByNonignoredType ? original.Methods.Where(_ => _.Name != ".ctor").ToList() :
                        original.Methods.Where(_ => _.IsStatic == false && _.Name != ".ctor").ToList();
                }
            }
        }
    }
}
