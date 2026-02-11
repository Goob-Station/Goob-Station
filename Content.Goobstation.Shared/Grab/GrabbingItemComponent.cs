// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Grab;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabbingItemComponent : Component, IGrabCooldownComponent
{
    [DataField]
    public GrabStage GrabStageOverride = GrabStage.Hard;

    [DataField]
    public float EscapeAttemptModifier = 2f;

    [DataField, AutoNetworkedField]
    public EntityUid? ActivelyGrabbingEntity;
    
    [DataField, AutoNetworkedField]
    public TimeSpan GrabCooldownDuration { get; set; } = TimeSpan.FromSeconds(0);
    
    [DataField]
    public string GrabCooldownVerb { get; set; } = "grabbing-item-cooldown-verb";

    [DataField, AutoNetworkedField]
    public TimeSpan GrabCooldownEnd { get; set; } = TimeSpan.Zero;

    public bool IsCooldownActive(TimeSpan now)
    {
        return GrabCooldownEnd > now;
    }

    public void StartCooldown(TimeSpan now)
    {
        GrabCooldownEnd = now + GrabCooldownDuration;
    }
}
