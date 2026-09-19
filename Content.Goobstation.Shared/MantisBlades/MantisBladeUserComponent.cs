using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MantisBlades;

/// <summary>
/// Marks a body with at least one mantis blade arm attached.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MantisBladeUserComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Extended;

    [DataField, AutoNetworkedField]
    public TimeSpan ExtendedAt;

    [DataField, AutoNetworkedField]
    public List<EntityUid> Blades = new();

    [DataField]
    public bool Popped;

    [DataField]
    public TimeSpan PopTime = TimeSpan.FromSeconds(0.2);

    [DataField]
    public ResPath Rsi = new("_Goobstation/Objects/Weapons/Melee/mantis_blade.rsi");

    #region Tuning

    /// <summary>
    /// This is applied when using another weapon while having the mantis blades out.
    /// </summary>
    [DataField]
    public float ArmedMultiplier = 0.5f;

    #endregion

    [DataField]
    public EntProtoId Action = "ActionToggleMantisBlades";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public SoundSpecifier? ExtendSound = new SoundPathSpecifier("/Audio/_Goobstation/Weapons/MantisBlades/mantis_extend.ogg")
    {
        Params = AudioParams.Default.WithMaxDistance(4f),
    };

    [DataField]
    public SoundSpecifier? RetractSound = new SoundCollectionSpecifier("MantisBladeRetract")
    {
        Params = AudioParams.Default.WithMaxDistance(4f),
    };
}

public sealed partial class ToggleMantisBladesActionEvent : InstantActionEvent;
