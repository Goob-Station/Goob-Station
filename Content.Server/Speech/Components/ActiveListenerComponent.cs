// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Chat.Systems;

namespace Content.Server.Speech.Components;

/// <summary>
///     This component is used to relay speech events to other systems.
/// </summary>
[RegisterComponent]
public sealed partial class ActiveListenerComponent : Component
{
    [DataField("range")]
    public float Range = ChatSystem.VoiceRange;
}