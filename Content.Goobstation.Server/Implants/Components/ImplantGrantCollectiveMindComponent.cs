// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.CollectiveMind;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class ImplantGrantCollectiveMindComponent : Component
{
    [DataField]
    public ProtoId<CollectiveMindPrototype> CollectiveMind;
}
