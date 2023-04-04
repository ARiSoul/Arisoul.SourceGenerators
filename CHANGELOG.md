<a name="changelog-top"></a>

# Changelog

Here you can find the release notes of each version of the project from more recent to older ones. 

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Version table</summary>
  <ul>
    <li><a href="#v1.0.1 (03/04/2023)">v1.0.1 (03/04/2023)</a></li>
    <li><a href="#v1.0.0 (29/03/2023)">v1.0.0 (29/03/2023)</a></li>
  </ul>
</details>

# v1.0.1 (03/04/2023)

## Features
- #2 Allow to set generated classes names by @ARiSoul in #9
- #3 Allow to set classes to generate by @ARiSoul in #10
- #4 Allow to set namespace in each generated class

## Bugs
- Fixed bug #8 Abstract classes should no be generated
- All changes were covered by unit tests

### Details
Is now possible to use the [DtoClassGenerationAttribute] and [DtoExtensionsClassGenerationAttribute] at class level, to set the desired class name to generate, as also the namespace of each one.
In DtoExtensionsClassGenerationAttribute there is another property to customize the Extensions class generation behavior, allowing not the generate the class at all, or other behaviors. Check #3 (comment) to more details about this property.

### Examples

- Customize the Dto class name to generate (default is the original class name sufixed with Dto. Ex.: Person will be PersonDto)
```
using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = ""PersonCustom"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}

```

- Customize the Extensions class name to generate (default is the original class name sufixed with Extensions. Ex.: Person will be PersonExtensions)
```
using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoExtensionsClassGeneration(Name = ""CustomPersonExtensions"")]
public class Person
{
    public int Id { get; set; }

    [DtoProperty(""TestAgain"")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}
```

- Customize the Dto class name and namespace, and in the extensions class only the namespace to generate
```
using Arisoul.SourceGenerators.DataTransferObjects;

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
}
```

- Customize the Dto class name and namespace, and set the extensions class to not be generated
```
using Arisoul.SourceGenerators.DataTransferObjects;

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
}
```

- [Release](https://github.com/ARiSoul/Arisoul.SourceGenerators/releases/tag/v1.0.1)

<p align="right">(<a href="#changelog-top">back to top</a>)</p>

# v1.0.0 (29/03/2023)

- This was the first release
- Added a single attribute to allow classes generation: `DtoPropertyAttribute`
- Add `using Arisoul.SourceGenerators.DataTransferObjects;` to the top of your class
- Decorate the properties you want to be considered in generation with the attribute `[DtoProperty]`
- Or to costumize the name of the property generated use `[DtoProperty("CustomName")]` or `[DtoProperty(Name = "CustomName")]`

- [Release](https://github.com/ARiSoul/Arisoul.SourceGenerators/releases/tag/v1.0.0)

<p align="right">(<a href="#changelog-top">back to top</a>)</p>
