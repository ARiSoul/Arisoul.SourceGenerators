using Arisoul.SourceGenerators.DataTransferObjects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Text;
using Xunit.Sdk;

namespace Arisoul.SourceGenerators.Tests;

[UsesVerify]
public class DtoGeneratorTests
{
    [Fact]
    public Task PersonDtoGenerator()
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

        return TestHelper.Verify<IncrementalDtoGenerator>(code);
    }

    [Fact]
    public Task PersonDtoGeneratorWithDtoName()
    {
        // TODO: this test is not correct
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

        return TestHelper.Verify<IncrementalDtoGenerator>(code);
    }
}
