using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.FadingTimedDespawn;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FadingTimedDespawnComponent : Component
{
    /// <summary>
    /// How long the entity will exist before despawning
    /// </summary>
    [DataField]
    public float Lifetime = 5f;

    /// <summary>
    /// If it is above zero, entity will fade out slowly when despawning
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FadeOutTime = 1f;

    /// <summary>
    /// Whether this entity started to fade out
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public bool FadeOutStarted;

    public const string AnimationKey = "fadeout";
}
