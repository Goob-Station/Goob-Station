using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

[RegisterComponent, NetworkedComponent]
public sealed partial class HereticComponent : Component
{
    #region Prototypes

    [DataField] public List<EntProtoId> BaseActions = new()
    {
        "ActionHereticOpenStore",
        "ActionHereticMansusGrasp"
    };

    #endregion

    [DataField] public bool Ascended = false;

    // hardcoded paths because i hate it
    // "Ash", "Moon", "Lock", "Flesh", "Void", "Blade", "Rust", "Cosmos"
    /// <summary>
    ///     Indicates a path the heretic is on.
    /// </summary>
    [DataField] public string? CurrentPath = null;

    /// <summary>
    ///     Indicates a stage of a path the heretic is on. 0 is starting, 6 is end, 7 is Ascension
    /// </summary>
    [DataField] public int PathStage = 0;

    [DataField] public List<ProtoId<HereticRitualPrototype>> KnownRituals = new();
    [DataField] public ProtoId<HereticRitualPrototype>? ChosenRitual;

    /// <summary>
    ///     Used to prevent double casting mansus grasp.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public bool MansusGraspActive = false;

    /// <summary>
    ///     Doubles the eldritch influence if true.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public bool CodexActive = false;

    /// <summary>
    ///     Indicates if a heretic is able to cast advanced spells.
    ///     Requires wearing focus, codex cicatrix, hood or anything else that allows him to do so.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] public bool CanCastSpells = false;
}
