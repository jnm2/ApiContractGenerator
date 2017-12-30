using System;
using System.Collections.Generic;
using ApiContractGenerator.Internal;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    public static class SerializedTypeReader
    {
        public static MetadataTypeReference Deserialize(string serializedName)
        {
            var remaining = (StringSpan)serializedName;

            var (typeName, genericArguments, nestedArrayRanks) = ReadTypeParts(ref remaining);
            var assemblyName = TryReadAssemblyName(remaining);

            return CreateType(typeName, assemblyName, genericArguments, nestedArrayRanks);
        }

        private static (StringSpan typeName, IReadOnlyList<MetadataTypeReference> genericArguments, IReadOnlyList<int> nestedArrayRanks)
            ReadTypeParts(ref StringSpan remaining)
        {
            var firstComma = remaining.IndexOf(',');
            var beforeComma = firstComma != -1 ? remaining.Slice(0, firstComma) : remaining;
            var firstBracket = beforeComma.IndexOf('[');
            if (firstBracket == -1)
            {
                var outerEndingBracket = beforeComma.IndexOf(']');
                if (outerEndingBracket != -1)
                {
                    remaining = remaining.Slice(outerEndingBracket);
                    return (beforeComma.Slice(0, outerEndingBracket), null, null);
                }

                remaining = firstComma != -1 ? remaining.Slice(firstComma) : default;
                return (beforeComma, null, null);
            }

            var typeName = remaining.Slice(0, firstBracket);
            remaining = remaining.Slice(firstBracket + 1);

            var nestedArrayRanks = (List<int>)null;

            IReadOnlyList<MetadataTypeReference> genericArguments;
            var hasGenericAritySeparator = typeName.LastIndexOf('`') != -1;
            if (!hasGenericAritySeparator)
            {
                genericArguments = null;
                nestedArrayRanks = new List<int> { ReadArrayRank(ref remaining) };
            }
            else
            {
                genericArguments = ReadGenericArgumentList(ref remaining);
                if (genericArguments.Count == 0) throw new FormatException("Expected: generic argument");
            }

            for (;;)
            {
                while (TryRead(ref remaining, ' ')) { }
                if (!TryRead(ref remaining, '[')) break;
                if (nestedArrayRanks == null) nestedArrayRanks = new List<int>();
                nestedArrayRanks.Add(ReadArrayRank(ref remaining));
            }

            return (typeName, genericArguments, nestedArrayRanks);
        }

        private static int ReadArrayRank(ref StringSpan remaining)
        {
            var rank = 1;

            for (;;)
            {
                switch (TryRead(ref remaining))
                {
                    case ',':
                        rank++;
                        break;
                    case ' ':
                        break;
                    case ']':
                        return rank;
                    default:
                        throw new FormatException("Expected ' ', ',', or ']'");
                }
            }
        }

        private static IReadOnlyList<MetadataTypeReference> ReadGenericArgumentList(ref StringSpan remaining)
        {
            var r = new List<MetadataTypeReference>();
            for (; ; )
            {
                if (TryRead(ref remaining, ' ')) continue;

                r.Add(ReadGenericArgument(ref remaining));
                while (TryRead(ref remaining, ' ')) { }
                switch (TryRead(ref remaining))
                {
                    case ']':
                        return r;
                    case ',':
                        break;
                    default:
                        throw new FormatException("Expected ' ', ',' or ']'");
                }
            }
        }

        private static MetadataTypeReference ReadGenericArgument(ref StringSpan remaining)
        {
            var isBracketed = TryRead(ref remaining, '[');
            var (typeName, genericArguments, nestedArrayRanks) = ReadTypeParts(ref remaining);

            var assemblyName = default(StringSpan);
            if (isBracketed)
            {
                var endingBracket = remaining.IndexOf(']');
                if (endingBracket == -1) throw new FormatException("Expected ']'");
                assemblyName = TryReadAssemblyName(remaining.Slice(0, endingBracket));
                remaining = remaining.Slice(endingBracket + 1);
            }

            return CreateType(typeName, assemblyName, genericArguments, nestedArrayRanks);
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

        private static MetadataTypeReference CreateType(StringSpan typeFullName, StringSpan assemblyName, IReadOnlyList<MetadataTypeReference> genericArguments, IReadOnlyList<int> nestedArrayRanks)
        {
            var pointerLevels = 0;
            for (var i = typeFullName.Length - 1; i >= 0 && typeFullName[i] == '*'; i--)
            {
                typeFullName = typeFullName.Slice(0, typeFullName.Length - 1);
                pointerLevels++;
            }

            var nestedNameSplit = typeFullName.IndexOf('+');
            var topLevelName = nestedNameSplit == -1 ? typeFullName : typeFullName.Slice(0, nestedNameSplit);

            var namespaceEndSplit = topLevelName.LastIndexOf('.');

            var topLevelNamespace = namespaceEndSplit == -1 ? null : topLevelName.Slice(0, namespaceEndSplit).ToString();
            var topLevelTypeName = topLevelName.Slice(namespaceEndSplit + 1).ToString();

            MetadataTypeReference current = new TopLevelTypeReference(
                assemblyName.Length == 0 ? null : MetadataAssemblyReference.FromFullName(assemblyName.ToString()),
                topLevelNamespace,
                topLevelTypeName);

            if (nestedNameSplit != -1)
            {
                var remainingNestedName = typeFullName.Slice(nestedNameSplit + 1);

                for (;;)
                {
                    nestedNameSplit = remainingNestedName.IndexOf('+');
                    if (nestedNameSplit == -1) break;
                    current = new NestedTypeReference(current, remainingNestedName.Slice(0, nestedNameSplit).ToString());
                    remainingNestedName = remainingNestedName.Slice(nestedNameSplit + 1);
                }

                current = new NestedTypeReference(current, remainingNestedName.ToString());
            }

            if (genericArguments != null)
                current = new GenericInstantiationTypeReference(current, genericArguments);

            for (; pointerLevels > 0; pointerLevels--)
                current = new PointerTypeReference(current);

            if (nestedArrayRanks != null)
                for (var i = nestedArrayRanks.Count - 1; i >= 0; i--)
                    current = new ArrayTypeReference(current, nestedArrayRanks[i]);

            return current;
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
