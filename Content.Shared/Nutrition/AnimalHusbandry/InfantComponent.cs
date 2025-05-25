// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Sailor <109166122+Equivocateur@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Nutrition.AnimalHusbandry;

/// <summary>
/// This is used for marking entities as infants.
/// Infants have half the size, visually, and cannot breed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class InfantComponent : Component
{
    /// <summary>
    /// How long the entity remains an infant.
    /// </summary>
    [DataField("infantDuration")]
    public TimeSpan InfantDuration = TimeSpan.FromMinutes(3);

    /// <summary>
    /// The base scale of the entity
    /// </summary>
    [DataField("defaultScale")]
    public Vector2 DefaultScale = Vector2.One;

    /// <summary>
    /// The size difference of the entity while it's an infant.
    /// </summary>
    [DataField("visualScale")]
    public Vector2 VisualScale = new(.5f, .5f);

    /// <summary>
    /// When the entity will stop being an infant.
    /// </summary>
    [DataField("infantEndTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan InfantEndTime;
}