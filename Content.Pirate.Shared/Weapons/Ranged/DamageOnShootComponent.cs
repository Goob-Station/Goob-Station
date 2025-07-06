using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.Damage;

namespace Content.Pirate.Shared.Damage.Components;


/// <summary>
/// This component is added to entities that you want to damage the player
/// if the player shoots it.
/// This damage can be cancelled if the user has a component that protects them from this.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DamageOnShootComponent : Component
{
    /// <summary>
    /// How much damage to apply to the person shooting
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = default!;

    /// <summary>
    /// Whether the damage should be resisted by a person's armor values
    /// and the <see cref="DamageOnShootProtectionComponent"/>
    /// </summary>
    [DataField]
    public bool IgnoreResistances;

    /// <summary>
    /// What kind of localized text should pop up when they interact with the entity
    /// </summary>
    [DataField]
    public LocId? PopupText;

    /// <summary>
    /// The sound that should be made when interacting with the entity
    /// </summary>
    [DataField]
    public SoundSpecifier DamageSound = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    /// <summary>
    /// Generic boolean to toggle the damage application on and off
    /// This is useful for things that can be toggled on or off, like a stovetop
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsDamageActive = true;

    /// <summary>
    /// Time between being able to interact with this entity
    /// </summary>
    [DataField]
    public uint InteractTimer = 0;

    /// <summary>
    /// Tracks the last time this entity was interacted with, but only if the interaction resulted in the user taking damage
    /// </summary>
    [DataField]
    public TimeSpan LastInteraction = TimeSpan.Zero;

    /// <summary>
    /// Tracks the time until this entity can next be interacted with, but only if the interaction resulted in the user taking damage
    /// </summary>
    [DataField]
    public TimeSpan NextInteraction = TimeSpan.Zero;

    /// <summary>
    /// Probability that the user will be stunned when they interact with with this entity and took damage
    /// </summary>
    [DataField]
    public float StunChance = 0.0f;

    /// <summary>
    /// Duration, in seconds, of the stun applied to the user when they interact with the entity and took damage
    /// </summary>
    [DataField]
    public float StunSeconds = 0.0f;
}
