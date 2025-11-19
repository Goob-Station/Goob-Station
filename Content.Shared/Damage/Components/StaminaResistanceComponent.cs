using Content.Shared.Damage.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Component that provides entities with stamina resistance.
/// By default this is applied when worn, but to solely protect the entity itself and
/// not the wearer use <c>worn: false</c>.
/// </summary>
/// <remarks>
/// This is desirable over just using damage modifier sets, given that equipment like bomb-suits need to
/// significantly reduce the damage, but shouldn't be silly overpowered in regular combat.
/// </remarks>
[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
public sealed partial class StaminaResistanceComponent : Component
{
    /// <summary>
    /// The stamina resistance coefficient, This fraction is multiplied into the total resistance.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float DamageCoefficient = 1;

    /// <summary>
    /// When true, resistances will be applied to the entity wearing this item.
    /// When false, only this entity will get the resistance.
    /// </summary>
    [DataField]
    public bool Worn = true;

    /// <summary>
    /// Examine string for stamina resistance.
    /// Passed <c>value</c> from 0 to 100.
    /// </summary>
    [DataField]
    public LocId Examine = "armor-stamina-projectile-coefficient-value"; // DeltaV


    /// <summary>
    /// DeltaV - Whether or not this includes melee resistance. By default, DeltaV assigns melee stamina resistance to
    /// blunt damage, but if this is set to true, it was override that and use the stamina resistance value.
    ///
    /// If this is true, then the stamina resistance will double-dip with any blunt resistance, since stamina damage due to
    /// blunt damage is calculated after blunt resistance is applied. Basically, use this when you want to make something even
    /// resistant or even immune to melee stamina damage.
    /// </summary>
    [DataField]
    public bool MeleeResistance = false;
}
