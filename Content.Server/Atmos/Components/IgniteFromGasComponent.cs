using Content.Shared._Shitmed.Targeting;
using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components;

/// <summary>
///   Component that can be used to add (or remove) fire stacks when exposed to a type of gas, unless wearing ignition immunity.
///   Don't add this component directly, instead attach a body part that has IgniteFromGasPartComponent.
/// </summary>
[RegisterComponent]
public sealed partial class IgniteFromGasComponent : Component
{
    /// <summary>
    ///   What type of gas triggers ignition.
    ///   Right now only one type of gas can be set, instead of different gasses per each body part.
    /// </summary>
    [DataField]
    public Gas Gas;

    /// <summary>
    ///   The total calculated fire stacks to apply every second without immunity.
    /// </summary>
    [DataField]
    public float FireStacksPerUpdate = 0f;

    /// <summary>
    ///   If this entity is currently not self-igniting.
    /// </summary>
    public bool HasImmunity => FireStacksPerUpdate == 0;

    /// <summary>
    ///   The base amount of fire stacks to apply every second without immunity.
    /// </summary>
    [DataField]
    public float BaseFireStacksPerUpdate = 0.13f;

    /// <summary>
    ///   The body parts that are vulnerable to ignition when exposed, and their fire stack values.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<TargetBodyPart, float> IgnitableBodyParts = new();

    /// <summary>
    ///   How many moles of the gas is needed to trigger ignition.
    /// </summary>
    [DataField]
    public float MolesToIgnite = 0.5f;
}
