using ApiContractGenerator.Model;

namespace ApiContractGenerator.Source
{
    public interface IMetadataSource
    {
        void Accept(IMetadataVisitor visitor);
    }
}
