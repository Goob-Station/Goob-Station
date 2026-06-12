// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Added to a mech while a Malf AI mind controls it. The AI brain stays behind in its
/// holder, so the crew can still intellicard it during the hijack.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiMechHijackComponent : Component
{
    /// <summary>
    /// The AI brain the mind came from and returns to.
    /// </summary>
    [DataField]
    public EntityUid Brain;

    [DataField]
    public EntityUid? ReturnAction;

    [DataField]
    public bool AddedCombatComp;

    [DataField]
    public bool AddedPilotComp;

    [DataField]
    public EntityUid? CycleAction;

    [DataField]
    public EntityUid? UiAction;
}
