// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Chemistry.SolutionCartridge;

[RegisterComponent, NetworkedComponent]
public sealed partial class SolutionCartridgeReceiverComponent : Component
{
    [DataField(required: true)]
    public ProtoId<TagPrototype> Tag;

    [DataField]
    public SoundSpecifier InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");
}