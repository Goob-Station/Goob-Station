// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Werewolf.UI;

[Serializable, NetSerializable]
public sealed class ClaimedMessage(ProtoId<WerewolfFormPrototype>? form) : BoundUserInterfaceMessage
{
    [ViewVariables]
    public ProtoId<WerewolfFormPrototype>? SelectedForm = form;
}
