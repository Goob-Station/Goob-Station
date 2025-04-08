// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Lube;

[RegisterComponent, NetworkedComponent]
public sealed partial class LubeComponent : Component
{
    [DataField("squeeze")]
    public SoundSpecifier Squeeze = new SoundPathSpecifier("/Audio/Items/squeezebottle.ogg");

    /// <summary>
    /// Solution on the entity that contains the glue.
    /// </summary>
    [DataField("solution")]
    public string Solution = "drink";

    /// <summary>
    /// Reagent that will be used as glue.
    /// </summary>
    [DataField("reagent", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string Reagent = "SpaceLube";

    /// <summary>
    /// Reagent consumption per use.
    /// </summary>
    [DataField("consumption"), ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Consumption = FixedPoint2.New(3);

    /// <summary>
    /// Min slips per unit
    /// </summary>
    [DataField("minSlips"), ViewVariables(VVAccess.ReadWrite)]
    public int MinSlips = 1;

    /// <summary>
    /// Max slips per unit
    /// </summary>
    [DataField("maxSlips"), ViewVariables(VVAccess.ReadWrite)]
    public int MaxSlips = 6;

    [DataField("slipStrength"), ViewVariables(VVAccess.ReadWrite)]
    public int SlipStrength = 10;
}