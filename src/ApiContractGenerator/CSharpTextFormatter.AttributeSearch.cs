using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.AttributeValues;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator
{
    partial class CSharpTextFormatter
    {
        public abstract class AttributeSearch
        {
            protected abstract bool TryMatch(string @namespace, string name, IMetadataAttribute attribute);

            public static IReadOnlyList<IMetadataAttribute> Extract(IReadOnlyList<IMetadataAttribute> attributes, params AttributeSearch[] searches)
            {
                if (attributes == null) throw new ArgumentNullException(nameof(attributes));
                if (searches == null) throw new ArgumentNullException(nameof(searches));
                if (searches.Length == 0) return attributes;

                var leftOver = (List<IMetadataAttribute>)null;

                for (var i = attributes.Count - 1; i >= 0; i--)
                {
                    var attribute = attributes[i];
                    if (!(attribute.AttributeType is TopLevelTypeReference topLevel)) continue;

                    var matched = false;

                    foreach (var search in searches)
                    {
                        if (search.TryMatch(topLevel.Namespace, topLevel.Name, attribute))
                        {
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


            public static AttributeSearch<bool> ExtensionAttribute() => new SimpleAttributeSearch("System.Runtime.CompilerServices", "ExtensionAttribute");

            public static AttributeSearch<bool> ParamArrayAttribute() => new SimpleAttributeSearch("System", "ParamArrayAttribute");

            public static AttributeSearch<bool> IteratorStateMachineAttribute() => new SimpleAttributeSearch("System.Runtime.CompilerServices", "IteratorStateMachineAttribute");

            private sealed class SimpleAttributeSearch : AttributeSearch<bool>
            {
                public SimpleAttributeSearch(string @namespace, string name) : base(@namespace, name)
                {
                }

                protected override bool TryMatch(IMetadataAttribute attribute, out bool result) => result = true;
            }


            public static AttributeSearch<string> DefaultMemberAttribute() => new DefaultMemberAttributeSearch();

            private sealed class DefaultMemberAttributeSearch : AttributeSearch<string>
            {
                public DefaultMemberAttributeSearch() : base("System.Reflection", "DefaultMemberAttribute")
                {
                }

                protected override bool TryMatch(IMetadataAttribute attribute, out string result)
                {
                    if (attribute.FixedArguments.FirstOrDefault() is ConstantAttributeValue constant
                        && constant.Value.TypeCode == ConstantTypeCode.String)
                    {
                        result = constant.Value.GetValueAsString();
                        return true;
                    }
                    result = default;
                    return false;
                }
            }
        }

        public abstract class AttributeSearch<T> : AttributeSearch
        {
            private readonly string @namespace;
            private readonly string name;

            public T Result { get; private set; }

            protected AttributeSearch(string @namespace, string name)
            {
                this.@namespace = @namespace;
                this.name = name;
            }

            protected abstract bool TryMatch(IMetadataAttribute attribute, out T result);

            protected sealed override bool TryMatch(string @namespace, string name, IMetadataAttribute attribute)
            {
                if (@namespace != this.@namespace || name != this.name || !TryMatch(attribute, out var result)) return false;
                Result = result;
                return true;
            }
        }
    }
}
