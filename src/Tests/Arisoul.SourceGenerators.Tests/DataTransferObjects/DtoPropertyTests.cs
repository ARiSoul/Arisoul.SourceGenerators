using Arisoul.SourceGenerators.Tests;

namespace Arisoul.SourceGenerators.DataTransferObjects.Tests;

[UsesVerify]
public class DtoGeneratorTests : BaseDtoTestClass
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task DtoPropertyWithNamedArgumentsShouldVerify()
    {
        var code = @"
using Arisoul.SourceGenerators.DataTransferObjects;

namespace GeneratorDebugConsumer
{
    public class Person
    {
        public Guid Id { get; set; }
        
        [DtoProperty(Name = ""TheName"")]
        public string Name { get; set; }
    
        [DtoProperty]
        public string Description { get; set; }
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
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
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task DtoPropertyCannotBeReadonly()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task DtoPropertyAbstractClassShouldNotBeGenerated()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public abstract class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task SourceClassWithMethodShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }

    public void DoSomething()
    {
        // does nothing
    }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task SourceClassWithoutDtoPropertiesShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }
}
