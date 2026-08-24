// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CanPerformComboComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? CurrentTarget;

    [DataField, AutoNetworkedField]
    public ProtoId<ComboPrototype> BeingPerformed;

    [DataField]
    public int LastAttacksLimit = 4;

    [DataField, AutoNetworkedField]
    public List<ComboAttackType> LastAttacks = new();

    [DataField]
    public List<ComboAttackType>? LastAttacksSaved = new();

    [DataField]
    public List<ComboPrototype> AllowedCombos = new();

    [DataField]
    public List<ProtoId<ComboPrototype>> RoundstartCombos = new();

    [DataField]
    public TimeSpan ResetTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public int ConsecutiveGnashes;
}
