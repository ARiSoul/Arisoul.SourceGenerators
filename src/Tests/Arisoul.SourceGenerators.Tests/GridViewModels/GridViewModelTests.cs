using Arisoul.SourceGenerators.Tests;
using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Arisoul.SourceGenerators.GridViewModels.Tests;

[UsesVerify]
public class GridViewModelTests
{
    internal const string SnapshotsDirectory = "DataTransferObjects\\Snapshots";

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

    public class PersonGvm
    {
        public GridViewModel<Person> Gvm { get; set; }
    }
}";

        return TestHelper.Verify<GridViewModelGenerator>(code, SnapshotsDirectory);
    }
}
