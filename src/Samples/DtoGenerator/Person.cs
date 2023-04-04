using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = "DomainPerson")]
[DtoExtensionsClassGeneration(GenerationBehavior = GenerationBehavior.NoGeneration)]
public class Person
{
    public int Id { get; set; }

    [DtoProperty("Name")]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }

    [DtoProperty]
    public DateTime Date { get; set; }

    [TargetDtoProperty<DomainPerson>]
    public Person ChildPerson  { get; set; }
}