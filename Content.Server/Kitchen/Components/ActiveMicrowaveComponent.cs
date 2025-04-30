// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 TinManTim <73014819+Tin-Man-Tim@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Kitchen;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Kitchen.Components;

/// <summary>
/// Attached to a microwave that is currently in the process of cooking
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class ActiveMicrowaveComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float CookTimeRemaining;

    [ViewVariables(VVAccess.ReadWrite)]
    public float TotalTime;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan MalfunctionTime = TimeSpan.Zero;

    [ViewVariables]
    public (FoodRecipePrototype?, int) PortionedRecipe;
}