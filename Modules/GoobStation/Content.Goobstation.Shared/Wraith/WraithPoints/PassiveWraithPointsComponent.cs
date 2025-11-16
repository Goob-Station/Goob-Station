using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.WraithPoints;

/// <summary>
/// Allows for passive generation of WP
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(WraithPointsSystem))]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class PassiveWraithPointsComponent : Component
{
    /// <summary>
    /// The rate at which the wraith regenerates WP.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 WpGeneration = 1;

    /// <summary>
    ///  The amount of WP that gets generated passively.
    /// Gets multiplied by WpRegeneration variable
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 BaseWpGeneration = 5;

    [DataField, AutoNetworkedField]
    public FixedPoint2 CurrentWpGeneration = 5;

    /// <summary>
    /// The accumulator for passive WP generation
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public TimeSpan WpGenerationAccumulator = TimeSpan.Zero;

    /// <summary>
    ///  How often to update the passive WP generation
    /// </summary>
    [DataField]
    public TimeSpan NextWpUpdate = TimeSpan.FromSeconds(5f);

    /// <summary>
    ///  The value to reset the BaseWpGeneration under certain circumstances.
    /// Usually in wraith due to Banishment.
    /// </summary>
    [ViewVariables]
    public FixedPoint2 BaseWpResetter;
}
