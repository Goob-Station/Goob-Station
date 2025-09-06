// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Silicon.MalfAI.Objectives;

[RegisterComponent]
public sealed partial class PurgeSpeciesObjectiveComponent : Component
{
    [DataField]
    public string TitleLoc = string.Empty;

    [DataField]
    public ProtoId<SpeciesPrototype> TargetSpeciesPrototype;

    [DataField(required: true)]
    public List<ProtoId<SpeciesPrototype>> SpeciesWhitelist;
}