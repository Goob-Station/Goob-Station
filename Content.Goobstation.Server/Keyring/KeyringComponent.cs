// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Keyring;

[RegisterComponent]
public sealed partial class KeyringComponent : Component
{
    /// <summary>
    /// How long each attempt takes to open a door.
    /// </summary>
    [DataField]
    public TimeSpan UnlockAttemptDuration = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The possible access levels.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<AccessLevelPrototype>> PossibleAccesses = [];

    /// <summary>
    /// How many access levels will be chosen.
    /// </summary>
    [DataField]
    public int MaxPossibleAccesses = 3;

    [DataField]
    public SoundSpecifier UseSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/key_rustle.ogg");
}
