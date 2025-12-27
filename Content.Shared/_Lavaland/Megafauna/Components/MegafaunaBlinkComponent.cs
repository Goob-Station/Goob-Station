// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Signifies that this entity is being blink-teleported to some spot.
/// TODO: cool shader for this fella
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MegafaunaBlinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.9f);

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    /// <summary>
    /// Entity that will be spawned on a target blink position.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnOnTarget;

    /// <summary>
    /// Entity that will be spawned on original position when before teleportation.
    /// </summary>
    [DataField]
    public EntProtoId? SpawnOnUsed;
}
