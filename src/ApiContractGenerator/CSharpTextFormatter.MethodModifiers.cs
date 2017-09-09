using System;
using ApiContractGenerator.Model;

namespace ApiContractGenerator
{
    public sealed partial class CSharpTextFormatter
    {
        private sealed class MethodModifiers
        {
            public readonly MetadataVisibility Visibility;
            public readonly bool Static;
            public readonly bool Abstract;
            public readonly bool Virtual;
            public readonly bool Final;
            public readonly bool Override;

            private MethodModifiers(MetadataVisibility visibility, bool @static, bool @abstract, bool @virtual, bool final, bool @override)
            {
                Visibility = visibility;
                Static = @static;
                Abstract = @abstract;
                Virtual = @virtual;
                Final = final;
                Override = @override;
            }

            public static MethodModifiers FromMethod(IMetadataMethod method)
            {
                if (method == null) throw new ArgumentNullException(nameof(method));
                return new MethodModifiers(method.Visibility, method.IsStatic, method.IsAbstract, method.IsVirtual, method.IsFinal, method.IsOverride);
            }

            /// <param name="accessorModifiers">Null array elements are permitted but there must be at least one non-null element.</param>
            public static MethodModifiers CombineAccessors(params IMetadataMethod[] accessors)
            {
                if (accessors == null) throw new ArgumentNullException(nameof(accessors));

                var visibility = (MetadataVisibility)0;
                var @static = true;
                var @abstract = true;
                var @virtual = true;
                var final = true;
                var @override = true;
                var any = false;

                foreach (var method in accessors)
                {
                    if (method == null) continue;
                    any = true;

                    switch (method.Visibility)
                    {
                        case MetadataVisibility.Public:
                            visibility = MetadataVisibility.Public;
                            break;
                        case MetadataVisibility.Protected:
                            if (visibility == 0) visibility = MetadataVisibility.Protected;
                            break;
                    }

                    @static &= method.IsStatic;
                    @abstract &= method.IsAbstract;
                    @virtual &= method.IsVirtual;
                    final &= method.IsFinal;
                    @override &= method.IsOverride;
                }

                if (!any) throw new ArgumentException("There must be at least one accessor.", nameof(accessors));

                return new MethodModifiers(visibility, @static, @abstract, @virtual, final, @override);
            }

            public MethodModifiers Except(MethodModifiers modifiersToRemoveIfEqual)
            {
                return new MethodModifiers(
                    Visibility == modifiersToRemoveIfEqual.Visibility ? 0 : Visibility,
                    Static & !modifiersToRemoveIfEqual.Static,
                    Abstract & !modifiersToRemoveIfEqual.Abstract,
                    Virtual & !modifiersToRemoveIfEqual.Virtual,
                    Final & !modifiersToRemoveIfEqual.Final,
                    Override & !modifiersToRemoveIfEqual.Override);
            }
        }
    }
}
