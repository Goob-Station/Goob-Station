// SPDX-FileCopyrightText: 2025 Evaisa <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.FloorGoblin;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StealShoesComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId? ActionProto;

    [DataField, AutoNetworkedField]
    public EntityUid? StealAction;

    [DataField]
    public string ContainerId = "floorgoblin-shoes";

    [DataField]
    public SoundSpecifier? ChompSound = new SoundPathSpecifier("/Audio/_Goobstation/FloorGoblin/laugh.ogg");
}

public sealed partial class StealShoesEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class StealShoesDoAfterEvent : SimpleDoAfterEvent;
