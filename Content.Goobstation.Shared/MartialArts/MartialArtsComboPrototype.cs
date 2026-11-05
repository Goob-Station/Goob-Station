// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Shared.Construction;
using Content.Shared.EntityConditions;
using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts;

[Prototype]
public sealed partial class MartialArtsComboPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public List<ComboAttackType> Attacks = [];

    [DataField]
    public EntityWhitelist WeaponWhitelist = new(); // Not currently implemented

    [DataField]
    public EntityCondition[] UserConditions = [];

    [DataField]
    public EntityCondition[] TargetConditions = [];

    [DataField]
    public EntityEffect[] UserEffects = [];

    [DataField]
    public EntityEffect[] TargetEffects = [];

    [DataField]
    public EntityEffect[] UserFailEffects = [];

    [DataField]
    public EntityEffect[] TargetFailEffects = [];

    /// <summary>
    /// Should the target drop items on knockdown?
    /// </summary>
    [DataField]
    public bool DropItems = true;

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

    /// <summary>
    /// Is this combo performed on self only or only on other targets
    /// </summary>
    [DataField]
    public bool PerformOnSelf;
}
