// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Plykiya <58439124+Plykiya@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.EnergySword;

[RegisterComponent, NetworkedComponent, Access(typeof(EnergySwordSystem))]
[AutoGenerateComponentState]
public sealed partial class EnergySwordComponent : Component
{
    /// <summary>
    /// What color the blade will be when activated.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color ActivatedColor = Color.DodgerBlue;

    /// <summary>
    ///     A color option list for the random color picker.
    /// </summary>
    [DataField]
    public List<Color> ColorOptions = new()
    {
        Color.Tomato,
        Color.DodgerBlue,
        Color.Aqua,
        Color.MediumSpringGreen,
        Color.MediumOrchid
    };

    /// <summary>
    /// Whether the energy sword has been pulsed by a multitool,
    /// causing the blade to cycle RGB colors.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Hacked;

    /// <summary>
    ///     RGB cycle rate for hacked e-swords.
    /// </summary>
    [DataField]
    public float CycleRate = 1f;
}