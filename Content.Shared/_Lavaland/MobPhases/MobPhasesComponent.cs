// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.MobPhases;

/// <summary>
/// Moves a boss between phases using damage thresholds and deaths.
/// Phase 0 is the starting state, every other phase is defined in <see cref="Phases"/>.
///
/// The current phase can be used in PhaseMegafaunaCondition to get
/// some attacks to be picked only at specific phases.
/// </summary>
[RegisterComponent, Access(typeof(MobPhasesSystem))]
[NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MobPhasesComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public List<MobPhase> Phases = new();

    [ViewVariables, AutoNetworkedField]
    public int CurrentPhase;

    [ViewVariables]
    public int LastPhase = -1;

    /// <summary>
    /// If true, when the boss heals the damage, allows them to switch to a previous phase.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CanSwitchBack;

    /// <summary>
    /// Go back to phase 0 after the fight.
    /// </summary>
    [DataField]
    public bool ResetOnShutdown = true;

    /// <summary>
    /// At which damage this megafauna switches phases.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<FixedPoint2, int> PhaseThresholds = new();

    /// <summary>
    /// How many times the boss has been killed this fight.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public int Deaths;

    [ViewVariables]
    public ComponentRegistry Stashed = new();

    [ViewVariables]
    public Dictionary<int, SpriteSpecifier> OriginalSprites = new();

    [ViewVariables]
    public Dictionary<EntityUid, ComponentRegistry> Enemies = new();
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MobPhase
{
    [DataField(required: true)]
    public int Phase;

    /// <summary>
    /// Enter phase once total damage reaches this much.
    /// </summary>
    [DataField]
    public FixedPoint2? Damage;

    /// <summary>
    /// Enter phase on death instead of dying.
    /// 1. first death, 2. second death, etc.
    /// </summary>
    [DataField]
    public int? OnDeath;

    /// <summary>
    /// How much of the dead threshold to heal when entering this phase.
    /// 1 = 100%, 0.5 = 50%, etc.
    /// </summary>
    [DataField]
    public float HealPercentageOnDeath = 1f;

    [DataField]
    public SpriteSpecifier? NewSprite;

    /// <summary>
    /// Sprite layer for newsprite.
    /// </summary>
    [DataField]
    public string? Layer;

    /// <summary>
    /// Components added when this phase is activated.
    /// </summary>
    [DataField]
    public List<ProtoId<MobPhaseComponentsPrototype>> AddComp = new();

    /// <summary>
    /// Components removed while this phase is active.
    /// They get added back after the phase ends.
    /// </summary>
    [DataField]
    public List<ProtoId<MobPhaseComponentsPrototype>> RemoveComp = new();

    [DataField]
    public List<ProtoId<MobPhaseComponentsPrototype>> AddEnemyComp = new();

    [DataField]
    public List<ProtoId<MobPhaseComponentsPrototype>> RemoveEnemyComp = new();

    [DataField]
    public LocId? SpeechOnEnter;
}
