namespace Arisoul.SourceGenerators.DataTransferObjects.Tests;

public class DtoAttributesTests
{
    [Fact]
    public void TestDtoPropertyAttributeParametlessConstructor()
    {
        // Arrange
        DtoPropertyAttribute dto = new()
        {
            Name = "Name"
        };

        // Act
        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void TestDtoPropertyAttributeConstructorArgs()
    {
        // Arrange
        DtoPropertyAttribute dto = new("Name");

        // Act
        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void TestDtoChildPropertyAttributeParametlessConstructor()
    {
        // Arrange
        DtoChildPropertyAttribute<DtoChildProperty> dto = new()
        {
            Name = "Name"
        };

        // Act
        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.TargetType);
    }

    [Fact]
    public void TestDtoChildPropertyAttributeConstructorArgs()
    {
        // Arrange
        DtoChildPropertyAttribute<DtoChildProperty> dto = new("Name");
        
        // Act
        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.TargetType);
    }


    [Fact]
    public void DtoExtensionsClassGenerationShouldInitializeWithGenerationBehaviorFull()
    {
        // Arrange
        DtoExtensionsClassGenerationAttribute classGenerationAttribute = new()
        {
            // Act
            Name = "Name",
            Namespace = "namespace"
        };

        // Assert
        Assert.NotNull(classGenerationAttribute);
        Assert.Equal(GenerationBehavior.Full, classGenerationAttribute.GenerationBehavior);
    }

    public class DtoChildProperty
    {
        public string Name { get; set; }
    }
}
