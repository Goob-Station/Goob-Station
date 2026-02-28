using Content.Goobstation.Common.Shaders;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class AddShadersStatusEffectComponent : Component
{
    [DataField(required: true)]
    public Dictionary<string, MultiShaderData> PostShaders;
}
