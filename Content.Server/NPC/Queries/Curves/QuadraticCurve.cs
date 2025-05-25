// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.Queries.Curves;

public sealed partial class QuadraticCurve : IUtilityCurve
{
    [DataField("slope")] public  float Slope = 1f;

    [DataField("exponent")] public  float Exponent = 1f;

    [DataField("yOffset")] public  float YOffset;

    [DataField("xOffset")] public  float XOffset;
}