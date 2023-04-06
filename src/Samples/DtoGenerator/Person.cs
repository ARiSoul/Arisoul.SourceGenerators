using Arisoul.SourceGenerators.DataTransferObjects;
using System.Collections.ObjectModel;

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

    [DtoProperty]
    public ICollection<Person> PeopleICollection { get; set; }

    [DtoProperty]
    public Collection<Person> PeopleCollection { get; set; }

    [DtoProperty]
    public IList<Person> PeopleIList { get; set; }

    [DtoProperty]
    public List<Person> PeopleList { get; set; }

    [DtoProperty]
    public Dictionary<string, Person> PeopleDictionary { get; set; }

    [DtoProperty]
    public IDictionary<string, Person> PeopleIDictionary { get; set; }

    [DtoProperty]
    public PersonEnum PersonEnum { get; set; }
}

public enum PersonEnum
{
    Value1,
    Value2
}