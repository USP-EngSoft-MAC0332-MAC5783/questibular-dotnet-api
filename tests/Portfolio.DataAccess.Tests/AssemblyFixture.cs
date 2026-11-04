[assembly: AssemblyFixture(typeof(Portfolio.DataAccess.Tests.AssemblyFixture))]

namespace Portfolio.DataAccess.Tests;

public sealed class AssemblyFixture
{
    public AssemblyFixture() => MappingConfig.RegisterMappings();
}