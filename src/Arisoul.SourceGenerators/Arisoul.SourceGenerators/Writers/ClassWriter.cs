﻿using System.Text;

namespace Arisoul.SourceGenerators.Writers;

internal static class ClassWriter
{
    public static string WriteClassHeader(bool addNullable)
    {
        StringBuilder sb = new();
        if (addNullable)
            sb.Append(@"#nullable enable");


        sb.Append(@"
/*  <auto-generated> ------------------------------------------------------------------------------ 
        This code was generated by Arisoul.SourceGenerators tool.
        Changes to this file may cause incorrect behavior and will be lost if
        the code is regenerated.
    </auto-generated> ------------------------------------------------------------------------------*/");

        return sb.ToString();
    }

    public static string WriteUsing(string @using) => $"using {@using};";
}
