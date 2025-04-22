// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.HoloCigar;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HoloCigarComponent : Component
{
    [ViewVariables]
    public bool Lit;

    [ViewVariables]
    public SoundSpecifier Music = new SoundPathSpecifier(
        "/Audio/_Goobstation/Items/TheManWhoSoldTheWorld/invisible.ogg",
        new AudioParams().WithLoop(true).WithVolume(3f));

    [ViewVariables]
    public EntityUid? MusicEntity;
}

[Serializable, NetSerializable]
public sealed class HoloCigarComponentState(bool lit) : ComponentState
{
    public bool Lit = lit;
}
