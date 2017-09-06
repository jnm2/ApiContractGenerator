using ApiContractGenerator.Source;

namespace ApiContractGenerator
{
    public interface IMetadataWriter
    {
        void Write(IMetadataSource metadataSource);
    }
}
