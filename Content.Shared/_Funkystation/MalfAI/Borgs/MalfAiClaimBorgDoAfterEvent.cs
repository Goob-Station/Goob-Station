// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.MalfAI.Borgs;

[Serializable, NetSerializable]
public sealed partial class MalfAiClaimBorgDoAfterEvent : DoAfterEvent
{
    public NetEntity Borg;

    public MalfAiClaimBorgDoAfterEvent() { }

    public MalfAiClaimBorgDoAfterEvent(NetEntity borg)
    {
        Borg = borg;
    }

    public override DoAfterEvent Clone() => new MalfAiClaimBorgDoAfterEvent(Borg);
}
