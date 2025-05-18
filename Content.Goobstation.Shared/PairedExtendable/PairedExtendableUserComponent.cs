// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.PairedExtendable;

/// <summary>
/// Abstract component for easier work with PairedExtendableSystem.
/// Check (or copy-paste) RightMantisBladeUserComponent and server-side MantisBladesSystem for reference.
/// </summary>
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
    public bool MakeUnremovable = true; // Maybe make a component registry of components to add to the extendable?
}

public abstract partial class RightPairedExtendableUserComponent : PairedExtendableUserComponent;
public abstract partial class LeftPairedExtendableUserComponent : PairedExtendableUserComponent;
