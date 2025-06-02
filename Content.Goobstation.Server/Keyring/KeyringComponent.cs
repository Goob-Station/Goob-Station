// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ilya246 <ilyukarno@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Keyring;

[RegisterComponent]
public sealed partial class KeyringComponent : Component
{
    /// <summary>
    /// How long each attempt takes to open a door.
    /// </summary>
    [DataField]
    public TimeSpan UnlockAttemptDuration = TimeSpan.FromSeconds(15);

    [DataField]
    public SoundSpecifier UseSound = new SoundPathSpecifier("/Audio/_Goobstation/Items/key_rustle.ogg");
}
