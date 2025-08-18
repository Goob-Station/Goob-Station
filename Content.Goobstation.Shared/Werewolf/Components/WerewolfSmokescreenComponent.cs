using Content.Shared.Chemistry.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfSmokescreenComponent : Component
{
    #region Smoke-Related
    /// <summary>
    /// How long the smoke stays for, after it has spread.
    /// </summary>
    [DataField]
    public float Duration = 5;

    /// <summary>
    /// How much the smoke will spread.
    /// </summary>
    [DataField]
    public int SpreadAmount;

    /// <summary>
    /// Smoke entity to spawn.
    /// Defaults to smoke but you can use foam if you want.
    /// </summary>
    [DataField]
    public EntProtoId SmokePrototype = "Smoke";

    /// <summary>
    /// Solution to add to smoke.
    /// </summary>
    [DataField]
    public Solution Solution = new();
    #endregion

    #region Ability-Related
    /// <summary>
    /// Whether the ability is active, or not.
    /// </summary>
    [ViewVariables]
    public bool Active;

    /// <summary>
    /// The duration of the effects
    /// </summary>
    [DataField]
    public TimeSpan EffectDuration = TimeSpan.FromSeconds(5f);

    [ViewVariables]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    /// <summary>
    ///  The status effect to apply to the werewolf.
    /// In this case, the pacified effect.
    /// </summary>
    [DataField]
    public EntProtoId Pacified = "Pacified";

    #endregion
}
