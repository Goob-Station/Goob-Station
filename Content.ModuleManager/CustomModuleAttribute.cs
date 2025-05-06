namespace Content.ModuleManager;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public sealed class ContentModuleAttribute(ModuleType type, params string[] dependencies) : Attribute
{
    public ModuleType Type { get; } = type;
    public string[] Dependencies { get; } = dependencies ?? Array.Empty<string>();
}

public enum ModuleType
{
    Client,
    Server,
    Shared,
    Common,
}
