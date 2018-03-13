using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.MetadataReferenceResolvers
{
    public sealed partial class MetadataReaderReferenceResolver
    {
        [DebuggerDisplay("{ToString(),nq}")]
        private struct NameSpec : IEquatable<NameSpec>
        {
            public string Namespace { get; }
            public string TopLevelName { get; }
            public string[] NestedNames { get; }

            public NameSpec(string @namespace, string topLevelName, string[] nestedNames)
            {
                Namespace = @namespace;
                TopLevelName = topLevelName;
                NestedNames = nestedNames != null && nestedNames.Length != 0 ? nestedNames : null;
            }

            public static (MetadataAssemblyReference assemblyReference, NameSpec typeName) FromMetadataTypeReference(MetadataTypeReference typeReference)
            {
                if (typeReference is GenericInstantiationTypeReference genericInstantiation)
                    typeReference = genericInstantiation.TypeDefinition;

                var nestedNames = (List<string>)null;

                for (; typeReference is NestedTypeReference nested; typeReference = nested.DeclaringType)
                {
                    if (nestedNames == null) nestedNames = new List<string>();
                    nestedNames.Add(nested.Name);
                }
                nestedNames?.Reverse();

                if (!(typeReference is TopLevelTypeReference topLevel))
                    throw new ArgumentException($"Type reference must either be a {nameof(TopLevelTypeReference)} or a {nameof(NestedTypeReference)}.", nameof(typeReference));

                return (
                    topLevel.Assembly,
                    new NameSpec(
                        topLevel.Namespace,
                        topLevel.Name,
                        nestedNames?.ToArray()));
            }

            public bool Equals(NameSpec other)
            {
                if (!(string.Equals(Namespace, other.Namespace)
                      && string.Equals(TopLevelName, other.TopLevelName)))
                {
                    return false;
                }

                if (NestedNames == null || other.NestedNames == null) return NestedNames == other.NestedNames;
                if (NestedNames.Length != other.NestedNames.Length) return false;

                for (var i = 0; i < NestedNames.Length; i++)
                    if (NestedNames[i] != other.NestedNames[i])
                        return false;

                return true;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is NameSpec && Equals((NameSpec)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = Namespace != null ? Namespace.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (TopLevelName != null ? TopLevelName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (NestedNames != null ? NestedNames.Length : 0);
                    return hashCode;
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(Namespace))
                    sb.Append(Namespace).Append('.');

                sb.Append(TopLevelName);

                if (NestedNames != null)
                    foreach (var nestedName in NestedNames)
                        sb.Append('+').Append(nestedName);

                return sb.ToString();
            }

            public static bool operator ==(NameSpec left, NameSpec right) => left.Equals(right);
            public static bool operator !=(NameSpec left, NameSpec right) => !left.Equals(right);
        }
    }
}
