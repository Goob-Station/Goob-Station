namespace Content.Goobstation.Client.News.Conditions;

[ImplicitDataDefinitionForInheritors]
public abstract partial class NewsCondition
{
    public abstract bool Check(IDependencyCollection deps);
}
