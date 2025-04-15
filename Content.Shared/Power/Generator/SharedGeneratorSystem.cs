// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Power.Generator;

/// <summary>
/// This handles small, portable generators that run off a material fuel.
/// </summary>
/// <seealso cref="FuelGeneratorComponent"/>
public abstract class SharedGeneratorSystem : EntitySystem
{
    /// <summary>
    /// Calculates the expected fuel efficiency based on the optimal and target power levels.
    /// </summary>
    /// <param name="targetPower">Target power level</param>
    /// <param name="optimalPower">Optimal power level</param>
    /// <param name="component"></param>
    /// <returns>Expected fuel efficiency as a percentage</returns>
    public static float CalcFuelEfficiency(float targetPower, float optimalPower, FuelGeneratorComponent component)
    {
        return MathF.Pow(optimalPower / targetPower, component.FuelEfficiencyConstant);
    }
}