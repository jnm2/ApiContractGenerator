using System.Reflection.Metadata;
using ApiContractGenerator.Model;
using ApiContractGenerator.Model.TypeReferences;

namespace ApiContractGenerator.Source
{
    public sealed partial class MetadataReaderSource
    {
        private sealed class ReaderEvent : IMetadataEvent
        {
            private readonly MetadataReader reader;
            private readonly EventDefinition definition;
            private readonly GenericContext genericContext;

            public ReaderEvent(MetadataReader reader, EventDefinition definition, GenericContext genericContext, IMetadataMethod addAccessor, IMetadataMethod removeAccessor, IMetadataMethod raiseAccessor)
            {
                this.reader = reader;
                this.definition = definition;
                this.genericContext = genericContext;
                AddAccessor = addAccessor;
                RemoveAccessor = removeAccessor;
                RaiseAccessor = raiseAccessor;
            }

            private string name;
            public string Name => name ?? (name = reader.GetString(definition.Name));

            public MetadataTypeReference HandlerType => GetTypeFromEntityHandle(reader, genericContext, definition.Type);

            public IMetadataMethod AddAccessor { get; }
            public IMetadataMethod RemoveAccessor { get; }
            public IMetadataMethod RaiseAccessor { get; }
        }
    }
}
