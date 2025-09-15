using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HauntComponent : Component
{
    /// <summary>
    /// How many debuffs to apply depending on the number of witnesses. (I think)
    /// </summary>
    [DataField]
    public Vector2 HauntDebuffs = new(2, 6);

    /// <summary>
    /// How much Wp you gain from using the action per witness.
    /// </summary>
    [DataField]
    public FixedPoint2 HauntStolenWpPerWitness = 2.5;

    /// <summary>
    /// How much the Wp regeneration gets boosted per witness.
    /// </summary>
    [DataField]
    public FixedPoint2 HauntWpRegenPerWitness = 0.5;

    /// <summary>
    /// How long the Wp regen boost lasts.
    /// </summary>
    [DataField]
    public TimeSpan HauntWpRegenDuration = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Sounds to be played whwn someone gets haunted.
    /// </summary>
    [DataField]
    public SoundSpecifier? HauntSound = new SoundCollectionSpecifier("RevenantHaunt");

    /// <summary>
    /// How much the Wp regeneration gets boosted per witness.
    /// </summary>
    [DataField]
    public TimeSpan HauntCorporealDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long the flash effect lasts when someone gets haunted.
    /// </summary>
    [DataField]
    public TimeSpan HauntFlashDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The status effect to flash anyone who gets haunted.
    /// </summary>
    [DataField]
    public EntProtoId FlashedId = "Flashed";

    /// <summary>
    /// The status effect to make the Wraith corporeal upon using haunt.
    /// </summary>
    [DataField]
    public ProtoId<StatusEffectPrototype> CorporealEffect = "Corporeal";
}
