// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Magnus Larsen <i.am.larsenml@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
namespace Content.Shared.Flash.Components;

/// <summary>
/// Upon being triggered will flash in an area around it.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FlashOnTriggerComponent : Component
{
    [DataField] public float Range = 1.0f;
    [DataField] public float Duration = 8.0f;
    [DataField] public float Probability = 1.0f;
}