using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using ApiContractGenerator.Internal;
using ApiContractGenerator.MetadataReferenceResolvers;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.AttributeValues;
using ApiContractGenerator.Model.TypeReferences;
using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public sealed partial class CSharpTextFormatter : IMetadataWriter
    {
        private readonly IndentedTextWriter writer;
        private readonly IMetadataReferenceResolver metadataReferenceResolver;
        private readonly bool spaceLines;

        public CSharpTextFormatter(TextWriter writer, IMetadataReferenceResolver metadataReferenceResolver, bool spaceLines = true)
        {
            this.writer = new IndentedTextWriter(writer);
            this.metadataReferenceResolver = metadataReferenceResolver;
            this.spaceLines = spaceLines;
        }

        private void WriteLineSpacing()
        {
            if (spaceLines) writer.WriteLine();
        }

        public void Write(IMetadataSource metadataSource)
        {
            foreach (var metadataNamespace in metadataSource.Namespaces.OrderBy(_ => _.Name))
                Write(metadataNamespace);
        }

        public void Write(IMetadataNamespace metadataNamespace)
        {
            var writeNamespace = !string.IsNullOrEmpty(metadataNamespace.Name);

            if (writeNamespace)
            {
                writer.Write("namespace ");
                writer.WriteLine(metadataNamespace.Name);
                writer.WriteLine('{');
                writer.Indent();
            }

            var isFirst = true;
            foreach (var type in metadataNamespace.Types.OrderBy(_ => _.Name))
            {
                if (isFirst) isFirst = false; else WriteLineSpacing();
                Write(type, metadataNamespace.Name, declaringTypeNumGenericParameters: 0);
            }

            if (writeNamespace)
            {
                writer.Unindent();
                writer.WriteLine('}');
            }
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

        private void WriteTypeNameAndGenericSignature(IMetadataType type, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            var genericParameters = type.GenericTypeParameters;
            if (genericParameters.Count == 0)
            {
                writer.Write(type.Name);
            }
            else
            {
                var genericSuffixIndex = type.Name.LastIndexOf('`');
                if (genericSuffixIndex == -1)
                {
                    // Nested types have their own copies of the declaring type's generic parameters.
                    // They only have a generic arity suffix if they have an additional generic parameter that the declaring type does not have.
                    writer.Write(type.Name);
                }
                else
                {
                    var buffer = new char[genericSuffixIndex];
                    type.Name.CopyTo(0, buffer, 0, buffer.Length);
                    writer.Write(buffer);
                }
                WriteGenericSignature(type.GenericTypeParameters, currentNamespace, declaringTypeNumGenericParameters);
            }
        }

        private void WriteGenericSignature(IReadOnlyList<IMetadataGenericTypeParameter> genericParameters, string currentNamespace, int numToSkip)
        {
            if (numToSkip >= genericParameters.Count) return;

            writer.Write('<');

            for (var i = numToSkip; i < genericParameters.Count; i++)
            {
                if (i != numToSkip) writer.Write(", ");

                var parameter = genericParameters[i];
                WriteAttributes(parameter.Attributes, currentNamespace, newLines: false);
                if (parameter.IsContravariant) writer.Write("in ");
                if (parameter.IsCovariant) writer.Write("out ");
                writer.Write(parameter.Name);
            }

            writer.Write('>');
        }

        private void WriteGenericConstraints(IReadOnlyList<IMetadataGenericTypeParameter> genericParameters, string currentNamespace)
        {
            foreach (var genericParameter in genericParameters)
            {
                var typeConstraints = genericParameter.TypeConstraints;
                if (genericParameter.HasReferenceTypeConstraint
                    || genericParameter.HasNotNullableValueTypeConstraint
                    || genericParameter.HasDefaultConstructorConstraint
                    || typeConstraints.Count != 0)
                {
                    writer.Write(" where ");
                    writer.Write(genericParameter.Name);
                    writer.Write(" : ");

                    var isFirst = true;

                    if (genericParameter.HasReferenceTypeConstraint)
                    {
                        isFirst = false;
                        writer.Write("class");
                    }
                    if (genericParameter.HasNotNullableValueTypeConstraint)
                    {
                        if (isFirst) isFirst = false; else writer.Write(", ");
                        writer.Write("struct");
                    }

                    foreach (var type in typeConstraints)
                    {
                        if (genericParameter.HasNotNullableValueTypeConstraint
                            && type is TopLevelTypeReference topLevel
                            && topLevel.Name == "ValueType"
                            && topLevel.Namespace == "System")
                        {
                            continue;
                        }

                        if (isFirst) isFirst = false; else writer.Write(", ");
                        Write(type, currentNamespace);
                    }

                    if (genericParameter.HasDefaultConstructorConstraint && !genericParameter.HasNotNullableValueTypeConstraint)
                    {
                        if (!isFirst) writer.Write(", ");
                        writer.Write("new()");
                    }
                }
            }
        }

        public void WriteStringLiteral(string literal)
        {
            writer.Write('"');
            foreach (var c in literal)
            {
                switch (c)
                {
                    case '\0':
                        writer.Write("\\0");
                        break;
                    case '\x1':
                        writer.Write("\\x1");
                        break;
                    case '\x2':
                        writer.Write("\\x2");
                        break;
                    case '\x3':
                        writer.Write("\\x3");
                        break;
                    case '\x4':
                        writer.Write("\\x4");
                        break;
                    case '\x5':
                        writer.Write("\\x5");
                        break;
                    case '\x6':
                        writer.Write("\\x6");
                        break;
                    case '\a':
                        writer.Write("\\a");
                        break;
                    case '\b':
                        writer.Write("\\b");
                        break;
                    case '\t':
                        writer.Write("\\t");
                        break;
                    case '\n':
                        writer.Write("\\n");
                        break;
                    case '\v':
                        writer.Write("\\v");
                        break;
                    case '\f':
                        writer.Write("\\f");
                        break;
                    case '\r':
                        writer.Write("\\r");
                        break;
                    case '\xE':
                        writer.Write("\\xE");
                        break;
                    case '\xF':
                        writer.Write("\\xF");
                        break;
                    case '\x10':
                        writer.Write("\\x10");
                        break;
                    case '\x11':
                        writer.Write("\\x11");
                        break;
                    case '\x12':
                        writer.Write("\\x12");
                        break;
                    case '\x13':
                        writer.Write("\\x13");
                        break;
                    case '\x14':
                        writer.Write("\\x14");
                        break;
                    case '\x15':
                        writer.Write("\\x15");
                        break;
                    case '\x16':
                        writer.Write("\\x16");
                        break;
                    case '\x17':
                        writer.Write("\\x17");
                        break;
                    case '\x18':
                        writer.Write("\\x18");
                        break;
                    case '\x19':
                        writer.Write("\\x19");
                        break;
                    case '\x1A':
                        writer.Write("\\x1A");
                        break;
                    case '\x1B':
                        writer.Write("\\x1B");
                        break;
                    case '\x1C':
                        writer.Write("\\x1C");
                        break;
                    case '\x1D':
                        writer.Write("\\x1D");
                        break;
                    case '\x1E':
                        writer.Write("\\x1E");
                        break;
                    case '\x1F':
                        writer.Write("\\x1F");
                        break;
                    case '"':
                        writer.Write("\\\"");
                        break;
                    case '\\':
                        writer.Write("\\\\");
                        break;
                    default:
                        writer.Write(c);
                        break;
                }
            }
            writer.Write('"');
        }

        public void WriteCharLiteral(char literal)
        {
            writer.Write('\'');
            switch (literal)
            {
                case '\0':
                    writer.Write("\\0");
                    break;
                case '\x1':
                    writer.Write("\\x1");
                    break;
                case '\x2':
                    writer.Write("\\x2");
                    break;
                case '\x3':
                    writer.Write("\\x3");
                    break;
                case '\x4':
                    writer.Write("\\x4");
                    break;
                case '\x5':
                    writer.Write("\\x5");
                    break;
                case '\x6':
                    writer.Write("\\x6");
                    break;
                case '\a':
                    writer.Write("\\a");
                    break;
                case '\b':
                    writer.Write("\\b");
                    break;
                case '\t':
                    writer.Write("\\t");
                    break;
                case '\n':
                    writer.Write("\\n");
                    break;
                case '\v':
                    writer.Write("\\v");
                    break;
                case '\f':
                    writer.Write("\\f");
                    break;
                case '\r':
                    writer.Write("\\r");
                    break;
                case '\xE':
                    writer.Write("\\xE");
                    break;
                case '\xF':
                    writer.Write("\\xF");
                    break;
                case '\x10':
                    writer.Write("\\x10");
                    break;
                case '\x11':
                    writer.Write("\\x11");
                    break;
                case '\x12':
                    writer.Write("\\x12");
                    break;
                case '\x13':
                    writer.Write("\\x13");
                    break;
                case '\x14':
                    writer.Write("\\x14");
                    break;
                case '\x15':
                    writer.Write("\\x15");
                    break;
                case '\x16':
                    writer.Write("\\x16");
                    break;
                case '\x17':
                    writer.Write("\\x17");
                    break;
                case '\x18':
                    writer.Write("\\x18");
                    break;
                case '\x19':
                    writer.Write("\\x19");
                    break;
                case '\x1A':
                    writer.Write("\\x1A");
                    break;
                case '\x1B':
                    writer.Write("\\x1B");
                    break;
                case '\x1C':
                    writer.Write("\\x1C");
                    break;
                case '\x1D':
                    writer.Write("\\x1D");
                    break;
                case '\x1E':
                    writer.Write("\\x1E");
                    break;
                case '\x1F':
                    writer.Write("\\x1F");
                    break;
                case '\'':
                    writer.Write("\'");
                    break;
                case '\\':
                    writer.Write("\\\\");
                    break;
                default:
                    writer.Write(literal);
                    break;
            }
            writer.Write('\'');
        }

        /// <summary>
        /// Handles enums and reference and value type defaults.
        /// </summary>
        private void WriteConstant(MetadataTypeReference type, IMetadataConstantValue metadataConstantValue, string currentNamespace, bool canTargetType)
        {
            var isNullable = false;

            if (type is GenericInstantiationTypeReference genericInstantiation
                && genericInstantiation.TypeDefinition is TopLevelTypeReference genericInstantiationTopLevel
                && genericInstantiationTopLevel.Name == "Nullable`1"
                && genericInstantiationTopLevel.Namespace == "System")
            {
                type = genericInstantiation.GenericTypeArguments[0];
                isNullable = true;
            }

            if (type is PrimitiveTypeReference primitive)
            {
                switch (primitive.Code)
                {
                    case PrimitiveTypeCode.IntPtr:
                    case PrimitiveTypeCode.UIntPtr:
                    case PrimitiveTypeCode.TypedReference:
                        if (metadataConstantValue == null || metadataConstantValue.TypeCode == ConstantTypeCode.NullReference)
                        {
                            writer.Write(isNullable ? "null" : "default");
                            return;
                        }
                        else
                        {
                            writer.Write("(System.");
                            writer.Write(primitive.Code);
                            writer.Write(')');
                            break; // It's going to be invalid C# but it'll be transparent
                        }
                }
                WriteConstantPrimitive(metadataConstantValue);
            }
            else if (metadataConstantValue == null || metadataConstantValue.TypeCode == ConstantTypeCode.NullReference)
            {
                if (isNullable || IsNullLiteralAllowed(type))
                {
                    if (!canTargetType)
                    {
                        writer.Write('(');
                        Write(type, currentNamespace);
                        writer.Write(')');
                    }
                    writer.Write("null");
                }
                else
                {
                    writer.Write("default");
                    if (!canTargetType)
                    {
                        writer.Write('(');
                        Write(type, currentNamespace);
                        writer.Write(')');
                    }
                }
            }
            else
            {
                WriteEnumReferenceValue(type, metadataConstantValue, currentNamespace, canTargetType);
            }
        }

        private bool IsNullLiteralAllowed(MetadataTypeReference type)
        {
            switch (type)
            {
                case PrimitiveTypeReference primitive:
                    switch (primitive.Code)
                    {
                        case PrimitiveTypeCode.Object:
                            case PrimitiveTypeCode.String:
                            return true;
                        default:
                            return false;
                    }

                case GenericInstantiationTypeReference genericInstantiation:
                    return IsNullLiteralAllowed(genericInstantiation.TypeDefinition);

                case TopLevelTypeReference topLevel when topLevel.Name == "Nullable`1" && topLevel.Namespace == "System":
                    return true;

                case GenericParameterTypeReference genericParameterReference:
                    if (genericParameterReference.TypeParameter.HasNotNullableValueTypeConstraint) return false;
                    if (genericParameterReference.TypeParameter.HasReferenceTypeConstraint) return true;

                    foreach (var typeConstraint in genericParameterReference.TypeParameter.TypeConstraints)
                        if (IsNullLiteralAllowed(typeConstraint)) return true;

                    return false;

                default:
                    return metadataReferenceResolver.TryGetIsValueType(type, out var isValueType) && !isValueType;
            }
        }

        /// <param name="value">Must be a numeric string formatted using the invariant culture "g" format.</param>
        private void WriteNumericString(string value)
        {
            foreach (var c in value)
                if (c == 'E' || c == 'e')
                {
                    writer.Write(value);
                    return;
                }

            var hasSign = value[0] == '-';
            var span = hasSign ? value.Slice(1) : value;
            var characteristicDigitCount = span.IndexOf('.');
            if (characteristicDigitCount == -1) characteristicDigitCount = span.Length;
            if (characteristicDigitCount < 5)
            {
                writer.Write(value);
                return;
            }

            if (hasSign) writer.Write('-');

            var position = ((characteristicDigitCount - 1) % 3) + 1;

            var digitBuffer = new char[4];
            digitBuffer[0] = '_';

            span.CopyTo(0, digitBuffer, 1, position);
            writer.Write(digitBuffer, 1, position);

            do
            {
                span.CopyTo(position, digitBuffer, 1, 3);
                writer.Write(digitBuffer);
                position += 3;
            } while (position != characteristicDigitCount);

            var remainingCount = span.Length - characteristicDigitCount;
            var remainingBuffer = remainingCount <= 4 ? digitBuffer : new char[remainingCount];
            span.CopyTo(characteristicDigitCount, remainingBuffer, 0, remainingCount);
            writer.Write(remainingBuffer, 0, remainingCount);
        }

        public void WriteConstantPrimitive(IMetadataConstantValue metadataConstantValue)
        {
            switch (metadataConstantValue.TypeCode)
            {
                case ConstantTypeCode.Boolean:
                    writer.Write(metadataConstantValue.GetValueAsBoolean() ? "true" : "false");
                    break;
                case ConstantTypeCode.Char:
                    WriteCharLiteral(metadataConstantValue.GetValueAsChar());
                    break;
                case ConstantTypeCode.SByte:
                    writer.Write(metadataConstantValue.GetValueAsSByte());
                    break;
                case ConstantTypeCode.Byte:
                    writer.Write(metadataConstantValue.GetValueAsByte());
                    break;
                case ConstantTypeCode.Int16:
                {
                    var value = metadataConstantValue.GetValueAsInt16();
                    if (-10_000 < value && value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.UInt16:
                {
                    var value = metadataConstantValue.GetValueAsUInt16();
                    if (value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.Int32:
                {
                    var value = metadataConstantValue.GetValueAsInt32();
                    if (-10_000 < value && value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.UInt32:
                {
                    var value = metadataConstantValue.GetValueAsUInt32();
                    if (value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.Int64:
                {
                    var value = metadataConstantValue.GetValueAsInt64();
                    if (-10_000 < value && value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.UInt64:
                {
                    var value = metadataConstantValue.GetValueAsUInt64();
                    if (value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.Single:
                {
                    var value = metadataConstantValue.GetValueAsSingle();
                    if (-10_000 < value && value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.Double:
                {
                    var value = metadataConstantValue.GetValueAsDouble();
                    if (-10_000 < value && value < 10_000)
                        writer.Write(value);
                    else
                        WriteNumericString(value.ToString(CultureInfo.InvariantCulture));
                    break;
                }
                case ConstantTypeCode.String:
                    WriteStringLiteral(metadataConstantValue.GetValueAsString());
                    break;
                case ConstantTypeCode.NullReference:
                    writer.Write("null");
                    break;
            }
        }

        public void Write(IMetadataField metadataField, string currentNamespace)
        {
            WriteAttributes(metadataField.Attributes, currentNamespace, newLines: false);
            WriteVisibility(metadataField.Visibility);

            if (metadataField.IsLiteral)
                writer.Write("const ");
            else if (metadataField.IsStatic)
                writer.Write("static ");
            if (metadataField.IsInitOnly)
                writer.Write("readonly ");

            Write(metadataField.FieldType, currentNamespace);
            writer.Write(' ');
            writer.Write(metadataField.Name);

            if (metadataField.DefaultValue != null)
            {
                writer.Write(" = ");
                WriteConstant(metadataField.FieldType, metadataField.DefaultValue, currentNamespace, canTargetType: true);
            }

            writer.WriteLine(';');
        }

        private void WriteEnumFields(IMetadataEnum metadataEnum, string currentNamespace)
        {
            var fields = metadataEnum.Fields.Where(_ => _.IsLiteral).ToArray();
            var sortInfo = new EnumFieldInfo[fields.Length];

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                sortInfo[i] = new EnumFieldInfo(field.Name, field.DefaultValue);
            }

            Array.Sort(sortInfo, fields);

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                if (i != 0)
                {
                    writer.WriteLine(',');
                    if (field.Attributes.Count != 0) WriteLineSpacing();
                }

                WriteAttributes(field.Attributes, currentNamespace, newLines: true);
                writer.Write(field.Name);
                writer.Write(" = ");
                WriteConstantPrimitive(field.DefaultValue);
            }
            writer.WriteLine();
        }

        private void WriteMethodModifiers(MethodModifiers modifiers, bool declaringTypeIsInterface)
        {
            if (!(declaringTypeIsInterface && modifiers.Visibility == MetadataVisibility.Public))
                WriteVisibility(modifiers.Visibility);

            if (modifiers.Static)
                writer.Write("static ");
            if (modifiers.Abstract && !declaringTypeIsInterface)
                writer.Write("abstract ");
            if (modifiers.Virtual && !(modifiers.Override || modifiers.Abstract || modifiers.Final))
                writer.Write("virtual ");
            if (modifiers.Final && modifiers.Override)
                writer.Write("sealed ");
            if (modifiers.Override)
                writer.Write("override ");
        }

        public void Write(IMetadataProperty metadataProperty, IMetadataType declaringType, string defaultMemberName, string currentNamespace)
        {
            WriteAttributes(metadataProperty.Attributes, currentNamespace, newLines: true);

            var modifiers = MethodModifiers.CombineAccessors(metadataProperty.GetAccessor, metadataProperty.SetAccessor);
            var declaringTypeIsInterface = declaringType is IMetadataInterface;
            WriteMethodModifiers(modifiers, declaringTypeIsInterface);

            Write(metadataProperty.PropertyType, currentNamespace);
            writer.Write(' ');
            writer.Write(defaultMemberName == metadataProperty.Name ? "this" : metadataProperty.Name);

            if (metadataProperty.ParameterTypes.Count != 0)
            {
                writer.Write('[');

                if (metadataProperty.GetAccessor != null)
                {
                    WriteParameters(metadataProperty.GetAccessor.Parameters, false, currentNamespace);
                }
                else
                {
                    var indexParameter = new IMetadataParameter[metadataProperty.SetAccessor.Parameters.Count - 1];
                    for (var i = 0; i < indexParameter.Length; i++)
                        indexParameter[i] = metadataProperty.SetAccessor.Parameters[i];
                    WriteParameters(indexParameter, false, currentNamespace);
                }

                writer.Write(']');
            }

            writer.Write(" { ");

            if (metadataProperty.GetAccessor != null)
            {
                WriteMethodModifiers(MethodModifiers.FromMethod(metadataProperty.GetAccessor).Except(modifiers), declaringTypeIsInterface);
                writer.Write("get; ");
            }

            if (metadataProperty.SetAccessor != null)
            {
                WriteMethodModifiers(MethodModifiers.FromMethod(metadataProperty.SetAccessor).Except(modifiers), declaringTypeIsInterface);
                writer.Write("set; ");
            }

            writer.WriteLine('}');

            if (metadataProperty.DefaultValue != null)
            {
                writer.WriteLine(" = ");
                WriteConstant(metadataProperty.PropertyType, metadataProperty.DefaultValue, currentNamespace, canTargetType: true);
            }
        }

        public void Write(IMetadataEvent metadataEvent, IMetadataType declaringType, string currentNamespace)
        {
            WriteAttributes(metadataEvent.Attributes, currentNamespace, newLines: true);

            var modifiers = MethodModifiers.CombineAccessors(metadataEvent.AddAccessor, metadataEvent.RemoveAccessor, metadataEvent.RaiseAccessor);
            var declaringTypeIsInterface = declaringType is IMetadataInterface;
            WriteMethodModifiers(modifiers, declaringTypeIsInterface);

            writer.Write("event ");
            Write(metadataEvent.HandlerType, currentNamespace);
            writer.Write(' ');
            writer.Write(metadataEvent.Name);

            if (metadataEvent.AddAccessor != null && metadataEvent.RemoveAccessor != null && metadataEvent.RaiseAccessor == null)
            {
                writer.WriteLine(';');
            }
            else
            {
                writer.Write(" { ");

                if (metadataEvent.AddAccessor != null)
                {
                    WriteMethodModifiers(MethodModifiers.FromMethod(metadataEvent.AddAccessor).Except(modifiers), declaringTypeIsInterface);
                    writer.Write("add; ");
                }

                if (metadataEvent.RemoveAccessor != null)
                {
                    WriteMethodModifiers(MethodModifiers.FromMethod(metadataEvent.RemoveAccessor).Except(modifiers), declaringTypeIsInterface);
                    writer.Write("remove; ");
                }

                if (metadataEvent.RaiseAccessor != null)
                {
                    WriteMethodModifiers(MethodModifiers.FromMethod(metadataEvent.RaiseAccessor).Except(modifiers), declaringTypeIsInterface);
                    writer.Write("raise; ");
                }

                writer.WriteLine('}');
            }
        }

        private void WriteParameters(IReadOnlyList<IMetadataParameter> parameters, bool isExtensionMethod, string currentNamespace)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                if (i != 0) writer.Write(", ");

                var metadataParameter = parameters[i];

                var paramArrayAttribute = AttributeSearch.ParamArrayAttribute();
                WriteAttributes(AttributeSearch.Extract(metadataParameter.Attributes, paramArrayAttribute), currentNamespace, newLines: false);

                if (i == 0 && isExtensionMethod) writer.Write("this ");
                if (paramArrayAttribute.Result) writer.Write("params ");

                if (metadataParameter.IsOut)
                {
                    if (metadataParameter.ParameterType is ByRefTypeReference byref)
                    {
                        writer.Write("out ");
                        Write(byref.ElementType, currentNamespace);
                    }
                    else
                    {
                        Write(metadataParameter.ParameterType, currentNamespace);
                    }
                }
                else
                {
                    Write(metadataParameter.ParameterType, currentNamespace);
                }

                writer.Write(' ');
                writer.Write(metadataParameter.Name);

                if (metadataParameter.IsOptional)
                {
                    writer.Write(" = ");
                    WriteConstant(metadataParameter.ParameterType, metadataParameter.DefaultValue, currentNamespace, canTargetType: true);
                }
            }
        }

        private static readonly IReadOnlyDictionary<string, string> OperatorSyntaxByMethodName = new Dictionary<string, string>
        {
            ["op_UnaryPlus"] = "operator +",
            ["op_UnaryNegation"] = "operator -",
            ["op_LogicalNot"] = "operator !",
            ["op_OnesComplement"] = "operator ~",
            ["op_Increment"] = "operator ++",
            ["op_Decrement"] = "operator --",
            ["op_True"] = "operator true",
            ["op_False"] = "operator false",
            ["op_Addition"] = "operator +",
            ["op_Subtraction"] = "operator -",
            ["op_Multiply"] = "operator *",
            ["op_Division"] = "operator /",
            ["op_Modulus"] = "operator %",
            ["op_BitwiseAnd"] = "operator &",
            ["op_BitwiseOr"] = "operator |",
            ["op_ExclusiveOr"] = "operator ^",
            ["op_LeftShift"] = "operator <<",
            ["op_RightShift"] = "operator >>",
            ["op_Equality"] = "operator ==",
            ["op_Inequality"] = "operator !=",
            ["op_LessThan"] = "operator <",
            ["op_LessThanOrEqual"] = "operator <=",
            ["op_GreaterThan"] = "operator >",
            ["op_GreaterThanOrEqual"] = "operator >="
        };

        public void Write(IMetadataMethod metadataMethod, IMetadataType declaringType, string currentNamespace)
        {
            var extensionAttribute = AttributeSearch.ExtensionAttribute();

            WriteAttributes(
                AttributeSearch.Extract(
                    metadataMethod.Attributes,
                    extensionAttribute,
                    AttributeSearch.IteratorStateMachineAttribute(),
                    AttributeSearch.AsyncStateMachineAttribute()),
                currentNamespace,
                newLines: true);

            WriteMethodModifiers(MethodModifiers.FromMethod(metadataMethod), declaringType is IMetadataInterface);

            if (metadataMethod.Name == ".ctor")
            {
                writer.Write(TrimGenericArity(declaringType.Name));
            }
            else
            {
                switch (metadataMethod.Name)
                {
                    case "op_Explicit":
                        writer.Write("explicit operator ");
                        Write(metadataMethod.ReturnType, currentNamespace);
                        break;
                    case "op_Implicit":
                        writer.Write("implicit operator ");
                        Write(metadataMethod.ReturnType, currentNamespace);
                        break;
                    default:
                        Write(metadataMethod.ReturnType, currentNamespace);
                        writer.Write(' ');
                        writer.Write(OperatorSyntaxByMethodName.TryGetValue(metadataMethod.Name, out var syntax) ? syntax : metadataMethod.Name);
                        break;
                }
            }

            WriteGenericSignature(metadataMethod.GenericTypeParameters, currentNamespace, numToSkip: 0);
            writer.Write('(');
            WriteParameters(metadataMethod.Parameters, extensionAttribute.Result, currentNamespace);
            writer.Write(')');
            WriteGenericConstraints(metadataMethod.GenericTypeParameters, currentNamespace);
            writer.WriteLine(';');
        }

        private void WriteTypeMembers(IMetadataType metadataType, string defaultMemberName, string currentNamespace)
        {
            writer.WriteLine('{');
            writer.Indent();

            var isFirst = true;

            if (metadataType is IMetadataEnum metadataEnum)
            {
                if (metadataEnum.Fields.Count != 0)
                {
                    WriteEnumFields(metadataEnum, currentNamespace);
                    isFirst = false;
                }
            }
            else
            {
                foreach (var field in metadataType.Fields
                    .OrderByDescending(_ => _.IsLiteral)
                    .ThenByDescending(_ => _.IsStatic)
                    .ThenByDescending(_ => _.IsInitOnly)
                    .ThenBy(_ => _.Name))
                {
                    if (isFirst) isFirst = false; else WriteLineSpacing();
                    Write(field, currentNamespace);
                }
            }

            var unusedMethods = new HashSet<IMetadataMethod>(metadataType.Methods);

            foreach (var property in metadataType.Properties
                .OrderByDescending(_ => (_.GetAccessor ?? _.SetAccessor).IsStatic)
                .ThenBy(_ => _.Name))
            {
                if (property.GetAccessor != null) unusedMethods.Remove(property.GetAccessor);
                if (property.SetAccessor != null) unusedMethods.Remove(property.SetAccessor);

                if (isFirst) isFirst = false; else WriteLineSpacing();
                Write(property, metadataType, defaultMemberName, currentNamespace);
            }

            foreach (var @event in metadataType.Events
                .OrderByDescending(_ => (_.AddAccessor ?? _.RemoveAccessor ?? _.RaiseAccessor).IsStatic)
                .ThenBy(_ => _.Name))
            {
                if (@event.AddAccessor != null) unusedMethods.Remove(@event.AddAccessor);
                if (@event.RemoveAccessor != null) unusedMethods.Remove(@event.RemoveAccessor);
                if (@event.RaiseAccessor != null) unusedMethods.Remove(@event.RaiseAccessor);

                if (isFirst) isFirst = false; else WriteLineSpacing();
                Write(@event, metadataType, currentNamespace);
            }

            var operatorMethods = new List<IMetadataMethod>();
            foreach (var method in metadataType.Methods)
            {
                if (!method.Name.StartsWith("op_", StringComparison.Ordinal)) continue;
                operatorMethods.Add(method);
                unusedMethods.Remove(method);
            }

            foreach (var method in unusedMethods
                .OrderByDescending(_ => _.IsStatic)
                .ThenByDescending(_ => _.Name == ".ctor")
                .ThenBy(_ => _.Name))
            {
                if (isFirst) isFirst = false; else WriteLineSpacing();
                Write(method, metadataType, currentNamespace);
            }

            operatorMethods.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
            foreach (var @operator in operatorMethods)
            {
                if (isFirst) isFirst = false; else WriteLineSpacing();
                Write(@operator, metadataType, currentNamespace);
            }

            foreach (var nestedType in metadataType.NestedTypes.OrderBy(_ => _.Name))
            {
                if (isFirst) isFirst = false; else WriteLineSpacing();
                Write(nestedType, currentNamespace, metadataType.GenericTypeParameters.Count);
            }

            writer.Unindent();
            writer.WriteLine('}');
        }

        private void WriteBaseTypeAndInterfaces(IMetadataType metadataType, string currentNamespace)
        {
            var didWriteColon = false;

            if (metadataType.BaseType != null
                && !(
                    metadataType is IMetadataClass
                    && metadataType.BaseType is TopLevelTypeReference topLevel
                    && topLevel.Namespace == "System"
                    && topLevel.Name == "Object"))
            {
                writer.Write(" : ");
                didWriteColon = true;
                Write(metadataType.BaseType, currentNamespace);
            }

            foreach (var interfaceImplementation in metadataType.InterfaceImplementations)
            {
                if (!didWriteColon)
                {
                    writer.Write(" : ");
                    didWriteColon = true;
                }
                else
                {
                    writer.Write(", ");
                }

                Write(interfaceImplementation, currentNamespace);
            }
        }

        private void Write(IMetadataType metadataType, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            switch (metadataType)
            {
                case IMetadataClass metadataClass:
                    Write(metadataClass, currentNamespace, declaringTypeNumGenericParameters);
                    break;
                case IMetadataStruct metadataStruct:
                    Write(metadataStruct, currentNamespace, declaringTypeNumGenericParameters);
                    break;
                case IMetadataEnum metadataEnum:
                    Write(metadataEnum, currentNamespace, declaringTypeNumGenericParameters);
                    break;
                case IMetadataDelegate metadataDelegate:
                    Write(metadataDelegate, currentNamespace, declaringTypeNumGenericParameters);
                    break;
                case IMetadataInterface metadataInterface:
                    Write(metadataInterface, currentNamespace, declaringTypeNumGenericParameters);
                    break;
            }
        }

        public void Write(IMetadataClass metadataClass, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            var defaultMemberAttribute = AttributeSearch.DefaultMemberAttribute();

            WriteAttributes(AttributeSearch.Extract(metadataClass.Attributes, AttributeSearch.ExtensionAttribute(), defaultMemberAttribute), currentNamespace, newLines: true);
            WriteVisibility(metadataClass.Visibility);

            if (metadataClass.IsStatic)
                writer.Write("static ");
            else if (metadataClass.IsAbstract)
                writer.Write("abstract ");
            else if (metadataClass.IsSealed)
                writer.Write("sealed ");

            writer.Write("class ");
            WriteTypeNameAndGenericSignature(metadataClass, currentNamespace, declaringTypeNumGenericParameters);
            WriteBaseTypeAndInterfaces(metadataClass, currentNamespace);
            WriteGenericConstraints(metadataClass.GenericTypeParameters, currentNamespace);
            writer.WriteLine();
            WriteTypeMembers(metadataClass, defaultMemberAttribute.Result, currentNamespace);
        }

        public void Write(IMetadataStruct metadataStruct, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            var defaultMemberAttribute = AttributeSearch.DefaultMemberAttribute();

            WriteAttributes(AttributeSearch.Extract(metadataStruct.Attributes, defaultMemberAttribute), currentNamespace, newLines: true);
            WriteVisibility(metadataStruct.Visibility);
            writer.Write("struct ");
            WriteTypeNameAndGenericSignature(metadataStruct, currentNamespace, declaringTypeNumGenericParameters);
            writer.WriteLine();
            WriteTypeMembers(metadataStruct, defaultMemberAttribute.Result, currentNamespace);
        }

        public void Write(IMetadataInterface metadataInterface, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            var defaultMemberAttribute = AttributeSearch.DefaultMemberAttribute();

            WriteAttributes(AttributeSearch.Extract(metadataInterface.Attributes, defaultMemberAttribute), currentNamespace, newLines: true);
            WriteVisibility(metadataInterface.Visibility);
            writer.Write("interface ");
            WriteTypeNameAndGenericSignature(metadataInterface, currentNamespace, declaringTypeNumGenericParameters);
            WriteBaseTypeAndInterfaces(metadataInterface, currentNamespace);
            writer.WriteLine();
            WriteTypeMembers(metadataInterface, defaultMemberAttribute.Result, currentNamespace);
        }

        public void Write(IMetadataEnum metadataEnum, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            var defaultMemberAttribute = AttributeSearch.DefaultMemberAttribute();

            WriteAttributes(AttributeSearch.Extract(metadataEnum.Attributes, defaultMemberAttribute), currentNamespace, newLines: true);
            WriteVisibility(metadataEnum.Visibility);
            writer.Write("enum ");
            WriteTypeNameAndGenericSignature(metadataEnum, currentNamespace, declaringTypeNumGenericParameters);
            writer.Write(" : ");
            Write(metadataEnum.UnderlyingType, currentNamespace);
            writer.WriteLine();
            WriteTypeMembers(metadataEnum, defaultMemberAttribute.Result, currentNamespace);
        }

        public void Write(IMetadataDelegate metadataDelegate, string currentNamespace, int declaringTypeNumGenericParameters)
        {
            WriteAttributes(metadataDelegate.Attributes, currentNamespace, newLines: true);
            WriteVisibility(metadataDelegate.Visibility);
            writer.Write("delegate ");
            Write(metadataDelegate.ReturnType, currentNamespace);
            writer.Write(' ');
            WriteTypeNameAndGenericSignature(metadataDelegate, currentNamespace, declaringTypeNumGenericParameters);
            writer.Write('(');
            WriteParameters(metadataDelegate.Parameters, false, currentNamespace);
            writer.WriteLine(");");
        }

        private static bool TryShortenAttributeName(string fullAttributeName, out string shortenedAttributeName)
        {
            if (fullAttributeName.EndsWith("Attribute"))
            {
                shortenedAttributeName = fullAttributeName.Substring(0, fullAttributeName.Length - 9);
                return true;
            }

            shortenedAttributeName = null;
            return false;
        }

        private static MetadataTypeReference GetShortenedAttributeName(MetadataTypeReference fullAttributeName)
        {
            switch (fullAttributeName)
            {
                case TopLevelTypeReference topLevel:
                {
                    if (TryShortenAttributeName(topLevel.Name, out var shortened))
                        return new TopLevelTypeReference(topLevel.Assembly, topLevel.Namespace, shortened);
                    break;
                }
                case NestedTypeReference nested:
                {
                    if (TryShortenAttributeName(nested.Name, out var shortened))
                        return new NestedTypeReference(nested.DeclaringType, shortened);
                    break;
                }
            }

            return null;
        }

        private void WriteAttributeArgumentValue(MetadataAttributeValue value, string currentNamespace)
        {
            switch (value)
            {
                case ConstantAttributeValue constant:
                    WriteConstantPrimitive(constant.Value);
                    break;

                case ArrayAttributeValue array:
                    writer.Write("new ");

                    if (array.Elements.Count == 0)
                    {
                        Write(array.ArrayType.ElementType, currentNamespace);
                        writer.Write("[0]");
                    }
                    else
                    {
                        Write(array.ArrayType, currentNamespace);
                        writer.Write(" { ");

                        var isFirst = true;

                        foreach (var element in array.Elements)
                        {
                            if (isFirst) isFirst = false; else writer.Write(", ");
                            WriteAttributeArgumentValue(element, currentNamespace);
                        }

                        writer.Write(" }");
                    }
                    break;

                case TypeAttributeValue type:
                    writer.Write("typeof(");
                    Write(type.Value, currentNamespace);
                    writer.Write(')');
                    break;

                case EnumAttributeValue @enum:
                    WriteEnumReferenceValue(@enum.EnumType, @enum.UnderlyingValue, currentNamespace, canTargetType: false);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static bool Equals(IMetadataConstantValue x, IMetadataConstantValue y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.TypeCode != y.TypeCode) return false;

            switch (x.TypeCode)
            {
                case ConstantTypeCode.NullReference:
                    return true;
                case ConstantTypeCode.Boolean:
                    return x.GetValueAsBoolean() == y.GetValueAsBoolean();
                case ConstantTypeCode.Char:
                    return x.GetValueAsChar() == y.GetValueAsChar();
                case ConstantTypeCode.SByte:
                    return x.GetValueAsSByte() == y.GetValueAsSByte();
                case ConstantTypeCode.Byte:
                    return x.GetValueAsByte() == y.GetValueAsByte();
                case ConstantTypeCode.Int16:
                    return x.GetValueAsInt16() == y.GetValueAsInt16();
                case ConstantTypeCode.UInt16:
                    return x.GetValueAsUInt16() == y.GetValueAsUInt16();
                case ConstantTypeCode.Int32:
                    return x.GetValueAsInt32() == y.GetValueAsInt32();
                case ConstantTypeCode.UInt32:
                    return x.GetValueAsUInt32() == y.GetValueAsUInt32();
                case ConstantTypeCode.Int64:
                    return x.GetValueAsInt64() == y.GetValueAsInt64();
                case ConstantTypeCode.UInt64:
                    return x.GetValueAsUInt64() == y.GetValueAsUInt64();
                case ConstantTypeCode.Single:
                    return x.GetValueAsSingle() == y.GetValueAsSingle();
                case ConstantTypeCode.Double:
                    return x.GetValueAsDouble() == y.GetValueAsDouble();
                case ConstantTypeCode.String:
                    return x.GetValueAsString() == y.GetValueAsString();
                default:
                    throw new NotImplementedException();
            }
        }

        private static ulong ConvertEnumValueToUInt64(IMetadataConstantValue value)
        {
            switch (value?.TypeCode)
            {
                case ConstantTypeCode.Boolean:
                    return value.GetValueAsBoolean() ? 1ul : 0ul;
                case ConstantTypeCode.Char:
                    return value.GetValueAsChar();
                case ConstantTypeCode.SByte:
                    return unchecked((ulong)value.GetValueAsSByte());
                case ConstantTypeCode.Byte:
                    return value.GetValueAsByte();
                case ConstantTypeCode.Int16:
                    return unchecked((ulong)value.GetValueAsInt16());
                case ConstantTypeCode.UInt16:
                    return value.GetValueAsUInt16();
                case ConstantTypeCode.Int32:
                    return unchecked((ulong)value.GetValueAsInt32());
                case ConstantTypeCode.UInt32:
                    return value.GetValueAsUInt32();
                case ConstantTypeCode.Int64:
                    return unchecked((ulong)value.GetValueAsInt64());
                case ConstantTypeCode.UInt64:
                    return value.GetValueAsUInt64();
                default:
                    throw new NotImplementedException();
            }
        }

        private void WriteEnumReferenceValue(MetadataTypeReference enumType, IMetadataConstantValue underlyingValue, string currentNamespace, bool canTargetType)
        {
            if (metadataReferenceResolver.TryGetEnumInfo(enumType, out var info))
            {
                var flagsToSearchFor = info.IsFlags ? ConvertEnumValueToUInt64(underlyingValue) : 0;
                if (flagsToSearchFor == 0)
                {
                    foreach (var field in info.SortedFields)
                    {
                        if (Equals(field.Value, underlyingValue))
                        {
                            Write(enumType, currentNamespace);
                            writer.Write('.');
                            writer.Write(field.Name);
                            return;
                        }
                    }
                }
                else
                {
                    var lastNegativeIndex = FindLastNegativeIndex(info);

                    // Logic from https://github.com/dotnet/coreclr/blob/c4d7429096edc5779f1cbd999c972c3bae236085/src/mscorlib/src/System/Enum.cs#L179-L195
                    // Since we don't sort by the unsigned value, the flags loop is split to search the negatives first.

                    var fieldIndexes = new List<int>();

                    for (var i = lastNegativeIndex; i >= 0; i--)
                    {
                        var fieldFlags = ConvertEnumValueToUInt64(info.SortedFields[i].Value);
                        if ((flagsToSearchFor & fieldFlags) != fieldFlags) continue;
                        flagsToSearchFor -= fieldFlags;
                        fieldIndexes.Add(i);
                        if (flagsToSearchFor == 0) break;
                    }
                    if (flagsToSearchFor != 0)
                    {
                        for (var i = info.SortedFields.Count - 1; i > lastNegativeIndex; i--)
                        {
                            var fieldFlags = ConvertEnumValueToUInt64(info.SortedFields[i].Value);
                            if ((flagsToSearchFor & fieldFlags) != fieldFlags) continue;
                            flagsToSearchFor -= fieldFlags;
                            fieldIndexes.Add(i);
                            if (flagsToSearchFor == 0) break;
                        }
                    }

                    if (flagsToSearchFor == 0)
                    {
                        for (var i = fieldIndexes.Count - 1; i >= 0; i--)
                        {
                            Write(enumType, currentNamespace);
                            writer.Write('.');
                            writer.Write(info.SortedFields[fieldIndexes[i]].Name);
                            if (i != 0) writer.Write(" | ");
                        }

                        return;
                    }
                }
            }

            if (canTargetType && IsDefault(underlyingValue))
            {
                WriteConstantPrimitive(underlyingValue);
            }
            else
            {
                writer.Write('(');
                Write(enumType, currentNamespace);
                writer.Write(')');

                var isNegative = IsNegativeInteger(underlyingValue);
                if (isNegative) writer.Write('(');
                WriteConstantPrimitive(underlyingValue);
                if (isNegative) writer.Write(')');
            }
        }

        private static bool IsNegativeInteger(IMetadataConstantValue value)
        {
            switch (value.TypeCode)
            {
                case ConstantTypeCode.SByte:
                    return value.GetValueAsSByte() < 0;
                case ConstantTypeCode.Int16:
                    return value.GetValueAsInt16() < 0;
                case ConstantTypeCode.Int32:
                    return value.GetValueAsInt32() < 0;
                case ConstantTypeCode.Int64:
                    return value.GetValueAsInt64() < 0;
                default:
                    return false;
            }
        }

        private static bool IsDefault(IMetadataConstantValue value)
        {
            switch (value.TypeCode)
            {
                case ConstantTypeCode.Boolean:
                    return value.GetValueAsBoolean() == default;
                case ConstantTypeCode.Char:
                    return value.GetValueAsChar() == default;
                case ConstantTypeCode.SByte:
                    return value.GetValueAsSByte() == default;
                case ConstantTypeCode.Byte:
                    return value.GetValueAsByte() == default;
                case ConstantTypeCode.Int16:
                    return value.GetValueAsInt16() == default;
                case ConstantTypeCode.UInt16:
                    return value.GetValueAsInt16() == default;
                case ConstantTypeCode.Int32:
                    return value.GetValueAsInt32() == default;
                case ConstantTypeCode.UInt32:
                    return value.GetValueAsUInt32() == default;
                case ConstantTypeCode.Int64:
                    return value.GetValueAsInt64() == default;
                case ConstantTypeCode.UInt64:
                    return value.GetValueAsUInt64() == default;
                case ConstantTypeCode.Single:
                    return value.GetValueAsSingle() == default;
                case ConstantTypeCode.Double:
                    return value.GetValueAsDouble() == default;
                case ConstantTypeCode.String:
                    return value.GetValueAsString() == default;
                default:
                    return false;
            }
        }

        private static int FindLastNegativeIndex(EnumInfo enumInfo)
        {
            switch (enumInfo.UnderlyingType)
            {
                case PrimitiveTypeCode.SByte:
                {
                    var maxIndex = enumInfo.SortedFields.Count - 1;
                    for (var i = 0; i <= maxIndex; i++)
                        if (enumInfo.SortedFields[i].Value.GetValueAsSByte() >= 0)
                            return i - 1;
                    return maxIndex;
                }
                case PrimitiveTypeCode.Int16:
                {
                    var maxIndex = enumInfo.SortedFields.Count - 1;
                    for (var i = 0; i <= maxIndex; i++)
                        if (enumInfo.SortedFields[i].Value.GetValueAsInt16() >= 0)
                            return i - 1;
                    return maxIndex;
                }
                case PrimitiveTypeCode.Int32:
                {
                    var maxIndex = enumInfo.SortedFields.Count - 1;
                    for (var i = 0; i <= maxIndex; i++)
                        if (enumInfo.SortedFields[i].Value.GetValueAsInt32() >= 0)
                            return i - 1;
                    return maxIndex;
                }
                case PrimitiveTypeCode.Int64:
                {
                    var maxIndex = enumInfo.SortedFields.Count - 1;
                    for (var i = 0; i <= maxIndex; i++)
                        if (enumInfo.SortedFields[i].Value.GetValueAsInt64() >= 0)
                            return i - 1;
                    return maxIndex;
                }
                default:
                    return -1;
            }
        }

        private void WriteAttributes(IReadOnlyList<IMetadataAttribute> attributes, string currentNamespace, bool newLines)
        {
            if (attributes.Count == 0) return;

            writer.Write('[');

            for (var i = 0; i < attributes.Count; i++)
            {
                if (i != 0)
                {
                    if (newLines)
                    {
                        writer.WriteLine(']');
                        writer.Write('[');
                    }
                    else
                    {
                        writer.Write(", ");
                    }
                }

                var attribute = attributes[i];
                Write(GetShortenedAttributeName(attribute.AttributeType), currentNamespace);

                var namedArgs = attribute.NamedArguments;
                var fixedArgs = attribute.FixedArguments;
                if (namedArgs.Count != 0 || fixedArgs.Count != 0)
                {
                    writer.Write('(');

                    var isFirst = true;

                    foreach (var arg in fixedArgs)
                    {
                        if (isFirst) isFirst = false; else writer.Write(", ");
                        WriteAttributeArgumentValue(arg, currentNamespace);
                    }

                    foreach (var arg in namedArgs)
                    {
                        if (isFirst) isFirst = false; else writer.Write(", ");
                        writer.Write(arg.Name);
                        writer.Write(" = ");
                        WriteAttributeArgumentValue(arg.Value, currentNamespace);
                    }

                    writer.Write(')');
                }
            }

            if (newLines)
                writer.WriteLine(']');
            else
                writer.Write("] ");
        }

        private void Write(MetadataTypeReference typeReference, string currentNamespace)
        {
            Write(typeReference.Accept(new SignatureTypeProvider(currentNamespace)));
        }

        private void Write(ImmutableNode<string> parts)
        {
            for (; parts != null; parts = parts.Next)
            {
                Write(parts.Prev);
                writer.Write(parts.Value);
            }
        }

        private static string TrimGenericArity(string typeName)
        {
            var index = typeName.LastIndexOf('`');
            return index == -1 ? typeName : typeName.Substring(0, index);
        }
    }
}
