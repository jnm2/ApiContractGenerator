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
            private readonly string @namespace;
            private readonly string topLevelName;
            private readonly string[] nestedNames;

            public NameSpec(string @namespace, string topLevelName, string[] nestedNames)
            {
                this.@namespace = @namespace;
                this.topLevelName = topLevelName;
                this.nestedNames = nestedNames != null && nestedNames.Length != 0 ? nestedNames : null;
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
                if (!(string.Equals(@namespace, other.@namespace)
                      && string.Equals(topLevelName, other.topLevelName)))
                {
                    return false;
                }

                if (nestedNames == null || other.nestedNames == null) return nestedNames == other.nestedNames;
                if (nestedNames.Length != other.nestedNames.Length) return false;

                for (var i = 0; i < nestedNames.Length; i++)
                    if (nestedNames[i] != other.nestedNames[i])
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
                    var hashCode = @namespace != null ? @namespace.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (topLevelName != null ? topLevelName.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (nestedNames != null ? nestedNames.Length : 0);
                    return hashCode;
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(@namespace))
                    sb.Append(@namespace).Append('.');

                sb.Append(topLevelName);

                if (nestedNames != null)
                    foreach (var nestedName in nestedNames)
                        sb.Append('+').Append(nestedName);

                return sb.ToString();
            }

            public static bool operator ==(NameSpec left, NameSpec right) => left.Equals(right);
            public static bool operator !=(NameSpec left, NameSpec right) => !left.Equals(right);
        }
    }
}
