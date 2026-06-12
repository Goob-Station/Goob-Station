// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Marks an action as a Malf AI ability: only usable while the AI is held
/// in a holder that applies the AiHeld registry (core or shunted APC).
/// Blocks ability use from an intellicard.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiActionComponent : Component;
