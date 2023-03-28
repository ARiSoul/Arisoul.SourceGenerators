using Arisoul.SourceGenerators.DataTransferObjects;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Arisoul.SourceGenerators.Tests;

[UsesVerify]
public class DtoGeneratorTests
{
    [Fact]
    public Task DtoPropertyWithNoNameShouldVerify()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    public class Person
    {
        public Guid Id { get; set; }
        
        [DtoProperty]
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task DtoPropertyWithNameShouldVerify()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    public class Person
    {
        public Guid Id { get; set; }
        
        [DtoProperty(""TheName"")]
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task DtoPropertyClassWithFileScopedNamespaceShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }
}
