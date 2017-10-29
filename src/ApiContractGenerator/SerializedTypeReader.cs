using System;
using System.Collections.Generic;
using System.Reflection;
using ApiContractGenerator.Internal;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    public static class SerializedTypeReader
    {
        public static MetadataTypeReference Deserialize(string serializedName)
        {
            var remaining = (StringSpan)serializedName;
            var (typeName, genericParameters) = ReadTypeParts(ref remaining);
            var assemblyName = TryReadAssemblyName(remaining);
            var namedType = GetNamedType(typeName, assemblyName);
            return genericParameters == null ? namedType : new GenericInstantiationTypeReference(namedType, genericParameters);
        }

        private static (StringSpan typeName, IReadOnlyList<MetadataTypeReference> genericParameters)
            ReadTypeParts(ref StringSpan remaining)
        {
            var firstComma = remaining.IndexOf(',');
            var beforeComma = firstComma != -1 ? remaining.Slice(0, firstComma) : remaining;
            var genericParametersStartingBracket = beforeComma.IndexOf('[');
            if (genericParametersStartingBracket == -1)
            {
                remaining = firstComma != -1 ? remaining.Slice(firstComma) : default;
                return (beforeComma, null);
            }

            var typeName = remaining.Slice(0, genericParametersStartingBracket);
            remaining = remaining.Slice(genericParametersStartingBracket + 1);

            var genericParameters = new List<MetadataTypeReference>();
            while (remaining.Length != 0)
            {
                if (TryRead(ref remaining, ' ')) continue;

                genericParameters.Add(ReadGenericParameter(ref remaining));
                while (TryRead(ref remaining, ' ')) { }
                switch (TryRead(ref remaining))
                {
                    case ']':
                        return (typeName, genericParameters);
                    case ',':
                        break;
                    default:
                        throw new FormatException("Expected ' ', ',' or ']'");
                }
            }

            throw new FormatException("Expected generic parameter");
        }

        private static MetadataTypeReference ReadGenericParameter(ref StringSpan remaining)
        {
            var isBracketed = TryRead(ref remaining, '[');
            var (typeName, genericParameters) = ReadTypeParts(ref remaining);

            var assemblyName = default(StringSpan);
            if (isBracketed)
            {
                var endingBracket = remaining.IndexOf(']');
                if (endingBracket == -1) throw new FormatException("Expected ']'");
                assemblyName = TryReadAssemblyName(remaining.Slice(0, endingBracket));
                remaining = remaining.Slice(endingBracket + 1);
            }

            var namedType = GetNamedType(typeName, assemblyName);
            return genericParameters == null ? namedType : new GenericInstantiationTypeReference(namedType, genericParameters);
        }

        /// <param name="span">Entire assembly name span if any, including comma.</param>
        private static StringSpan TryReadAssemblyName(StringSpan span)
        {
            while (TryRead(ref span, ' ')) { }
            if (!TryRead(ref span, ',')) return default;

            while (TryRead(ref span, ' ')) { }
            if (span.Length == 0) throw new FormatException("Expected assembly name");

            for (var i = span.Length - 1; i >= 1; i++)
                if (span[i] != ' ')
                    return span.Slice(0, i + 1);

            return span;
        }

        private static MetadataTypeReference GetNamedType(StringSpan typeFullName, StringSpan assemblyName)
        {
            var nestedNameSplit = typeFullName.IndexOf('+');
            var topLevelName = nestedNameSplit == -1 ? typeFullName : typeFullName.Slice(0, nestedNameSplit);

            var namespaceEndSplit = topLevelName.LastIndexOf('.');

            var topLevelNamespace = namespaceEndSplit == -1 ? null : topLevelName.Slice(0, namespaceEndSplit).ToString();
            var topLevelTypeName = topLevelName.Slice(namespaceEndSplit + 1).ToString();

            MetadataTypeReference current = new TopLevelTypeReference(
                assemblyName.Length == 0 ? null : new AssemblyName(assemblyName.ToString()),
                topLevelNamespace,
                topLevelTypeName);

            if (nestedNameSplit == -1) return current;

            var remainingNestedName = typeFullName.Slice(nestedNameSplit + 1);

            for (;;)
            {
                nestedNameSplit = remainingNestedName.IndexOf('+');
                if (nestedNameSplit == -1) return new NestedTypeReference(current, remainingNestedName.ToString());
                current = new NestedTypeReference(current, remainingNestedName.Slice(0, nestedNameSplit).ToString());
                remainingNestedName = remainingNestedName.Slice(nestedNameSplit + 1);
            }
        }

        private static char? TryRead(ref StringSpan span)
        {
            if (span.Length == 0) return null;
            var r = span[0];
            span = span.Slice(1);
            return r;
        }

        private static bool TryRead(ref StringSpan span, char value)
        {
            if (span.Length == 0 || span[0] != value) return false;
            span = span.Slice(1);
            return true;
        }
    }
}
