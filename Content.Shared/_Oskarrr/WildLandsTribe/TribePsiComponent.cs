// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Oskarrr.WildLandsTribe;

/// <summary>
/// Lightweight tribal "psionics" — energy pool + aura heal / regen / resurrect.
/// Full Nyano psionics are not in Badlands.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TribePsiComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Energy = 100f;

    [DataField, AutoNetworkedField]
    public float MaxEnergy = 100f;

    [DataField]
    public float RegenPerSecond = 2f;

    /// <summary>
    /// Instant full regen while standing on these tiles (e.g. FloorOskarrrMoss).
    /// </summary>
    [DataField]
    public HashSet<string> InstantRegenTiles = new() { "FloorOskarrrMoss" };

    [DataField]
    public float HealCost = 25f;

    [DataField]
    public float HealRange = 5f;

    [DataField]
    public DamageSpecifier HealDamage = new();

    [DataField]
    public float RegenCost = 35f;

    [DataField]
    public float RegenRange = 6f;

    [DataField]
    public float RegenDuration = 12f;

    [DataField]
    public float RegenTickInterval = 1f;

    [DataField]
    public DamageSpecifier RegenDamage = new();

    [DataField]
    public float ResurrectCost = 75f;

    [DataField]
    public float ResurrectRange = 2.5f;

    [DataField]
    public DamageSpecifier ResurrectHeal = new();

    [ViewVariables]
    public float Accumulator;
}

/// <summary>
/// Temporary shamanic regeneration applied by Psi-Regen.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TribePsiRegenBuffComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    [DataField]
    public float TickInterval = 1f;

    [DataField]
    public DamageSpecifier HealPerTick = new();

    [ViewVariables]
    public float Accumulator;
}

public sealed partial class AboriginePsiHealEvent : InstantActionEvent;

public sealed partial class AboriginePsiRegenEvent : InstantActionEvent;

public sealed partial class AboriginePsiResurrectEvent : EntityTargetActionEvent;
