using Content.Shared._Goobstation.MartialArts.Events;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.MartialArts.Components;

[NetworkedComponent]
[RegisterComponent]
public sealed partial class CanPerformComboComponent : Component
{
    public EntityUid? CurrentTarget;

    public List<ComboAttackType> LastAttacks = new();

    public List<ComboPrototype> AllowedCombos = new();

    [DataField]
    public List<ProtoId<ComboPrototype>> RoundstartCombos = new();

    public TimeSpan ResetTime = TimeSpan.Zero;

    public int ConsecutiveGnashes = 0;
}
