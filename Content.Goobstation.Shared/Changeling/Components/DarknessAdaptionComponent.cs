using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DarknessAdaptionComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionDarknessAdaption";

    [DataField]
    public EntityUid? ActionEnt;

    [DataField]
    public ProtoId<AlertPrototype> AlertId = "DarknessAdaption";

    /// <summary>
    /// To save on performance (stops ShowAlert/ClearAlert from being called over and over)
    /// </summary>
    [DataField]
    public bool AlertDisplayed;

    /// <summary>
    /// The modifier to be applied to a changeling's chemical generation multiplier
    /// </summary>
    [DataField]
    public float ChemicalModifier = 0.15f;

    /// <summary>
    /// Popup when toggled on
    /// </summary>
    [DataField]
    public LocId ActivePopup = "changeling-darkadapt-active";

    /// <summary>
    /// Popup when toggled off
    /// </summary>
    [DataField]
    public LocId InactivePopup = "changeling-darkadapt-inactive";

    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between update cycles.
    /// </summary>
    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Is the ability currently active?
    /// </summary>
    [DataField]
    public bool Active;

    /// <summary>
    /// Is the darkness being adapted to?
    /// </summary>
    [DataField]
    public bool Adapting;

    /// <summary>
    /// The visibility to be set for the stealth component.
    /// </summary>
    [DataField]
    public float Visibility = 0.2f;

    /// <summary>
    /// True if the entity had LightDetectionComponent beforehand for any reason.
    /// </summary>
    [DataField]
    public bool HadLightDetection;
}
