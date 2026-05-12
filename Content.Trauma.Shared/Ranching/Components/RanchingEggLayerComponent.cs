// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Nutrition.Components;
using Robust.Shared.Audio;

namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// This is used for ranching egg laying
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class RanchingEggLayerComponent : Component
{
    /// <summary>
    /// The item that gets laid/spawned.
    /// </summary>
    [DataField]
    public EntProtoId? EggSpawn;

    [DataField]
    public SoundSpecifier EggLaySound = new SoundPathSpecifier("/Audio/Effects/pop.ogg");

    /// <summary>
    /// Minimum cooldown used for the egg laying.
    /// </summary>
    [DataField]
    public float EggLayCooldownMin = 15f;

    /// <summary>
    /// Maximum cooldown used for the egg laying.
    /// </summary>
    [DataField]
    public float EggLayCooldownMax = 40f;

    /// <summary>
    /// The amount of nutrient consumed on eggLay.
    /// </summary>
    [DataField]
    public float HungerUsage = 10f;

    /// <summary>
    /// What hunger threshold is required
    /// </summary>
    [DataField]
    public HungerThreshold HungerThresholdRequired = HungerThreshold.Okay;

    /// <summary>
    /// Whether given entity needs to have Hunger in order to lay eggs
    /// </summary>
    [DataField]
    public bool HungerRequired = true;

    /// <summary>
    /// When to next try to lay an egg.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextGrowth = TimeSpan.Zero;

    [DataField]
    public string Solution = "bloodstream";
}
