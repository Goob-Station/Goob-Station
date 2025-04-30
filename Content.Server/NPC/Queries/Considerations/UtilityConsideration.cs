// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.NPC.Queries.Curves;
using JetBrains.Annotations;

namespace Content.Server.NPC.Queries.Considerations;

[ImplicitDataDefinitionForInheritors, MeansImplicitUse]
public abstract partial class UtilityConsideration
{
    [DataField("curve", required: true)]
    public IUtilityCurve Curve = default!;
}