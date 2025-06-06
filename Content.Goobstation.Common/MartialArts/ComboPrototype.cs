// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Common.MartialArts;

[Prototype("combo")]
[Serializable, NetSerializable, DataDefinition]
public sealed partial class ComboPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public MartialArtsForms MartialArtsForm;

    [DataField("attacks", required: true)]
    public List<ComboAttackType> AttackTypes = new();

    //[DataField("weapon")] // Will be done later
    //public string? WeaponProtoId;
    [DataField("event", required: true)]
    public object? ResultEvent;

    /// <summary>
    /// How much extra damage should this move do on perform?
    /// </summary>
    [DataField]
    public int ExtraDamage;

    /// <summary>
    /// Stun time in seconds
    /// </summary>
    [DataField]
    public int ParalyzeTime;

    /// <summary>
    /// How much stamina damage should this move do on perform.
    /// </summary>
    [DataField]
    public float StaminaDamage;

    /// <summary>
    /// Blunt, Slash, etc.
    /// </summary>
    [DataField]
    public string DamageType = "Blunt";

    /// <summary>
    /// How fast people are thrown on combo
    /// </summary>
    [DataField]
    public float ThrownSpeed = 7f;

    /// <summary>
    /// Name of the move
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

}

[Prototype("comboList")]
public sealed partial class ComboListPrototype : IPrototype
{
    [IdDataField] public string ID { get; private init; } = default!;

    [DataField( required: true)]
    public List<ProtoId<ComboPrototype>> Combos = new();
}