namespace Content.ModuleManager;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ContentModuleAttribute(ModuleType type) : Attribute
{
    public ModuleType Type { get; } = type;
}

public enum ModuleType
{
    Client,
    Server,
    Shared,
    Common,
}
