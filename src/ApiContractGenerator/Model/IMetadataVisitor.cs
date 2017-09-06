namespace ApiContractGenerator.Model
{
    public interface IMetadataVisitor
    {
        void Visit(IMetadataNamespace metadataNamespace);
        void Visit(IMetadataClass metadataClass);
        void Visit(IMetadataStruct metadataStruct);
        void Visit(IMetadataInterface metadataInterface);
        void Visit(IMetadataEnum metadataEnum);
        void Visit(IMetadataDelegate metadataDelegate);
    }
}
