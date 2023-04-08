using Arisoul.SourceGenerators.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetFrameworkSample
{
    [DtoClassGeneration(Name = "MyPerson")]
    public class Person
    {
        public int Id { get; set; }

        [DtoProperty]
        public string Name { get; set; }
    }
}
