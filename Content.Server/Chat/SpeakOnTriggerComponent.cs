// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.Chat;

/// <summary>
///     Makes the entity speak when triggered. If the item has UseDelay component, the system will respect that cooldown.
/// </summary>
[RegisterComponent]
public sealed partial class SpeakOnTriggerComponent : Component
{
    /// <summary>
    ///     The identifier for the dataset prototype containing messages to be spoken by this entity.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<LocalizedDatasetPrototype> Pack = string.Empty;
}