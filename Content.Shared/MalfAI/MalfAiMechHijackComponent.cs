// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marker component stored on the AI while it is hijacking a mech.
/// Tracks the hijacked mech and the AI core holder to return to.
/// Also stores the Return-to-Core action entity assigned during hijack (for cleanup).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiMechHijackComponent : Component
{
    [DataField]
    public EntityUid? HijackedMech;

    [DataField]
    public EntityUid? CoreHolder;

    [DataField]
    public EntityUid? ReturnAction;

    [DataField]
    public EntityUid? CombatToggleAction;

    // Tracks whether we added CombatModeComponent during hijack, so we can remove it on cleanup.
    [DataField]
    public bool AddedCombatComp;
}
