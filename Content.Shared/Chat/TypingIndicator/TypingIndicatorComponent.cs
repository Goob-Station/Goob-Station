// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Chat.TypingIndicator;

/// <summary>
///     Show typing indicator icon when player typing text in chat box.
///     Added automatically when player poses entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // CD - AutoGenerateComponentState fixes a bug with synth trait
// [Access(typeof(SharedTypingIndicatorSystem))] CD - Restricted access breaks synth trait because it rewrites the speech bubble over the default race indicator
public sealed partial class TypingIndicatorComponent : Component
{
    /// <summary>
    ///     Prototype id that store all visual info about typing indicator.
    /// </summary>
    [DataField("proto"), AutoNetworkedField] // CD - AutoNetworkedField fixes a bug in synth trait
    public ProtoId<TypingIndicatorPrototype> TypingIndicatorPrototype = "default";

    /// <summary>
    ///  DeltaV - Allow the indicator to be temporarily overriden
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<TypingIndicatorPrototype>? TypingIndicatorOverridePrototype;
}
