// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

[RegisterComponent, NetworkedComponent]
public sealed partial class BoneSetterComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "a bone setter";
    public bool? Used { get; set; } = null;
    [DataField]
    public float Speed { get; set; } = 1f;
}