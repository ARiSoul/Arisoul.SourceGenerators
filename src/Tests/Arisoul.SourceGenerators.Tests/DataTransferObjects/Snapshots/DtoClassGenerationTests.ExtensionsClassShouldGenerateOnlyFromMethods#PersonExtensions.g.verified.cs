﻿//HintName: PersonExtensions.g.cs
#nullable enable
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

using System;
using MyNamespace;

namespace DtoGenerator
{
    public static partial class PersonExtensions
    {
        public static void FromDto(this Person poco, MyNamespace.PersonCustom dto)
        {
            poco.FirstName = dto.TestAgain;
            poco.LastName = dto.LastName;
        }

        public static void FromPoco(this MyNamespace.PersonCustom dto, Person poco)
        {
            dto.TestAgain = poco.FirstName;
            dto.LastName = poco.LastName;
        }
    }
}