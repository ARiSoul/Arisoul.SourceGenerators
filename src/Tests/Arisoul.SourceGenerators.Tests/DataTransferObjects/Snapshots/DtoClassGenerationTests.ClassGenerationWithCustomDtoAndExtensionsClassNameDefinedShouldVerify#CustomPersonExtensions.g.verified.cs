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
    public static partial class CustomPersonExtensions
    {
        public static DtoGenerator.PersonCustom ToDto(this Person poco)
        {
            DtoGenerator.PersonCustom dto = new DtoGenerator.PersonCustom();

            dto.TestAgain = poco.FirstName;
            dto.LastName = poco.LastName;

            return dto;
        }

        public static void FromDto(this Person poco, DtoGenerator.PersonCustom dto)
        {
            poco.FirstName = dto.TestAgain;
            poco.LastName = dto.LastName;
        }

        public static Person ToPoco(this DtoGenerator.PersonCustom dto)
        {
            Person poco = new Person();

            poco.FirstName = dto.TestAgain;
            poco.LastName = dto.LastName;

            return poco;
        }

        public static void FromPoco(this DtoGenerator.PersonCustom dto, Person poco)
        {
            dto.TestAgain = poco.FirstName;
            dto.LastName = poco.LastName;
        }
    }
}