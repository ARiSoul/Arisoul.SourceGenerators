<!-- Improved compatibility of back to top link: See: https://github.com/othneildrew/Best-README-Template/pull/73 -->
<a name="readme-top"></a>
<!--
*** This template was copyied from <a href="https://github.com/othneildrew/Best-README-Template/tree/master">othneildrew/Best-README-Template</a>
*** Go there and give him a star :)
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

***

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/ARiSoul/Arisoul.SourceGenerators">
    <img src="img/Arisoul_Triangle_Fill_128x128.png" alt="Logo" width="80" height="80">
  </a>

<h3 align="center">Arisoul Source Generators (under construction)</h3>

  <p align="center">
    This is a simple source generator for .Net C#, created with personal purposes, but it may grow as needed. For now, the only available generators are for DTO generation, and extensions that facilitate mappings.
    <br />
    <br />
    <a href="https://github.com/ARiSoul/Arisoul.SourceGenerators/issues">Report Bug</a>
    ·
    <a href="https://github.com/ARiSoul/Arisoul.SourceGenerators/issues">Request Feature</a>
  </p>

[![NuGet](https://img.shields.io/nuget/v/Arisoul.SourceGenerators)](https://www.nuget.org/packages/Arisoul.SourceGenerators/)
</div>
</br>

<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
        <li><a href="#tests-coverage">Tests Coverage</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
# About The Project

I found myself many times creating **POCO** classes, or **Entity Models**, and then rewriting by hand new classes for the corresponding DTOs. I think that this a very boring task, so I decided to give the <a href="https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview">**.Net Source Generators**</a> a try, and do some of that job for me. I then understood that using <a href="https://automapper.org/">**AutoMapper**</a>, <a href="https://github.com/MapsterMapper/Mapster">**Mapster**</a>, etc. it's all good and all, but why don't to try to genereate some extensions helpers for simple scenarios?

In this first version, only single entities are considered in mapping extensions. The generated DTO class will have the name of the source class followed by Dto, and the extensions class will have the suffix Extensions. Ex.: Class 'Person', will generate 'PersonDto' and 'PersonExtensions'.

The marker attribute to trigger the generation is called **DtoPropertyAttribute** and it's provided in the package through the **Arisoul.SourceGenerators.Attributes** dll that comes together. 

I intend to grow the package as needed to allow other generations, and make it better.

Of course, this is a very very simple tool, so don't be to hard on me 😃 . Any suggestions or ideas are welcome, such as issues.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Built With

* [![csharp][c#]][csharp-url]
* [![React][netstandard20]][netstandard-url]
* [![.Net 7][.Net 7]][net7-url]
* [![xunit][xunit]][xunit-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Tests Coverage
- Arisoul SourceGenerator is unit tested with <a href="https://xunit.net/">**xUnit**</a> and <a href="https://github.com/VerifyTests/Verify">**Verify**</a>
- 11 tests at the moment with a coverage more then 95% as you can see below

<a href="https://github.com/ARiSoul/Arisoul.SourceGenerators">
    <img src="img/test_coverage.png" alt="Tests Coverage"/>
</a>

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- GETTING STARTED -->
# Getting Started

## Prerequisites

- The generator is made with netstandard2.0, so it should be compatible with all .Net Frameworks since 4.6 :wink:

## Installation

Choose how to install the package:

* .NET CLI
  ```sh
  dotnet add package Arisoul.SourceGenerators --version 1.0.0
  ```

* Package Manager
  ```sh
  NuGet\Install-Package Arisoul.SourceGenerators -Version 1.0.0
  ```

* Directly in your .csproj
  ```sh
  <PackageReference Include="Arisoul.SourceGenerators" Version="1.0.0" />
  ```

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- USAGE EXAMPLES -->
## Usage

1. Create a POCO class with the properties you want
2. Add ``` using Arisoul.SourceGenerators.DataTransferObjects; ```
3. Decorate the properties that you want to be included in the Dto class with the attribute ``` [DtoProperty] ```
4. That's it 👍 :smiley:

- Now, you can use a class named **YourClassDto** with the corresponding properties
- The other generated class creates extensions for the DTO and the POCO class, allowing to call ```dtoInstance = pocoInstance.ToDto()```, or ```pocoInstance.FromDto(dtoInstance)```, and the same reverse corresponding methods.
- If you want to provide your custom names for the generated DTO class properties, you can use the overload of the DtoPropertyAttribute, like so: ```[DtoProperty("PreferredName")]``` or ```[DtoProperty(Name = "OtherName")] ```

### But... where are the generated classes?

- #### If you're targeting .NetStandard or other above (.Net Core, .Net 5, 6, 7, etc...)
  - Below your project in the Solution Explorer, expand Dependencies
  - Expand Analyzers > Arisoul.SourceGenerators > Arisoul.SourceGenerators.DataTransferObjects.DtoGenerator
  - There they are 👍

## Simple examples
</br>

- ### The POCO class:
```
using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    [DtoProperty("Name")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }

    [DtoProperty]
    public DateTime Date { get; set; }
}
```

- ### The generated DTO:
``` 
#nullable enable
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

using System;

namespace DtoGenerator
{
    public class PersonDto
    {
        public string Name { get; set; }
        public string LastName { get; set; }
        public DateTime Date { get; set; }
    }
}
```
- ### The generated extensions:
```
#nullable enable
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

using System;

namespace DtoGenerator
{
    public static class PersonExtensions
    {
        public static PersonDto ToDto(this Person poco)
        {
            PersonDto dto = new PersonDto();

            dto.Name = poco.FirstName;
            dto.LastName = poco.LastName;
            dto.Date = poco.Date;

            return dto;
        }

        public static void FromDto(this Person poco, PersonDto dto)
        {
            poco.FirstName = dto.Name;
            poco.LastName = dto.LastName;
            poco.Date = dto.Date;
        }

        public static Person ToPoco(this PersonDto dto)
        {
            Person poco = new Person();

            poco.FirstName = dto.Name;
            poco.LastName = dto.LastName;
            poco.Date = dto.Date;

            return poco;
        }

        public static void FromPoco(this PersonDto dto, Person poco)
        {
            dto.Name = poco.FirstName;
            dto.LastName = poco.LastName;
            dto.Date = poco.Date;
        }
    }
}
```

- ### Usage Demo
```
    public void Demo()
    {
        // Create an instance of the POCO class
        Person pocoPerson = new Person();

        // Map to a DTO (extension)
        PersonDto dtoPerson = pocoPerson.ToDto();

        // Reverse mapping using the POCO extension
        pocoPerson.FromDto(dtoPerson);

        // Other reverse mapping using the DTO extension
        pocoPerson = dtoPerson.ToPoco();
        
        // Fill the DTO with the POCO (another extension way)
        dtoPerson.FromPoco(pocoPerson);
    }
```

_If you need more examples, please, let me now. And remember, this has a lot to improve._

<p align="right">(<a href="#readme-top">back to top</a>)</p>

# Release Notes
- [v1.0.0 (29/03/2023)](./changelog.md#v1.0.0 (29/03/2023))

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- ROADMAP -->
# Roadmap

- [ ] Allow to set generated classes names
- [ ] Allow to set classes to generate
- [ ] Allow to set namespace in each generated class
- [ ] Mapping collections
- [ ] Allow generated files persisting in disk
    - Actually, this option can be achived by adding this to your project:
```
    <PropertyGroup>
        <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
        <CompilerGeneratedFilesOutputPath>DesiredOutpuPath</CompilerGeneratedFilesOutputPath>
    </PropertyGroup>
```


    The downside is that all files will be persisted and, in the same path. So this feature aims to allow this configuration

- [ ] Add a type converter option
- [ ] More to come

See the [open issues](https://github.com/github_username/repo_name/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
# Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a ⭐ ! Thanks again ❤️ !

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature-your-feature`)
3. Commit your Changes (`git commit -m 'Amazing commit'`)
4. Push to the Branch (`git push origin feature-your-feature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
# License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
# Contact

Ricardo Sousa - rjr.sousa0915@gmail.com

Project Link: [https://github.com/ARiSoul/Arisoul.SourceGenerators](https://github.com/ARiSoul/Arisoul.SourceGenerators)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### This README template was copied from <a href="https://github.com/othneildrew/Best-README-Template/tree/master">othneildrew/Best-README-Template</a>
- Go there and give him a star ⭐



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/Arisoul/Arisoul.SourceGenerators.svg?style=for-the-badge
[contributors-url]: https://github.com/ARiSoul/Arisoul.SourceGenerators/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/Arisoul/Arisoul.SourceGenerators.svg?style=for-the-badge
[forks-url]: https://github.com/Arisoul/Arisoul.SourceGenerators/network/members
[stars-shield]: https://img.shields.io/github/stars/Arisoul/Arisoul.SourceGenerators.svg?style=for-the-badge
[stars-url]: https://github.com/Arisoul/Arisoul.SourceGenerators/stargazers
[issues-shield]: https://img.shields.io/github/issues/Arisoul/Arisoul.SourceGenerators.svg?style=for-the-badge
[issues-url]: https://github.com/Arisoul/Arisoul.SourceGenerators/issues
[license-shield]: https://img.shields.io/github/license/Arisoul/Arisoul.SourceGenerators.svg?style=for-the-badge
[license-url]: https://github.com/Arisoul/Arisoul.SourceGenerators/Arisoul/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/rsousa1983
[product-screenshot]: images/screenshot.png
[c#]: https://img.shields.io/badge/c_sharp-white?style=for-the-badge&logo=csharp&logoColor=blueviolet
[csharp-url]: https://dotnet.microsoft.com/en-us/languages/csharp
[netstandard20]: https://img.shields.io/badge/standard_2.0-blue?style=for-the-badge&logo=dotnet&logoColor=white
[netstandard-url]: https://dotnet.microsoft.com/en-us/platform/dotnet-standard
[.Net 7]: https://img.shields.io/badge/7-blueviolet?style=for-the-badge&logo=dotnet&logoColor=white
[net7-url]: https://dotnet.microsoft.com/en-us/download/dotnet/7.0
[xunit]: https://img.shields.io/badge/xUnit-Tests-green
[xunit-url]: https://xunit.net/