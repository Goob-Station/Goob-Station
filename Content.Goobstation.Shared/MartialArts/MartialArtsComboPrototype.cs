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
    /// Name of the move
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    /// <summary>
    /// Can this combo be used on non-mobs?
    /// </summary>
    [DataField]
    public bool PerformableOnObjects = false;

    /// <summary>
    /// Is this combo performed on self or on other targets
    /// </summary>
    [DataField]
    public bool PerformOnSelf = false;
}
