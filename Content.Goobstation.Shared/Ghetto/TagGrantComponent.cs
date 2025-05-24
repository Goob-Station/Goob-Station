// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Ghetto;

[RegisterComponent, NetworkedComponent]
public sealed partial class TagGrantComponent : Component
{
    [DataField]
    public int Uses = 1;

    [DataField]
    public ProtoId<TagPrototype> Tag = string.Empty;

    [DataField]
    public LocId Popup = string.Empty;
}
