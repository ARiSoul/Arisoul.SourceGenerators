using Arisoul.SourceGenerators.Tests;
using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Arisoul.SourceGenerators.DataTransferObjects.Tests;

[UsesVerify]
public class DtoPropertyTests : BaseDtoTestClass
{
    [Fact]
    public Task WithNoNameShouldVerify()
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
    public Task WithNameShouldVerify()
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
    public Task WithNamedArgumentsShouldVerify()
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
    public Task SourceClassWithFileScopedNamespaceShouldVerify()
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
    public Task CannotBeReadonly()
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
    public Task ReadonlyPropertyWithoutAttributeMustVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ReadonlyPropertyWithOtherAttributeMustVerify()
    {
        var code = @"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    [Required]
    public int Id { get; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}";

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task AbstractClassShouldNotBeGenerated()
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

    [Fact]
    public Task ICollectionDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode(nameof(ICollection));

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task IListDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode(nameof(IList));

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task IEnumerableDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode(nameof(IEnumerable));

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task CollectionDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode(nameof(Collection));

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task ListDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode("List");

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task EnumerableDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode(nameof(Enumerable));

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    [Fact]
    public Task HashSetDtoPropertyShouldVerify()
    {
        string code = GetCollectionPropertyCode("HashSet");

        return TestHelper.Verify<DtoGenerator>(code, SnapshotsDirectory);
    }

    private static string GetCollectionPropertyCode(string collection) => $@"using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{{
    [DtoProperty]
    public {collection}<Person> People {{ get; set; }}

    public int Id {{ get; set; }}

    public string FirstName {{ get; set; }}

    public string LastName {{ get; set; }}
}}";
}
