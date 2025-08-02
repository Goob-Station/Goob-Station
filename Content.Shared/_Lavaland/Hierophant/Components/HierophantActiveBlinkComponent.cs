// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Lavaland.Hierophant.Components;

/// <summary>
/// Signifies that this entity is being blink-teleported to some spot.
/// TODO: cool shader for this fella
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class HierophantActiveBlinkComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan BlinkDelay = TimeSpan.FromSeconds(0.9f);

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    [ViewVariables]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? BlinkTime;

    [ViewVariables, AutoNetworkedField]
    public EntityCoordinates Coordinates;
}
