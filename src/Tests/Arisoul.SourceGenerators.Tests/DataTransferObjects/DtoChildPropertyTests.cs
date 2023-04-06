using Arisoul.SourceGenerators.Tests;

namespace Arisoul.SourceGenerators.DataTransferObjects.Tests;

[UsesVerify]
public class DtoChildPropertyTests : BaseDtoTestClass
{
    [Fact]
    public Task ExtensionClassGenerationDefinedButNoBehaviorSetShouldDiagnoseNotSupporterError()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    [DtoExtensionsClassGeneration(Name = ""PersonCustom"")]
    public class Person
    {
        [DtoChildProperty<DomainPerson>]
        public Person Child { get; set; }
        
        public Guid Id { get; set; }

        [DtoProperty]        
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }

    public class DomainPerson
    {
        public int Id { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ExtensionClassGenerationNotDefinedShouldDiagnoseNotSupporterError()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    public class Person
    {
        [DtoChildProperty<DomainPerson>]
        public Person Child { get; set; }
        
        public Guid Id { get; set; }

        [DtoProperty]        
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }

    public class DomainPerson
    {
        public int Id { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ExtensionClassGenerationDefinedButBehaviorNotNoGenerationShouldDiagnoseNotSupporterError()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    [DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.OnlyFromMethods)]
    public class Person
    {
        [DtoChildProperty<DomainPerson>]
        public Person Child { get; set; }
        
        public Guid Id { get; set; }

        [DtoProperty]        
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }

    public class DomainPerson
    {
        public int Id { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ExtensionClassGenerationNoGenerationShouldGenerateOnlyDto()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    [DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.NoGeneration)]
    public class Person
    {
        [DtoChildProperty<DomainPerson>]
        public Person Child { get; set; }
        
        public Guid Id { get; set; }

        [DtoProperty]        
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }

    public class DomainPerson
    {
        public int Id { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }
}
