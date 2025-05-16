// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.PairedExtendable;

[RegisterComponent]
public abstract partial class PairedExtendableUserComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ActionUid;

    [DataField]
    public SoundSpecifier? ExtendSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/MantisBlades/mantis_extend.ogg");

    [DataField]
    public SoundSpecifier? RetractSound = new SoundCollectionSpecifier("MantisBladeRetract");

    [DataField]
    public string ExtendableProto = "MantisBlade";

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? ExtendableUid;

    [DataField]
    public bool AffectedByEmp = true;
}
