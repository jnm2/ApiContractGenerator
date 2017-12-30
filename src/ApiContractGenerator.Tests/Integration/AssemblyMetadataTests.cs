using ApiContractGenerator.Tests.Utils;
using NUnit.Framework;

namespace ApiContractGenerator.Tests.Integration
{
    public sealed class AssemblyMetadataTests : IntegrationTests
    {
        [Test]
        public static void Without_public_key()
        {
            Assert.That("public struct Test { }", HasContract(
                "// Name:       " + AssemblyUtils.EmittedAssemblyName,
                "// Public key: (none)",
                "",
                "public struct Test",
                "{",
                "}").IncludeAssemblyMetadata);
        }
    }
}
