using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasturDevourComponent : Component
{

    [DataField(required: true)]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(3);

    [DataField]
    public SoundSpecifier? DevourSound = new SoundCollectionSpecifier("HasturDevour");
}
