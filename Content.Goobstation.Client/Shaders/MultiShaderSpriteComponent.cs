using Content.Goobstation.Common.Shaders;

namespace Content.Goobstation.Client.Shaders;

[RegisterComponent]
public sealed partial class MultiShaderSpriteComponent : Component
{
    [DataField]
    public Dictionary<string, MultiShaderData> PostShaders = new();
}
