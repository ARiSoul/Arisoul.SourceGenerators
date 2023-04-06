using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

[DtoClassGeneration(Name = "Person", Namespace = "DtoGenerator.Domain")]
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

    [DtoChildProperty<Domain.Person>]
    public Person ChildPerson  { get; set; }

    [DtoChildProperty<List<Domain.Person>>]
    public ICollection<Person> People { get; set; }

    [DtoProperty]
    public PersonEnum PersonEnum { get; set; }
}

public enum PersonEnum
{
    Value1,
    Value2
}