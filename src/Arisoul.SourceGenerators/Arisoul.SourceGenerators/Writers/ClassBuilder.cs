using System.Net.NetworkInformation;

namespace Arisoul.SourceGenerators.Writers;

// TODO: maybe in another time
internal class ClassBuilder :
    IExpectUsing,
    IExpectNamespace,
    IExpectClassName,
    IExpectPrivateField,
    IExpectPublicProperty,
    IExpectConstructor,
    IMethodBuilder,
    IClassBuilder
{
    private StringBuilder _stringBuilder = new();
    private bool _namespaceFileScoped = false;

    public ClassBuilder(bool addNullable = true)
    {
     ClassWriter.WriteClassHeader(addNullable);
    }

    public IExpectUsing WithUsing(string @using)
    {
        _stringBuilder.AppendLine(@$"using {@using};");

        return this;
    }

    public IExpectClassName WithNamespace(string @namespace, bool fileScoped = false)
    {
        _stringBuilder.Append(@$"
namespace {@namespace}");

        _namespaceFileScoped = fileScoped;

        if (_namespaceFileScoped)
            _stringBuilder.AppendLine(";");
        else
            _stringBuilder.AppendLine(@"
{");

        return this;
    }

    public IExpectPrivateField WithClassName(string name, string modifier = "public")
    {
        _stringBuilder.Append($@"
{modifier} class {name}
{{");

        return this;
    }

    public SourceText Build()
    {
        if (!_namespaceFileScoped)
            _stringBuilder.Append(@"
}");
        else
            _stringBuilder.Append(@"
    }
}");

        return SourceText.From(_stringBuilder.ToString(), Encoding.UTF8);
    }

    public IExpectPrivateField WithPrivateField(string type, string name)
    {
        throw new NotImplementedException();
    }

    public IExpectPublicProperty WithPublicProperty(string type, string name)
    {
        throw new NotImplementedException();
    }

    public IMethodBuilder WithContructor()
    {
        throw new NotImplementedException();
    }

    public IExpectsMethodBody WithMethod(string modifier, string returnType, string name, params KeyValuePair<string, string>[] args)
    {
        throw new NotImplementedException();
    }
}

interface IExpectUsing :
    IExpectNamespace
{
    IExpectUsing WithUsing(string @using);
}

interface IExpectNamespace
{
    IExpectClassName WithNamespace(string @namespace, bool fileScoped = false);
}

interface IExpectClassName :
    IExpectPrivateField,
    IExpectPublicProperty,
    IExpectConstructor,
    IMethodBuilder
{
    IExpectPrivateField WithClassName(string name, string modifier = "public");
}

interface IExpectPrivateField :
    IExpectPublicProperty,
    IExpectConstructor,
    IMethodBuilder
{
    IExpectPrivateField WithPrivateField(string type, string name);
}

interface IExpectPublicProperty :
    IExpectConstructor,
    IMethodBuilder,
    IClassBuilder
{
    IExpectPublicProperty WithPublicProperty(string type, string name);
}

interface IExpectConstructor :
    IMethodBuilder,
    IClassBuilder
{
    IMethodBuilder WithContructor();
}

interface IMethodBuilder
{
    IExpectsMethodBody WithMethod(string modifier, string returnType, string name, params KeyValuePair<string, string>[] args);
}

interface IExpectsMethodBody
{
    IExpectsMethodBody WithFieldAttribution(string left, string right);
    IExpectsMethodBody WithFieldDeclaration(string type, string name, bool initNew = true);
    IExpectsMethodBody WithBlock(string block);
    IExpectPublicProperty BuildMethod(); 
}

interface IClassBuilder
{
    SourceText Build();
}
