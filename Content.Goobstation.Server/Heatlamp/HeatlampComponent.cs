// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Item;
using Content.Shared.Temperature;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Heatlamp;

[RegisterComponent]
public sealed partial class HeatlampComponent : Component
{
    /// <summary>
    /// Power used when heating at the high setting.
    /// Low and medium are 33% and 66% respectively.
    /// </summary>
    [DataField]
    public float Power = 100f;

    /// <summary>
    /// How much the power used is multiplied by before being turned into heat.
    /// </summary>
    [DataField]
    public float PowerToHeatMultiplier = 60f;

    /// <summary>
    /// The multiplier used when the temperature delta is negative
    /// AKA, it is cooling not heating.
    /// </summary>
    [DataField]
    public float NegativeDeltaMultiplier = -0.5f;

    /// <summary>
    /// Current setting of the heater. If it is off or unpowered it won't heat anything.
    /// </summary>
    [DataField]
    public EntityHeaterSetting Setting = EntityHeaterSetting.Off;

    /// <summary>
    /// Should the efficiency be lowered when contained? (E.G, in a bag)
    /// </summary>
    [DataField]
    public bool LowerEfficiencyWhenContained = true;

    /// <summary>
    /// Does this lamp require an internal battery to function?
    /// </summary>
    [DataField]
    public bool NeedsPower = true;

    /// <summary>
    /// Should the temperature change happen regardless of resistances?
    /// </summary>
    [DataField]
    public bool ForceHeat;

    /// <summary>
    /// What amount is the efficiency multiplied by when contained.
    /// </summary>
    [DataField]
    public float ContainerMultiplier = 0.3f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentPowerDraw;

    /// <summary>
    /// An optional sound that plays when the setting is changed.
    /// </summary>
    [DataField]
    public SoundSpecifier SettingSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    /// <summary>
    /// The size of this lamp when off.
    /// </summary>
    [DataField]
    public ProtoId<ItemSizePrototype> OffSize = "Small";

    /// <summary>
    /// The size of this lamp when on.
    /// </summary>
    [DataField]
    public ProtoId<ItemSizePrototype> OnSize = "Normal";

    /// <summary>
    /// The entity to be heated.
    /// </summary>
    [ViewVariables]
    public EntityUid? User;

    /// <summary>
    /// The shape when off.
    /// </summary>
    [DataField]
    public List<Box2i> OffShape;

    /// <summary>
    /// The shape when on.
    /// </summary>
    [DataField]
    public List<Box2i> OnShape;

    /// <summary>
    /// The damage when on.
    /// </summary>
    [DataField]
    public DamageSpecifier? ActivatedDamage;

    /// <summary>
    /// The damage when off.
    /// </summary>
    [DataField]
    public DamageSpecifier? DeactivatedDamage;

}
