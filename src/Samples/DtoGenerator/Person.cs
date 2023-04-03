﻿using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = "DomainPerson")]
[DtoExtensionsClassGeneration]
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