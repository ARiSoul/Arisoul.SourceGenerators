<a id="changelog-top"></a>

# Changelog

Here you can find the release notes of each version of the project from more recent to older ones. 

## Table of contents
- [v1.0.2 (07/04/2023)](#v1.0.2)
- [v1.0.1 (03/04/2023)](#v1.0.1)
- [v1.0.0 (29/03/2023)](#v1.0.0)


# v1.0.2 <a id="v1.0.2"></a>

**Release date** - 07/04/2023

## Features

- #5 Mapping collections
- #6 Allow generated files persisting in disk
- #7 Add a type converter option

### Details

- Made generated classes partial so they can be extended if required
- Made generated dto properties virtual, so they can be overrided if needed
- Generated Dto class property types will now be in the fully qualified format, to avoid naming collisions
- Added a new attribute ```DtoChildPropertyAttribute``` 
    - When applied to a property, it allows to define the type of the child property to generate in the Dto class. Suppose you have an entity called **Person** and a **PersonDto**. Now suppose that in your **Person**, you have a child property that references another **Person**, for example **RelatedPerson**. If you use the simple ```DtoPropertyAttribute```, the **PersonDto** will be generated also with a child property **RelatedPerson** of the same type from **Person**. Normally, we want that those child properties are also Dtos, so using the new attribute, you can say that the child property **RelatedPerson** in the Dto will be of type **PersonDto** also. Just decorate the **RelatedPerson** in your **Person** class with the attribute like this: ```[DtoChildProperty<PersonDto>]```, and the **RelatedPerson** in the **PersonDto**, will be of type **PersonDto**. See the following example:

```
public class Person
{
    public int Id { get; set; }

    [DtoProperty]
    public string Name { get; set; }

    // other properties...

    [DtoChildProperty<PersonDto>]
    public Person RelatedPerson { get; set; }
}
```
- Will generate:
```
public partial class PersonDto
{
    public virtual string Name { get; set; }

    // other properties...

    public virtual PersonDto RelatedPerson { get; set; }
}
```

## Bugs

- Fixed a bug while generating extensions class where the class name is the same as the source class, but in different namespaces, that would cause a cast conversion error while trying to "map" one property to other

## Test coverage
- All changes were covered by unit tests
- Coverage: **94.9%**

- [Release](https://github.com/ARiSoul/Arisoul.SourceGenerators/releases/tag/v1.0.2)

<p align="right">(<a href="#changelog-top">back to top</a>)</p>

# v1.0.1 <a id="v1.0.1"></a>

**Release date** - 03/04/2023

## Features
- #2 Allow to set generated classes names by @ARiSoul in #9
- #3 Allow to set classes to generate by @ARiSoul in #10
- #4 Allow to set namespace in each generated class

## Bugs
- Fixed bug #8 Abstract classes should no be generated

## Test coverage
- All changes were covered by unit tests
- Coverage: **94.9%**

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

# v1.0.0 <a id="v1.0.0"></a>

**Release date** - 29/03/2023

- This was the first release
- Added a single attribute to allow classes generation: `DtoPropertyAttribute`
- Add `using Arisoul.SourceGenerators.DataTransferObjects;` to the top of your class
- Decorate the properties you want to be considered in generation with the attribute `[DtoProperty]`
- Or to costumize the name of the property generated use `[DtoProperty("CustomName")]` or `[DtoProperty(Name = "CustomName")]`

- [Release](https://github.com/ARiSoul/Arisoul.SourceGenerators/releases/tag/v1.0.0)

<p align="right">(<a href="#changelog-top">back to top</a>)</p>
