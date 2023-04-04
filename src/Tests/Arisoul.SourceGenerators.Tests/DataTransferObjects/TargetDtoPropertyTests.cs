using Arisoul.SourceGenerators.Tests;

namespace Arisoul.SourceGenerators.DataTransferObjects.Tests;

[UsesVerify]
public class TargetDtoPropertyTests : BaseDtoTestClass
{
    [Fact]
    public Task SimpleTargetDtoPropertyWithNoNameShouldVerify()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    public class Person
    {
        [TargetDtoProperty<string>]
        public Guid Id { get; set; }

        [DtoProperty]        
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }
}
