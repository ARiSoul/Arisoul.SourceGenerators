using Arisoul.SourceGenerators.DataTransferObjects;

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
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
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

        return TestHelper.Verify<DtoGenerator>(code);
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

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task DtoClassGenerationWithCustomDtoClassNameDefinedShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task ExtensionsClassGenerationWithCustomClassNameDefinedShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoExtensionsClassGeneration(Name = ""CustomPersonExtensions"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task DtoClassGenerationWithCustomDtoAndExtensionsClassNameDefinedShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"")]
[DtoExtensionsClassGeneration(Name = ""CustomPersonExtensions"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task DtoClassGenerationWithCustomDtoNamespaceDefinedShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
    }

    [Fact]
    public Task DtoClassGenerationWithCustomNamespacesDefinedShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
[DtoExtensionsClassGeneration(Namespace = ""ExtensionsNamespace"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code);
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

        return TestHelper.Verify<DtoGenerator>(code);
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

        return TestHelper.Verify<DtoGenerator>(code);
    }
}
