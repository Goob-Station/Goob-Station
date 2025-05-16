// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.MantisBlades;

[RegisterComponent]
public sealed partial class LeftMantisBladeUserComponent : Component, IMantisBladeUserComponent
{
    [DataField]
    public string ActionProto = "ActionToggleLeftMantisBlade";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionUid;

    [DataField]
    public SoundSpecifier? ExtendSound { get; set; } = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/MantisBlades/mantis_extend.ogg");

    [DataField]
    public SoundSpecifier? RetractSound { get; set; } = new SoundCollectionSpecifier("MantisBladeRetract");

    [DataField]
    public string BladeProto { get; set; } = "MantisBlade";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? BladeUid { get; set; }
}
