using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;


namespace Content.Goobstation.Shared.Virology;

[RegisterComponent]
public sealed partial class VirologyMachineComponent : Component
{
    [ViewVariables]
    public const string SwabSlotId = "disease_swab_slot";

    [DataField]
    public ItemSlot SwabSlot = new();

    [DataField]
    public EntProtoId PaperPrototype = "DiagnosisReportPaper";

    [DataField]
    public EntProtoId VaccinePrototype = "Vaccine";

    [DataField]
    public SoundSpecifier AnalyzedSound = new SoundPathSpecifier("/Audio/Machines/diagnoser_printing.ogg");

    [DataField]
    public SoundSpecifier AnalysisSound = new SoundPathSpecifier("/Audio/Machines/buzz_loop.ogg");

    [ViewVariables]
    public EntityUid? SoundEntity;

    [DataField, ViewVariables]
    public TimeSpan AnalysisDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public string? IdleState;

    [DataField]
    public string? RunningState;

    // is this machine a vaccinator or analyzer?
    // holy fuck goida
    [DataField]
    public bool Vaccinator;

    // vaccine or live injector mode?
    [DataField]
    public bool InjectorMode;
}
