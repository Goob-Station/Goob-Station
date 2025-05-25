// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;

/// <summary>
///     Activate artifact when it contacted with an electricity source.
///     It could be connected MV cables, stun baton or multi tool.
/// </summary>
[RegisterComponent]
public sealed partial class ArtifactElectricityTriggerComponent : Component
{
    /// <summary>
    ///     How much power should artifact receive to operate.
    /// </summary>
    [DataField("minPower")]
    public float MinPower = 400;
}