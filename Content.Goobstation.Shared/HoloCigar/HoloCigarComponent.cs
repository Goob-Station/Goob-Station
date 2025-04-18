// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using System;
using Robust.Shared.Audio;
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

    [DataField]
    public SoundSpecifier? Music;

    [ViewVariables]
    public EntityUid? MusicEntity;
}

[Serializable, NetSerializable]
public sealed class HoloCigarComponentState: ComponentState
{
    public bool Lit;
    public HoloCigarComponentState(bool lit)
    {
        Lit = lit;
    }
}
