using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    [DtoProperty(Name = "Test")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}