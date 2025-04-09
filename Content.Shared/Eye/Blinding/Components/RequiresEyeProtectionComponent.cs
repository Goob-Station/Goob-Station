// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Eye.Blinding.Components;

/// <summary>
/// For tools like welders that will damage your eyes when you use them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RequiresEyeProtectionComponent : Component
{
    /// <summary>
    /// How long to apply temporary blindness to the user.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("statusEffectTime"), AutoNetworkedField]
    public TimeSpan StatusEffectTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// You probably want to turn this on in yaml if it's something always on and not a welder.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("toggled"), AutoNetworkedField]
    public bool Toggled;
}