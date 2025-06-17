using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Nail;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NailComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new ();

    [DataField]
    public DamageSpecifier DamageToWhitelisted = new ();

    [DataField]
    public EntityWhitelist Whitelist = new ();

    [DataField]
    public bool AutoHammerIntoNonWhitelisted;

    [DataField]
    public TargetBodyPart? ForceBodyPart = TargetBodyPart.Chest;

    [DataField, AutoNetworkedField]
    public bool ShotFromNailgun;
}
