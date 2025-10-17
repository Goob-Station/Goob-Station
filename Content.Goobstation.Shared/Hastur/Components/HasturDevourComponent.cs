using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasturDevourComponent : Component
{

    [DataField]
    public string Normal = "hasturM";

    [DataField]
    public string Devouring = "hastur_devour";

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier? DevourSound = new SoundCollectionSpecifier("HasturDevour");

    /// <summary>
    /// How long the DoAfter delay before devour executes
    /// </summary>
    [DataField]
    public TimeSpan DevourDuration = TimeSpan.FromSeconds(1.7);

    /// <summary>
    /// Healing from devouring an entity.
    /// </summary>
    [DataField]
    public DamageSpecifier Healing = new();
}
