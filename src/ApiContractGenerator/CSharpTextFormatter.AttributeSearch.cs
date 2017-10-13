using System;
using System.Collections.Generic;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    partial class CSharpTextFormatter
    {
        private sealed class AttributeSearch
        {
            private readonly string name;

            public IMetadataAttribute Result { get; private set; }

            private AttributeSearch(string name)
            {
                this.name = name;
            }

            public static AttributeSearch ExtensionAttribute() => new AttributeSearch("ExtensionAttribute");

            public static bool operator true(AttributeSearch search) => search.Result != null;
            public static bool operator false(AttributeSearch search) => search.Result == null;


            public static IReadOnlyList<IMetadataAttribute> Extract(IReadOnlyList<IMetadataAttribute> attributes, params AttributeSearch[] searches)
            {
                if (attributes == null) throw new ArgumentNullException(nameof(attributes));
                if (searches == null) throw new ArgumentNullException(nameof(searches));
                if (searches.Length == 0) return attributes;

                var leftOver = (List<IMetadataAttribute>)null;

                for (var i = attributes.Count - 1; i >= 0; i--)
                {
                    var attribute = attributes[i];
                    if (!(attribute.AttributeType is TopLevelTypeReference topLevel
                          && topLevel.Namespace == "System.Runtime.CompilerServices")) continue;

                    var matched = false;

                    foreach (var search in searches)
                    {
                        if (search.name == topLevel.Name)
                        {
                            search.Result = attribute;
                            matched = true;
                            break;
                        }
                    }

                    if (matched)
                    {
                        if (leftOver == null) leftOver = new List<IMetadataAttribute>(attributes);
                        leftOver.RemoveAt(i);
                    }
                }

                return leftOver ?? attributes;
            }
        }
    }
}
