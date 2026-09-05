// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Weapons.AmmoSelector;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Weapons.AmmoSelector;

[Serializable, NetSerializable]
public sealed class AmmoSelectedMessage(ProtoId<SelectableAmmoPrototype> protoId) : BoundUserInterfaceMessage
{
    public ProtoId<SelectableAmmoPrototype> ProtoId { get; } = protoId;
}

[Serializable, NetSerializable]
public enum AmmoSelectorUiKey : byte
{
    Key
}