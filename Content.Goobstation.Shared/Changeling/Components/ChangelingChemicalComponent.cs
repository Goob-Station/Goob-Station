using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Component used for the changeling chemical reserves.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingChemicalComponent : Component
{
    [DataField]
    public ProtoId<AlertPrototype> AlertId = "ChangelingChemicals";

    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between update cycles.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Current number of chemicals.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Chemicals;

    /// <summary>
    /// Maximum value of chemicals the changeling can have.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxChemicals = 100f;

    /// <summary>
    /// The chemical generation per cycle.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float RegenAmount = 1f;

    /// <summary>
    /// The default modifier applied to passive chemical generation.
    /// </summary>
    public float Modifier = 1f;

    /// <summary>
    /// The multiplier applied to passive chemical generation while on fire.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FireModifier = 0.25f;
}
