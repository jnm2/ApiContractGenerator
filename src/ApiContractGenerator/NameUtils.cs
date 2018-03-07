namespace ApiContractGenerator
{
    internal static class NameUtils
    {
        public static (string name, int arity) ParseGenericArity(string typeName)
        {
            var index = typeName.LastIndexOf('`');
            return index == -1 ? (typeName, 0) : (typeName.Substring(0, index), int.Parse(typeName.Substring(index + 1)));
        }
    }
}
