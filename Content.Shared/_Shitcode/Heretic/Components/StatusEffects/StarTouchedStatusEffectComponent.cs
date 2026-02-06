using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class StarTouchedStatusEffectComponent : Component
{
    [DataField]
    public TimeSpan SleepTime = TimeSpan.FromSeconds(8);

    [DataField]
    public EntProtoId CosmicCloud = "EffectCosmicCloud";
}
