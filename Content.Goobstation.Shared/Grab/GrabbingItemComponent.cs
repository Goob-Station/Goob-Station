// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Grab;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabbingItemComponent : Component
{
    [DataField]
    public GrabStage GrabStageOverride = GrabStage.Hard;

    [DataField]
    public float EscapeAttemptModifier = 2f;

    [DataField, AutoNetworkedField]
    public EntityUid? ActivelyGrabbingEntity;
}
