﻿#nullable enable
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DtoGenerator.Domain
{
    public partial class Person
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual global::System.DateTime Date { get; set; }
        public virtual global::DtoGenerator.Person RelatedPerson { get; set; }
        public virtual ICollection<DtoGenerator.Person> PeopleICollection { get; set; }
        public virtual Collection<DtoGenerator.Person> PeopleCollection { get; set; }
        public virtual IList<DtoGenerator.Person> PeopleIList { get; set; }
        public virtual List<DtoGenerator.Person> PeopleList { get; set; }
        public virtual Dictionary<System.String, DtoGenerator.Person> PeopleDictionary { get; set; }
        public virtual IDictionary<System.String, DtoGenerator.Person> PeopleIDictionary { get; set; }
        public virtual global::DtoGenerator.PersonEnum PersonEnum { get; set; }
    }
}