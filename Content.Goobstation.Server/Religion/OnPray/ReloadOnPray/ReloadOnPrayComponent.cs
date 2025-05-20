// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Religion.OnPray.ReloadOnPray;

[RegisterComponent]
public sealed partial class ReloadOnPrayComponent : Component
{
    [DataField]
    public SoundPathSpecifier ReloadSoundPath = new ("/Audio/Weapons/Guns/MagIn/shotgun_insert.ogg");
}
