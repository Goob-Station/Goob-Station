// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ComboPerformerComponent : Component
{
    [DataField]
    public TimeSpan LastAttacksResetDelay = TimeSpan.FromSeconds(5);

    [ViewVariables, AutoNetworkedField]
    public EntityUid? CurrentTarget;

    [ViewVariables, AutoNetworkedField]
    public List<ComboAttackType> LastAttacks = [];

    [ViewVariables, AutoNetworkedField]
    public TimeSpan ResetTime = TimeSpan.Zero;
}
