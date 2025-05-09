// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Random;

/// <summary>
/// IWeightedRandomPrototype implements a dictionary of strings to float weights
/// to be used with <see cref="Helpers.SharedRandomExtensions.Pick(IWeightedRandomPrototype, Robust.Shared.Random.IRobustRandom)" />.
/// </summary>
public interface IWeightedRandomPrototype : IPrototype
{
    [ViewVariables]
    public Dictionary<string, float> Weights { get; }
}