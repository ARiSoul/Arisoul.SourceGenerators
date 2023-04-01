using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration("DomainPerson")]
[DtoExtensionsClassGeneration("PersonMap")]
public abstract class Person
{
    public int Id { get; set; }

    [DtoProperty("Name")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }

    [DtoProperty]
    public DateTime Date { get; set; }
}