﻿//HintName: CustomPersonExtensions.g.cs
#nullable enable
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

using System;

namespace DtoGenerator
{
    public static class CustomPersonExtensions
    {
        public static DtoGenerator.PersonDto ToDto(this Person poco)
        {
            DtoGenerator.PersonDto dto = new DtoGenerator.PersonDto();

            dto.TestAgain = poco.FirstName;
            dto.LastName = poco.LastName;

            return dto;
        }

        public static void FromDto(this Person poco, DtoGenerator.PersonDto dto)
        {
            poco.FirstName = dto.TestAgain;
            poco.LastName = dto.LastName;
        }

        public static Person ToPoco(this DtoGenerator.PersonDto dto)
        {
            Person poco = new Person();

            poco.FirstName = dto.TestAgain;
            poco.LastName = dto.LastName;

            return poco;
        }

        public static void FromPoco(this DtoGenerator.PersonDto dto, Person poco)
        {
            dto.TestAgain = poco.FirstName;
            dto.LastName = poco.LastName;
        }
    }
}