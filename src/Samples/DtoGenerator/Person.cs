using Arisoul.SourceGenerators.DataTransferObjects;

namespace DtoGenerator;

public class Person
{
    public int Id { get; set; }

    [DtoProperty]
    public string FirstName { get; set; }

    [DtoProperty]
    public string LastName { get; set; }
}

//public class PersonDto
//{
//    public string FirstName { get; set; }
//    public string LastName { get; set; }
//}

//public static class PersonExtensions
//{
//    public static PersonDto ToDto(this Person poco)
//    {
//        PersonDto dto = new PersonDto();

//        dto.FirstName = poco.FirstName;
//        dto.LastName = poco.LastName;

//        return dto;
//    }

//    public static void FromDto(this Person poco, PersonDto dto)
//    {
//        poco.FirstName = dto.FirstName;
//        poco.LastName = dto.LastName;
//    }

//    public static Person ToPoco(this PersonDto dto)
//    {
//        Person poco = new Person();

//        poco.FirstName = dto.FirstName;
//        poco.LastName = dto.LastName;

//        return poco;
//    }

//    public static void FromPoco(this PersonDto dto, Person poco)
//    {
//        dto.FirstName = poco.FirstName;
//        dto.LastName = poco.LastName;
//    }
//}