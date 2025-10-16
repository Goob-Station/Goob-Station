using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasturDevourComponent : Component
{

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundSpecifier? DevourSound = new SoundCollectionSpecifier("HasturDevour");

    /// <summary>
    /// How long the DoAfter delay before devour executes
    /// </summary>
    [DataField]
    public TimeSpan DevourDuration = TimeSpan.FromSeconds(2);
}
