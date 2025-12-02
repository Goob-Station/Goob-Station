using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TerrorSpiderComponent : Component
{
    /// <summary>
    /// Keeps track of how many creatures this individual terror spider has wrapped.
    /// </summary>
    [DataField]
    public int WrappedAmount;


    /// <summary>
    /// Max amount of corpses to count before you stop multiplying the regen. 12 healing per second is pretty busted.
    /// </summary>
    [DataField]
    public int MaxRegenCorpses = 12;

    /// <summary>
    /// Stores the original passive regen so scaling isn't exponential.
    /// </summary>
    [DataField]
    public DamageSpecifier? BaselineRegen;

    [DataField, AutoNetworkedField]
    public float RegenAccumulator = 0f;

    [DataField]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/Animals/Blob/attackblob.ogg");
}
