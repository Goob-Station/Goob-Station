using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StarBlastActionComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Projectile;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(25);

    [DataField]
    public EntProtoId Effect = "EffectCosmicCloud";

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/cosmic_energy.ogg");
}
