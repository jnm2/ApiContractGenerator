namespace ApiContractGenerator.Model.TypeReferences
{
    public abstract class TopLevelOrNestedTypeReference : MetadataTypeReference
    {
        public string Name { get; }

        protected TopLevelOrNestedTypeReference(string name)
        {
            Name = name;
        }
    }
}
