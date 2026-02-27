using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Trauma.Shared.ShadowDemon.ShadowCocoon;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowCocoonComponent : Component
{
    /// <summary>
    ///  This should be the overall radius of the area the Shadow Cocoon occupies
    /// </summary>
    [DataField]
    public float Radius = 4f;

    /// <summary>
    /// How often to update this entity for lookups
    /// </summary>
    [DataField]
    public TimeSpan Update = TimeSpan.FromSeconds(2);

    [DataField, AutoNetworkedField]
    public TimeSpan NextUpdate;

    /// <summary>
    /// Random sounds that play when you turn on hallucinations
    /// </summary>
    [DataField]
    public SoundCollectionSpecifier RandomSounds;

    /// <summary>
    /// If the cocoon will play random sounds or not
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Silent = true;
}
