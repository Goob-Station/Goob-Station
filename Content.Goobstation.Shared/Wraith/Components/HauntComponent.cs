using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class HauntComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionHaunt";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// How much the Wp regeneration gets boosted per witness.
    /// </summary>
    [DataField]
    public FixedPoint2 HauntWpRegenPerWitness = 0.5;

    /// <summary>
    /// How long the Wp regen boost lasts.
    /// </summary>
    [DataField]
    public TimeSpan HauntWpRegenDuration = TimeSpan.FromSeconds(60);

    [DataField, AutoNetworkedField]
    public TimeSpan NextHauntWpRegenUpdate = TimeSpan.Zero;

    /// <summary>
    /// How much the Wp regeneration gets boosted per witness.
    /// </summary>
    [DataField]
    public TimeSpan HauntCorporealDuration = TimeSpan.FromSeconds(30);

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
    public EntProtoId CorporealEffect = "Corporeal";

    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [ViewVariables, AutoNetworkedField]
    public bool WpBoostActive;

    [DataField]
    public TimeSpan HauntDuration = TimeSpan.FromSeconds(30);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextHauntUpdate;

    [DataField]
    public TimeSpan WitnessUpdate = TimeSpan.FromSeconds(0.75f);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan WitnessNextUpdate;

    [ViewVariables]
    public HashSet<EntityUid> Witnesses = new();

    [ViewVariables]
    public FixedPoint2 OriginalWpRegen;
}
