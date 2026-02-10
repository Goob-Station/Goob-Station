using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EldritchSleepStatusEffectComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry ComponentsToAdd = new();

    [DataField]
    public ComponentRegistry ComponentDifference = new();
}
