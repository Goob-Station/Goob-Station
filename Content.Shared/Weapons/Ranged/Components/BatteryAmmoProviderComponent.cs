using Robust.Shared.GameStates; // Shitmed Change
namespace Content.Shared.Weapons.Ranged.Components;

[RegisterComponent, NetworkedComponent] // Shitmed Change
public abstract partial class BatteryAmmoProviderComponent : AmmoProviderComponent
{
    /// <summary>
    /// How much battery it costs to fire once.
    /// </summary>
    [DataField("fireCost")] // Shitmed Change
    public float FireCost = 100;

    // Batteries aren't predicted which means we need to track the battery and manually count it ourselves woo!

    [DataField("shots")] // Shitmed Change
    public int Shots;

    [DataField("capacity")] // Shitmed Change
    public int Capacity;
}
