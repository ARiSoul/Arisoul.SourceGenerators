using Arisoul.SourceGenerators.Tests;

namespace Arisoul.SourceGenerators.DataTransferObjects.Tests;

[UsesVerify]
public class DtoClassGenerationTests : BaseDtoTestClass
{
    [Fact]
    public Task DtoClassGenerationWithCustomClassNameDefinedShouldVerify()
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ClassGenerationWithCustomDtoAndExtensionsClassNameDefinedShouldVerify()
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task DtoClassGenerationWithCustomNamespaceDefinedShouldVerify()
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ExtensionsClassGenerationWithCustomNamespaceDefinedShouldVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoExtensionsClassGeneration(Namespace = ""ExtensionsNamespace"")]
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
    public Task ClassGenerationWithBothCustomNamespacesDefinedShouldVerify()
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

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ExtensionsClassShouldNotGenerated()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.NoGeneration)]
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
    public Task ExtensionsClassShouldGenerateOnlyPocoExtensions()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.OnlyPoco)]
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
    public Task ExtensionsClassShouldGenerateOnlyDtoExtensions()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.OnlyDto)]
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
    public Task ExtensionsClassShouldGenerateOnlyToMethods()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.OnlyToMethods)]
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
    public Task ExtensionsClassShouldGenerateOnlyFromMethods()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"", Namespace = ""MyNamespace"")]
[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.OnlyFromMethods)]
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
}
