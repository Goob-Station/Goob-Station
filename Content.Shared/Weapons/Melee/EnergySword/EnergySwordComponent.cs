// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

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


// Goobstation-EsColorPicker-Start
[NetSerializable, Serializable]
public enum EsColorPickerMenu : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class EsColorChangedMessage(Color color) : BoundUserInterfaceMessage
{
    public Color Color = color;
}

[Serializable, NetSerializable]
public sealed class EsHackedStateChangedMessage(bool state) : BoundUserInterfaceMessage
{
    public bool State = state;
}
// Goobstation-EsColorPicker-End
