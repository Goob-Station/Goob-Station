namespace Content.Goobstation.Common.Shaders;

[ByRefEvent]
public readonly record struct SetMultiShaderEvent(
    string Proto,
    bool Add,
    int RenderOrder,
    Color? Modulate = null,
    bool Mutable = true,
    bool RaiseEvent = false);

[ByRefEvent]
public readonly record struct SetMultiShadersEvent(Dictionary<string, MultiShaderData>? PostShaders, bool Add);

[DataDefinition]
public sealed partial class MultiShaderData
{
    [DataField]
    public bool Mutable = true;

    [DataField]
    public bool RaiseShaderEvent;

    [DataField]
    public Color? Color;

    [DataField]
    public int RenderOrder;
}
