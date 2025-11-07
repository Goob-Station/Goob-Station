using Content.Shared.Polymorph;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Abilities.Basic;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class WerewolfBasicAbilitiesComponent : Component
{
    [DataField] public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg"); // todo
    [DataField] public SoundSpecifier DistantSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg"); // todo
    [DataField] public float ShriekPower = 2.5f;

    public readonly List<EntProtoId> BaseWerewolfActions = new()
    {
        "ActionWerewolfTransfurm",
        "ActionWerewolfOpenMutationStore",
        "ActionWerewolfHowl",
        "ActionWerewolfAbsorb"
    };

    // public readonly List<EntProtoId> TransfurmedActions = new()
    // {
    // };
    // public readonly List<EntityUid?> TransfurmedActionsUid = new();

    [DataField]
    public int StunDuration = 1;

    [DataField, AutoNetworkedField]
    public bool Transfurmed = false;

    [DataField]
    public bool StoreOpened = true; // todo ungoida it, tie it to the mind and not the body you chud i fucking hate you future me raagh

    [ViewVariables]
    public ProtoId<PolymorphPrototype> CurrentMutation; //"WerewolfTransformBasic"

    [DataField] // amount of points given per devour of a guy
    public int Amount = 3;
}
