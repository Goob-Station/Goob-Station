// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MantisBlades;

[RegisterComponent]
public sealed partial class MantisBladeArmComponent : Component
{
    [DataField]
    public string ActionProto;

    [DataField]
    public EntityUid? ActionUid;

    [DataField]
    public string BladeProto = "MantisBlade";

    [DataField]
    public EntityUid? BladeUid;

    [DataField]
    public SoundSpecifier? ExtendSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/MantisBlades/mantis_extend.ogg");

    [DataField]
    public SoundSpecifier? RetractSound = new SoundCollectionSpecifier("MantisBladeRetract");
}

public sealed partial class ToggleMantisBladeEvent : InstantActionEvent;
