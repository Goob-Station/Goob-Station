using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Obsession;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ObsessedComponent : Component
{
    public const int PassiveSanityLoss = 4;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int TargetId = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    public string TargetName = "";

    [ViewVariables(VVAccess.ReadWrite)]
    public int MaxSanity = 100;

    [ViewVariables(VVAccess.ReadWrite)]
    private int _sanity = 100;

    public int Sanity
    {
        get => _sanity;
        set => _sanity = Math.Clamp(value, 0, MaxSanity);
    }

    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<ObsessionInteraction, int> InteractionRecovery = new()
    {
        { ObsessionInteraction.Touch, 4 },
        { ObsessionInteraction.Grab, 6 },
        { ObsessionInteraction.Hear, 2 },
        { ObsessionInteraction.Photo, 10 },
        { ObsessionInteraction.PhotoLook, 2 },
        { ObsessionInteraction.StayNear, 3 }
    };

    [ViewVariables(VVAccess.ReadWrite)]
    public List<ObsessionInteraction> LastRecoveries = new();

    [AutoNetworkedField]
    public int SanityLossStage = 0;

    public Dictionary<ObsessionInteraction, int> Interactions = new()
    {
        { ObsessionInteraction.Touch, 0 },
        { ObsessionInteraction.Grab, 0 },
        { ObsessionInteraction.Photo, 0 }
    };

    public TimeSpan NextUpdate = TimeSpan.Zero;

    public TimeSpan NextDirty = TimeSpan.Zero;
}
