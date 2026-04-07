using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the ability to toggle incorporeal form.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SlasherIncorporealComponent : Component
{
    [ViewVariables]
    public EntityUid? IncorporealizeActionEnt;

    [ViewVariables]
    public EntityUid? CorporealizeActionEnt;

    [DataField]
    public EntProtoId IncorporealizeActionId = "ActionSlasherIncorporealize";

    [DataField]
    public EntProtoId CorporealizeActionId = "ActionSlasherCorporealize";

    /// <summary>
    /// Current state of the slasher. True when incorporeal.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsIncorporeal;

    /// <summary>
    /// Range (in tiles) to check for observers with line of sight that prevent incorporealizing.
    /// </summary>
    [DataField]
    public float ObserverCheckRange = 10f;

    /// <summary>
    /// How long the do-after to enter incorporeal form takes.
    /// </summary>
    [DataField]
    public TimeSpan IncorporealizeDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Range to disable lights around the slasher when entering incorporeal.
    /// </summary>
    [DataField]
    public float LightDisableRange = 5f;

    /// <summary>
    /// Stores the remaining cooldown time for each action when entering incorporeal state.
    /// </summary>
    [ViewVariables]
    public Dictionary<EntityUid, TimeSpan> FrozenCooldowns = new();

    /// <summary>
    /// The time when the slasher entered incorporeal state, used to calculate cooldown adjustments.
    /// </summary>
    [ViewVariables]
    public TimeSpan? IncorporealStartTime;

    /// <summary>
    /// Effect spawned when entering incorporeal state.
    /// </summary>
    [DataField]
    public EntProtoId JauntInEffect = "EffectSlasherJauntIn";

    /// <summary>
    /// Effect spawned when exiting incorporeal state.
    /// </summary>
    [DataField]
    public EntProtoId JauntOutEffect = "EffectSlasherJauntOut";

    /// <summary>
    /// Alert shown to indicate if the slasher is seen or unseen by observers.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> SeenAlert = "SlasherSeen";

    [DataField]
    public SoundSpecifier JauntAppear =
        new SoundPathSpecifier(new ResPath("/Audio/_Goobstation/Effects/Slasher/SlasherJauntAppear.ogg"));

    [DataField]
    public SoundSpecifier JauntDisappear =
        new SoundPathSpecifier(new ResPath("/Audio/_Goobstation/Effects/Slasher/SlasherJauntDisappear.ogg"));
}
