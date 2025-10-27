using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CorticalBorer.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class CorticalBorerComponent : Component
{
    /// <summary>
    /// Host of this Borer
    /// </summary>
    [ViewVariables]
    public EntityUid? Host = null;

    /// <summary>
    /// Current number of chemical points this Borer has, used to level up and buy chems
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public int ChemicalPoints;

    /// <summary>
    /// Chemicals added every second WHILE IN A HOST
    /// </summary>
    [DataField]
    public int ChemicalGenerationRate = 1;

    /// <summary>
    /// Max Chemicals that can be held
    /// </summary>
    [DataField]
    public int ChemicalPointCap = 250;

    /// <summary>
    /// Reagent injection amount
    /// </summary>
    public int InjectAmount = 10;

    /// <summary>
    /// At what interval does the chem ui update
    /// </summary>
    public int UiUpdateInterval = 5; // every 6 to prevent constant update on cap

    /// <summary>
    /// The max duration you can take control of your host
    /// </summary>
    [DataField]
    public TimeSpan ControlDuration = TimeSpan.FromSeconds(40);

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan UpdateTimer = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateCooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Can this borer make more
    /// </summary>
    [DataField]
    public bool CanReproduce = true;

    /// <summary>
    /// What does it vomit out of its mouth when it lays an egg
    /// </summary>
    [DataField]
    public string EggProto = "CorticalBorerEgg";

    [DataField]
    public bool ControlingHost;

    [DataField]
    public ComponentRegistry? AddOnInfest;

    [DataField]
    public ComponentRegistry? RemoveOnInfest;

    [DataField]
    public ProtoId<AlertPrototype> ChemicalAlert = "Chemicals";

    [DataField]
    public ProtoId<CollectiveMindPrototype> HivemindChannel = "CorticalBorer";

    [DataField]
    public EntProtoId StatusEffectProto = "CorticalBorerProtection";

    [DataField]
    public EntProtoId EndControlAction = "ActionEndControlHost";

    [DataField]
    public EntProtoId LayEggAction = "ActionLayEggHost";
}
