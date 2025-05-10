// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Devil.Grip;

[RegisterComponent]
public sealed partial class DevilGripComponent : Component
{
    [DataField]
    public TimeSpan CooldownAfterUse = TimeSpan.FromSeconds(20);

    [DataField]
    public EntityWhitelist Blacklist = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5f);

    [DataField]
    public float StaminaDamage = 80f;

    [DataField]
    public TimeSpan SpeechTime = TimeSpan.FromSeconds(10f);

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/bone_crack.ogg");

    [DataField]
    public LocId Invocation = "devil-speech-grip";
}
