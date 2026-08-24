// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Lavaland.Damage.Components;

/// <summary>
/// Actor having this component will not get damaged by damage squares.
/// </summary>
/// <remarks>
/// TODO: cool shader for this fella
/// Also, maybe we should move this thing to DamageableSystem if it ever gets predictions
/// </remarks>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class DamageSquareImmunityComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField, AutoNetworkedField]
    public TimeSpan? ImmunityEndTime;
}
