// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Verm <32827189+Vermidia@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Labels.Components;

/// <summary>                                                                                                                                                 
/// Specifies the paper type (see textures/storage/crates/labels.rsi to see currently supported paper types)  to show on crates this label is attached to.                                                                     
/// </summary>                                                                                                                                                
[RegisterComponent, NetworkedComponent]
public sealed partial class PaperLabelTypeComponent : Component
{
    /// <summary>                                                                                                                                             
    /// The type of label to show.                                                                                                                                    
    /// </summary>
    [DataField]
    public string PaperType = "Paper";
}