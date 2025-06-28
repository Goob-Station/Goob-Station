// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Silicons.Borgs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Silicons.Borgs;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(raiseAfterAutoHandleState: true)]
public sealed partial class BorgSwitchableSubtypeComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<BorgSubtypePrototype>? BorgSubtype;
}

[Serializable, NetSerializable]
public sealed class BorgSelectSubtypeMessage(ProtoId<BorgSubtypePrototype> subtype) : BoundUserInterfaceMessage
{
    public ProtoId<BorgSubtypePrototype> Subtype = subtype;
}
